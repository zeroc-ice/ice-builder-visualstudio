// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using MSProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder
{
    class ProjectConverter
    {
        public static void TryUpgrade(List<IVsProject> projects)
        {
            Dictionary<string, IVsProject> upgradeProjects = new Dictionary<string, IVsProject>();
            foreach (IVsProject project in projects)
            {
                IceBuilderProjectType projectType = IsIceBuilderEnabled(project);
                if (projectType != IceBuilderProjectType.None)
                {
                    upgradeProjects.Add(project.GetDTEProject().UniqueName, project);
                }
            }

            if (upgradeProjects.Count > 0)
            {
                UpgradeDialog dialog = new UpgradeDialog();
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.Projects = upgradeProjects;
                dialog.ShowDialog();
            }
        }

        public static void Upgrade(Dictionary<string, IVsProject> projects, UpgradeProgressCallback progressCallback)
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Thread t = new Thread(() =>
            {
                try
                {
                    var NuGet = Package.Instance.NuGet;
                    if (!NuGet.IsUserConsentGranted())
                    {
                        Package.WriteMessage("Ice Builder cannot download the required NuGet packages because " +
                            "\"Allow NuGet to download missing packages\" is disabled");
                        dispatcher.Invoke(new Action(() => progressCallback.Finished()));
                        return;
                    }

                    var DTE = Package.Instance.DTE;
                    var builder = Package.Instance;
                    int i = 0;
                    int total = projects.Count;
                    foreach(var entry in projects)
                    {
                        var project = entry.Value;
                        var dteProject = project.GetDTEProject();
                        var uniqueName = dteProject.UniqueName;
                        var name = entry.Key;
                        i++;
                        if(progressCallback.Canceled)
                        {
                            break;
                        }

                        dispatcher.Invoke(
                            new Action(() =>
                            {
                                progressCallback.ReportProgress(name, i);
                            }));

                        NuGet.Restore(dteProject);
                        if(!NuGet.IsPackageInstalled(dteProject, Package.NuGetBuilderPackageId))
                        {
                            var retry = 5;
                            while(true)
                            {
                                try
                                {
                                    DTE.StatusBar.Text = string.Format("Installing NuGet package {0} in project {1}",
                                                                       Package.NuGetBuilderPackageId, name);
                                    NuGet.InstallLatestPackage(dteProject, Package.NuGetBuilderPackageId);
                                    break;
                                }
                                catch(Exception ex)
                                {
                                    retry--;
                                    if (retry == 4)
                                    {
                                        Package.WriteMessage(
                                            "NuGet package zeroc.icebuilder.msbuild install failed, retrying .");
                                    }
                                    else if(retry >= 0)
                                    {
                                        Package.WriteMessage(".");
                                    }
                                    else
                                    {
                                        Package.WriteMessage(
                                            string.Format("\nNuGet package zeroc.icebuilder.msbuild install failed:\n{0}\n", ex.Message));
                                        dispatcher.Invoke(new Action(() => progressCallback.Finished()));
                                        return;
                                    }
                                }
                            }
                        }

                        IceBuilderProjectType projectType = IsIceBuilderEnabled(project);
                        if(projectType != IceBuilderProjectType.None)
                        {
                            bool cpp = projectType == IceBuilderProjectType.CppProjectType;
                            DTE.StatusBar.Text = string.Format("Upgrading project {0} Ice Builder settings", project.GetDTEProject().UniqueName);

                            var fullPath = project.GetProjectFullPath();
                            var assemblyDir = project.GetEvaluatedProperty("IceAssembliesDir");
                            var outputDir = project.GetEvaluatedProperty("IceBuilderOutputDir");

                            var outputDirUnevaluated = project.GetPropertyWithDefault("IceBuilderOutputDir", "generated");
                            var sourceExt = project.GetPropertyWithDefault("IceBuilderSourceExt", "cpp");
                            var headerOutputDirUnevaluated = project.GetProperty("IceBuilderHeaderOutputDir");
                            var headerExt = project.GetPropertyWithDefault("IceBuilderHeaderExt", "h");

                            var cppOutputDir = new List<string>();
                            var cppHeaderOutputDir = new List<string>();
                            if(cpp)
                            {
                                foreach(EnvDTE.Configuration configuration in dteProject.ConfigurationManager)
                                {
                                    cppOutputDir.Add(Package.Instance.VCUtil.Evaluate(configuration, outputDirUnevaluated));
                                    if(string.IsNullOrEmpty(headerOutputDirUnevaluated))
                                    {
                                        cppHeaderOutputDir.Add(Package.Instance.VCUtil.Evaluate(configuration, outputDirUnevaluated));
                                    }
                                    else
                                    {
                                        cppHeaderOutputDir.Add(Package.Instance.VCUtil.Evaluate(configuration, headerOutputDirUnevaluated));
                                    }
                                }
                            }
                            else
                            {
                                foreach(VSLangProj80.Reference3 r in dteProject.GetProjectRererences())
                                {
                                    if(Package.AssemblyNames.Contains(r.Name))
                                    {
                                        project.UpdateProject((MSProject msproject) =>
                                            {
                                                var item = msproject.AllEvaluatedItems.FirstOrDefault(referenceItem =>
                                                                referenceItem.ItemType.Equals("Reference") &&
                                                                referenceItem.EvaluatedInclude.Split(",".ToCharArray()).ElementAt(0).Equals(r.Name));
                                                if(item != null)
                                                {
                                                    if(item.HasMetadata("HintPath"))
                                                    {
                                                        var hintPath = item.GetMetadata("HintPath").UnevaluatedValue;
                                                        if(hintPath.Contains("$(IceAssembliesDir)"))
                                                        {
                                                            hintPath = hintPath.Replace("$(IceAssembliesDir)",
                                                                FileUtil.RelativePath(Path.GetDirectoryName(r.ContainingProject.FullName), assemblyDir));
                                                            //
                                                            // If the HintPath points to the NuGet zeroc.ice.net package we upgrade it to not
                                                            // use IceAssembliesDir otherwise we remove it
                                                            if(hintPath.Contains("packages\\zeroc.ice.net"))
                                                            {
                                                                item.SetMetadataValue("HintPath", hintPath);
                                                            }
                                                            else
                                                            {
                                                                item.RemoveMetadata("HintPath");
                                                            }
                                                        }
                                                    }
                                                }
                                            });
                                    }
                                }
                            }

                            project.UpdateProject((MSProject msproject) =>
                            {
                                MSBuildUtils.UpgradeProjectImports(msproject);
                                MSBuildUtils.UpgradeProjectProperties(msproject, cpp);
                                MSBuildUtils.RemoveIceBuilderFromProject(msproject, true);
                                MSBuildUtils.UpgradeProjectItems(msproject);
                                MSBuildUtils.UpgradeCSharpGeneratedItems(msproject, outputDir);
                                foreach(var d in cppOutputDir)
                                {
                                    MSBuildUtils.UpgradeGeneratedItems(msproject, d, sourceExt, "ClCompile");
                                }

                                foreach(var d in cppHeaderOutputDir)
                                {
                                    MSBuildUtils.UpgradeGeneratedItems(msproject, d, headerExt, "ClInclude");
                                }

                                var propertyGroup = msproject.Xml.PropertyGroups.FirstOrDefault(group => group.Label.Equals("IceBuilder"));
                                if(propertyGroup != null)
                                {
                                    propertyGroup.Parent.RemoveChild(propertyGroup);
                                }

                                if(cpp)
                                {
                                    propertyGroup = msproject.Xml.AddPropertyGroup();
                                    propertyGroup.Label = "IceBuilder";
                                    propertyGroup.AddProperty("IceCppMapping", "cpp98");
                                }
                            });

                            builder.ReloadProject(project);
                        }
                    }
                    dispatcher.BeginInvoke(new Action(() => progressCallback.Finished()));
                }
                catch(Exception ex)
                {
                    dispatcher.BeginInvoke(new Action(() => progressCallback.Canceled = true));
                    Package.UnexpectedExceptionWarning(ex);
                }
            });
            t.Start();
        }

        //
        // Check if IceBuilder 4.x is enabled
        //
        public static IceBuilderProjectType IsIceBuilderEnabled(IVsProject project)
        {
            if(project != null)
            {
                IceBuilderProjectType type = project.IsCppProject() ? IceBuilderProjectType.CppProjectType :
                                             project.IsCSharpProject() ? IceBuilderProjectType.CsharpProjectType : IceBuilderProjectType.None;
                if(type != IceBuilderProjectType.None)
                {
                    return project.WithProject((MSProject msproject) =>
                        {
                            if(MSBuildUtils.IsIceBuilderEnabled(msproject))
                            {
                                return type;
                            }
                            return IceBuilderProjectType.None;
                        });
                }
            }
            return IceBuilderProjectType.None;
        }
    }
}
