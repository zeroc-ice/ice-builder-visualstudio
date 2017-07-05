// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
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
        //
        // Property names used to persist project configuration in old project versions.
        //
        public const string PropertyIce = "ZerocIce_Enabled";
        public const string PropertyIceOutputDir = "ZerocIce_OutputDir";
        public const string PropertyIceHeaderExt = "ZerocIce_HeaderExt";
        public const string PropertyIceSourceExt = "ZerocIce_SourceExt";
        public const string PropertyIceComponents = "ZerocIce_Components";
        public const string PropertyIceExtraOptions = "ZerocIce_ExtraOptions";
        public const string PropertyIceIncludePath = "ZerocIce_IncludePath";
        public const string PropertyIceStreaming = "ZerocIce_Streaming";
        public const string PropertyIceChecksum = "ZerocIce_Checksum";
        public const string PropertyIceTie = "ZerocIce_Tie";
        public const string PropertyIcePrefix = "ZerocIce_Prefix";
        public const string PropertyIceDllExport = "ZerocIce_DllExport";
        public const string PropertyVerboseLevel = "ZerocIce_VerboseLevel";
        public const string PropertyProjectVersion = "ZerocIce_ProjectVersion";

        public static string[] OldPropertyNames = new string[]
        {
            PropertyIce,
            PropertyIceOutputDir,
            PropertyIceHeaderExt,
            PropertyIceSourceExt,
            PropertyIceComponents,
            PropertyIceExtraOptions,
            PropertyIceIncludePath,
            PropertyIceStreaming,
            PropertyIceChecksum,
            PropertyIceTie,
            PropertyIcePrefix,
            PropertyIceDllExport,
            PropertyVerboseLevel,
            PropertyProjectVersion
        };

        class OldConfiguration
        {
            public OldConfiguration()
            {
                Enabled = false;
                OutputDir = string.Empty;
                HeaderExt = string.Empty;
                SourceExt = string.Empty;
                AdditionalOptions = string.Empty;
                IncludeDirectories = string.Empty;
                Ice = false;
                Stream = false;
                Checksum = false;
                Tie = false;
                DLLExport = string.Empty;
            }

            public bool Enabled
            {
                get;
                set;
            }

            public string OutputDir
            {
                get;
                set;
            }

            public string HeaderExt
            {
                get;
                set;
            }

            public string SourceExt
            {
                get;
                set;
            }

            public string AdditionalOptions
            {
                get;
                set;
            }

            public string IncludeDirectories
            {
                get;
                set;
            }

            public bool Ice
            {
                get;
                set;
            }

            public bool Stream
            {
                get;
                set;
            }

            public bool Checksum
            {
                get;
                set;
            }

            public bool Tie
            {
                get;
                set;
            }

            public string DLLExport
            {
                get;
                set;
            }

            public bool Load(Microsoft.Build.Evaluation.Project project, bool remove)
            {
                bool loaded = false;
                foreach(ProjectElement element in project.Xml.Children)
                {
                    ProjectExtensionsElement extension = element as ProjectExtensionsElement;
                    if(extension != null && !string.IsNullOrEmpty(extension.Content))
                    {
                        try
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(extension.Content);
                            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                            ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");
                            XmlNode userProperties = doc.DocumentElement.SelectSingleNode("/msb:VisualStudio/msb:UserProperties", ns);
                            if(userProperties != null)
                            {
                                if(userProperties.Attributes[PropertyIce] != null)
                                {
                                    loaded = true;
                                    Enabled = userProperties.Attributes[PropertyIce].Value.Equals("True");
                                }

                                if(userProperties.Attributes[PropertyIceOutputDir] != null)
                                {
                                    loaded = true;
                                    OutputDir = userProperties.Attributes[PropertyIceOutputDir].Value;
                                }

                                if(userProperties.Attributes[PropertyIceHeaderExt] != null)
                                {
                                    loaded = true;
                                    HeaderExt = userProperties.Attributes[PropertyIceHeaderExt].Value;
                                }

                                if(userProperties.Attributes[PropertyIceSourceExt] != null)
                                {
                                    loaded = true;
                                    SourceExt = userProperties.Attributes[PropertyIceSourceExt].Value;
                                }

                                if(userProperties.Attributes[PropertyIceExtraOptions] != null)
                                {
                                    loaded = true;
                                    AdditionalOptions = userProperties.Attributes[PropertyIceExtraOptions].Value;
                                }

                                if(userProperties.Attributes[PropertyIceIncludePath] != null)
                                {
                                    loaded = true;
                                    IncludeDirectories = userProperties.Attributes[PropertyIceIncludePath].Value;
                                }

                                if(userProperties.Attributes[PropertyIceStreaming] != null)
                                {
                                    loaded = true;
                                    Stream = userProperties.Attributes[PropertyIceStreaming].Value.Equals("True");
                                }

                                if(userProperties.Attributes[PropertyIceChecksum] != null)
                                {
                                    loaded = true;
                                    Checksum = userProperties.Attributes[PropertyIceChecksum].Value.Equals("True");
                                }

                                if(userProperties.Attributes[PropertyIceTie] != null)
                                {
                                    loaded = true;
                                    Tie = userProperties.Attributes[PropertyIceTie].Value.Equals("True");
                                }

                                if(userProperties.Attributes[PropertyIcePrefix] != null)
                                {
                                    loaded = true;
                                    Ice = userProperties.Attributes[PropertyIcePrefix].Value.Equals("True");
                                }

                                if(userProperties.Attributes[PropertyIceDllExport] != null)
                                {
                                    loaded = true;
                                    DLLExport = userProperties.Attributes[PropertyIceDllExport].Value;
                                }

                                if(remove)
                                {
                                    foreach(string name in OldPropertyNames)
                                    {
                                        if(userProperties.Attributes[name] != null)
                                        {
                                            userProperties.Attributes.Remove(userProperties.Attributes[name]);
                                        }
                                    }

                                    if(userProperties.Attributes.Count == 0)
                                    {
                                        project.Xml.RemoveChild(extension);
                                    }
                                }
                                break;
                            }
                        }
                        catch(XmlException)
                        {
                        }
                    }
                }
                return loaded;
            }
        }

        public static void TryUpgrade(List<IVsProject> projects)
        {
            string baseDir = string.Empty;
            if(!string.IsNullOrEmpty(Package.Instance.DTE2.Solution.FullName))
            {
                baseDir = Path.GetDirectoryName(Package.Instance.DTE2.Solution.FullName);
            }
            Dictionary<string, IVsProject> upgradeProjects = new Dictionary<string, IVsProject>();
            foreach(IVsProject project in projects)
            {
                if(DTEUtil.IsCppProject(project) || DTEUtil.IsCSharpProject(project))
                {
                    string fullName = ProjectUtil.GetProjectFullPath(project);
                    if(new OldConfiguration().Load(MSBuildUtils.LoadedProject(fullName, DTEUtil.IsCppProject(project), true), false))
                    {
                        upgradeProjects.Add(FileUtil.RelativePath(baseDir, fullName), project);
                    }
                }
            }

            if(upgradeProjects.Count > 0)
            {
                UpgradeDialog dialog = new UpgradeDialog();
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.Projects = upgradeProjects;
                dialog.ShowDialog();
            }
        }

        public static void Upgrade(List<IVsProject> projects, UpgradeProgressCallback progressCallback)
        {
            string solutionDir = Path.GetDirectoryName(Package.Instance.DTE2.Solution.FullName);
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Thread t = new Thread(() =>
            {
                for(int i = 0; i < projects.Count; ++i)
                {
                    if(progressCallback.Canceled)
                    {
                        break;
                    }

                    IVsProject project = projects[i];
                    string projectName = FileUtil.RelativePath(solutionDir, ProjectUtil.GetProjectFullPath(project));
                    if(Upgrade(project))
                    {
                        EnvDTE.Project p = DTEUtil.GetProject(project as IVsHierarchy);
                        if(p.Globals != null)
                        {
                            foreach(string name in OldPropertyNames)
                            {
                                if(p.Globals.get_VariableExists(name))
                                {
                                    p.Globals[name] = string.Empty;
                                    p.Globals.set_VariablePersists(name, false);
                                }
                            }
                        }
                        Package.Instance.SaveProject(project, MSBuildUtils.LoadedProject(ProjectUtil.GetProjectFullPath(project), DTEUtil.IsCppProject(project), true));
                    }
                    dispatcher.Invoke(
                        new Action(() =>
                        {
                            progressCallback.ReportProgress(projectName, i);
                        }));
                }
                dispatcher.BeginInvoke(new Action(() => progressCallback.Finished()));
            });
            t.Start();
        }

        public static bool Upgrade(IVsProject project)
        {
            OldConfiguration oldConfiguration = new OldConfiguration();
            var fullPath = ProjectUtil.GetProjectFullPath(project);
            Microsoft.Build.Evaluation.Project msbuildProject = MSBuildUtils.LoadedProject(fullPath, DTEUtil.IsCppProject(project), true);
            DTEUtil.EnsureFileIsCheckout(fullPath);
            if(oldConfiguration.Load(msbuildProject, true))
            {
                if(DTEUtil.IsCppProject(project))
                {
                    return UpgadeCppConfiguration(msbuildProject, oldConfiguration);
                }
                else if(DTEUtil.IsCSharpProject(project))
                {
                    return UpgradeCSharpConfiguration(DTEUtil.GetProject(project as IVsHierarchy), msbuildProject, oldConfiguration);
                }
            }
            return false;
        }

        private static bool UpgradeCSharpConfiguration(EnvDTE.Project dteProject,
            Microsoft.Build.Evaluation.Project project, OldConfiguration cfg)
        {
            ProjectPropertyGroupElement propertyGroup = project.Xml.PropertyGroups.FirstOrDefault(g => g.Label.Equals("IceBuilder"));
            if(propertyGroup == null)
            {
                propertyGroup = project.Xml.AddPropertyGroup();
                propertyGroup.Label = "IceBuilder";
            }

            if(!string.IsNullOrEmpty(cfg.OutputDir))
            {
                propertyGroup.AddProperty(PropertyNames.OutputDir, cfg.OutputDir);
            }

            if(!string.IsNullOrEmpty(cfg.AdditionalOptions))
            {
                propertyGroup.AddProperty(PropertyNames.AdditionalOptions, cfg.AdditionalOptions);
            }

            if(!string.IsNullOrEmpty(cfg.IncludeDirectories))
            {
                propertyGroup.AddProperty(PropertyNames.IncludeDirectories,
                    string.Format(@"{0};$(IceHome)\slice", cfg.IncludeDirectories));
            }
            else
            {
                propertyGroup.AddProperty(PropertyNames.IncludeDirectories, @"$(IceHome)\slice");
            }

            if(cfg.Stream)
            {
                propertyGroup.AddProperty(PropertyNames.Stream, "True");
            }

            if(cfg.Checksum)
            {
                propertyGroup.AddProperty(PropertyNames.Checksum, "True");
            }

            if(cfg.Ice)
            {
                propertyGroup.AddProperty(PropertyNames.AllowIcePrefix, "True");
            }

            if(cfg.Tie)
            {
                propertyGroup.AddProperty(PropertyNames.Tie, "True");
            }

            foreach(string assembly in Package.AssemblyNames)
            {
                VSLangProj80.Reference3 reference = ProjectUtil.FindAssemblyReference(dteProject, assembly) as VSLangProj80.Reference3;
                if(reference != null)
                {
                    reference.SpecificVersion = false;
                }
            }

            List<ProjectItem> sliceItems =
                project.GetItems("None").Where(item => Path.GetExtension(item.UnevaluatedInclude).Equals(".ice")).ToList();

            //
            // Default output directory has changed
            //
            if(string.IsNullOrEmpty(cfg.OutputDir))
            {
                project.GetItems("Compile").Where(
                    item =>
                    {
                        return sliceItems.FirstOrDefault(
                            slice =>
                            {
                                return slice.UnevaluatedInclude.Equals(Path.ChangeExtension(item.UnevaluatedInclude, ".ice"));
                            }) != null;
                    })
                .ToList()
                .ForEach(item => project.RemoveItem(item));
            }

            return true;
        }

        private static bool UpgadeCppConfiguration(Microsoft.Build.Evaluation.Project project, OldConfiguration cfg)
        {
            ProjectPropertyGroupElement propertyGroup = project.Xml.PropertyGroups.FirstOrDefault(g => g.Label.Equals("IceBuilder"));
            if(propertyGroup == null)
            {
                propertyGroup = project.Xml.AddPropertyGroup();
                propertyGroup.Label = "IceBuilder";
            }

            if(!string.IsNullOrEmpty(cfg.OutputDir))
            {
                propertyGroup.AddProperty(PropertyNames.OutputDir, cfg.OutputDir);
            }

            if(!string.IsNullOrEmpty(cfg.HeaderExt))
            {
                propertyGroup.AddProperty(PropertyNames.HeaderExt, cfg.HeaderExt);
            }

            if(!string.IsNullOrEmpty(cfg.SourceExt))
            {
                propertyGroup.AddProperty(PropertyNames.SourceExt, cfg.SourceExt);
            }

            if(!string.IsNullOrEmpty(cfg.AdditionalOptions))
            {
                propertyGroup.AddProperty(PropertyNames.AdditionalOptions, cfg.AdditionalOptions);
            }

            if(!string.IsNullOrEmpty(cfg.IncludeDirectories))
            {
                propertyGroup.AddProperty(PropertyNames.IncludeDirectories,
                    string.Format("{0};$({1})", cfg.IncludeDirectories, PropertyNames.IncludeDirectories));
            }

            if(cfg.Stream)
            {
                propertyGroup.AddProperty(PropertyNames.Stream, "True");
            }

            if(cfg.Checksum)
            {
                propertyGroup.AddProperty(PropertyNames.Checksum, "True");
            }

            if(cfg.Ice)
            {
                propertyGroup.AddProperty(PropertyNames.AllowIcePrefix, "True");
            }

            if(!string.IsNullOrEmpty(cfg.DLLExport))
            {
                propertyGroup.AddProperty(PropertyNames.DLLExport, cfg.DLLExport);
            }

            foreach(ProjectItemDefinitionGroupElement group in project.Xml.ItemDefinitionGroups)
            {
                //
                // Remove old property sheet from all configurations
                //
                IEnumerable<ProjectImportElement> imports = project.Xml.Imports.Where(
                    p => p.Project.Equals("$(ALLUSERSPROFILE)\\ZeroC\\Ice.props", StringComparison.CurrentCultureIgnoreCase));

                if(imports != null)
                {
                    foreach(ProjectImportElement import in imports)
                    {
                        import.Parent.RemoveChild(import);
                    }
                }
                //
                // WinRT SDK old property sheet
                //
                imports = project.Xml.Imports.Where(
                    p => p.Project.IndexOf("CommonConfiguration\\Neutral\\Ice.props", StringComparison.CurrentCultureIgnoreCase) != -1);
                if(imports != null)
                {
                    foreach(ProjectImportElement import in imports)
                    {
                        import.Parent.RemoveChild(import);
                    }
                }

                foreach(ProjectItemDefinitionElement i in group.ItemDefinitions)
                {
                    if(i.ItemType.Equals("ClCompile"))
                    {
                        if(!string.IsNullOrEmpty(cfg.OutputDir))
                        {
                            ProjectMetadataElement metaData = i.Metadata.FirstOrDefault(
                                e => e.Name.Equals("AdditionalIncludeDirectories"));

                            if(metaData != null)
                            {
                                List<string> values = new List<string>(metaData.Value.Split(new char[] { ';' }));
                                values.Remove(cfg.OutputDir);
                                metaData.Value = string.Join(";", values);

                                if(values.Count == 0 ||
                                    (values.Count == 1 && values[0].Equals("%(AdditionalIncludeDirectories)")))
                                {
                                    i.RemoveChild(metaData);
                                }
                                else
                                {
                                    if(!values.Contains("%(AdditionalIncludeDirectories)"))
                                    {
                                        values.Add("%(AdditionalIncludeDirectories)");
                                    }
                                    metaData.Value = string.Join(";", values);
                                }
                            }
                        }
                    }
                    else if(i.ItemType.Equals("Link"))
                    {
                        ProjectMetadataElement metaData = i.Metadata.FirstOrDefault(
                                e => e.Name.Equals("AdditionalDependencies"));

                        if(metaData != null)
                        {
                            List<string> values = new List<string>(metaData.Value.Split(new char[] { ';' }));
                            foreach(string name in Package.CppLibNames)
                            {
                                values.Remove(string.Format("{0}.lib", name));
                                values.Remove(string.Format("{0}d.lib", name));
                            }

                            if(values.Count == 0 || (values.Count == 1 && values[0].Equals("%(AdditionalDependencies)")))
                            {
                                i.RemoveChild(metaData);
                            }
                            else
                            {
                                metaData.Value = string.Join(";", values);
                            }
                        }

                        metaData = i.Metadata.FirstOrDefault(e => e.Name.Equals("AdditionalLibraryDirectories"));
                        if(metaData != null)
                        {
                            if(metaData.Value.Equals("%(AdditionalLibraryDirectories)"))
                            {
                                i.RemoveChild(metaData);
                            }
                        }
                    }
                }
            }

            List<ProjectItem> sliceItems =
                project.GetItems("None").Where(item => Path.GetExtension(item.UnevaluatedInclude).Equals(".ice")).ToList();

            //
            // Default output directory has changed
            //
            if(string.IsNullOrEmpty(cfg.OutputDir))
            {
                foreach(string itemType in new string[] { "ClInclude", "ClCompile" })
                {
                    project.GetItems(itemType).Where(
                        item =>
                        {
                            return sliceItems.FirstOrDefault(
                                slice =>
                                {
                                    return slice.UnevaluatedInclude.Equals(Path.ChangeExtension(item.UnevaluatedInclude, ".ice"));
                                }) != null;
                        })
                    .ToList()
                    .ForEach(item => project.RemoveItem(item));
                }
            }

            sliceItems.ForEach(item =>
            {
                project.RemoveItem(item);
                project.AddItem("IceBuilder", item.UnevaluatedInclude);
            });
            return true;
        }
    }
}
