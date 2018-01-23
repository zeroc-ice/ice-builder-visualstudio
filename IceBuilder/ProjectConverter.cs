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

using Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.Shell.Interop;

using System.Xml;

namespace IceBuilder
{
    class ProjectConverter
    {
        public static void TryUpgrade(List<IVsProject> projects)
        {
            Dictionary<string, IVsProject> upgradeProjects = new Dictionary<string, IVsProject>();
            foreach (IVsProject project in projects)
            {
                IceBuilderProjectType projectType = DTEUtil.IsIceBuilderEnabled(project);
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
                var NuGet = Package.Instance.NuGet;
                var DTE = Package.Instance.DTE;
                var builder = Package.Instance;
                int i = 0;
                int total = projects.Count;
                foreach(var entry in projects)
                {
                    var project = entry.Value;
                    var dteProject = project.GetDTEProject();
                    var name = entry.Key;
                    i++;
                    if (progressCallback.Canceled)
                    {
                        break;
                    }

                    dispatcher.Invoke(
                        new Action(() =>
                        {
                            progressCallback.ReportProgress(name, i);
                        }));

                    NuGet.Restore(dteProject);
                    if (!NuGet.IsPackageInstalled(dteProject, Package.NuGetBuilderPackageId))
                    {
                        DTE.StatusBar.Text = string.Format("Installing NuGet package {0} in project {1}",
                            Package.NuGetBuilderPackageId, name);
                        NuGet.InstallLatestPackage(dteProject, Package.NuGetBuilderPackageId);
                    }

                    IceBuilderProjectType projectType = DTEUtil.IsIceBuilderEnabled(project);
                    if (projectType != IceBuilderProjectType.None)
                    {
                        DTE.StatusBar.Text = string.Format("Upgrading project {0} Ice Builder settings", project.GetDTEProject().UniqueName);
                        var fullPath = ProjectUtil.GetProjectFullPath(project);
                        var assemblyDir = ProjectUtil.GetEvaluatedProperty(project, "IceAssembliesDir");
                        if (projectType == IceBuilderProjectType.CsharpProjectType)
                        {
                            ProjectUtil.UpgradReferencesHintPath(DTEUtil.GetProject(project as IVsHierarchy), assemblyDir);
                        }
                        Microsoft.Build.Evaluation.Project p = MSBuildUtils.LoadedProject(
                                    ProjectUtil.GetProjectFullPath(project), projectType == IceBuilderProjectType.CppProjectType, false);
                        bool modified = MSBuildUtils.UpgradeProjectImports(p);
                        modified = MSBuildUtils.UpgradeProjectProperties(p) || modified;
                        modified = MSBuildUtils.RemoveIceBuilderFromProject(p, true) || modified;
                        modified = MSBuildUtils.UpgradeProjectItems(p, projectType == IceBuilderProjectType.CppProjectType) || modified;

                        if (modified)
                        {
                            builder.SaveProject(project, p);
                        }
                    }
                }
                dispatcher.BeginInvoke(new Action(() => progressCallback.Finished()));
            });
            t.Start();
        }
    }
}
