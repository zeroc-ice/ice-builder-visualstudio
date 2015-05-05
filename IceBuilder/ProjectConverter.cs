// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

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

        private static readonly string[] CppLibNames =
        {
            "Freeze", "Glacier2", "Ice", "IceBox", "IceGrid", "IcePatch2", 
            "IceSSL", "IceStorm", "IceUtil", "IceXML" 
        };

        private static readonly string[] AssemblyNames =
        {
            "Glacier2", "Ice", "IceBox", "IceDiscovery", "IceLocatorDiscovery", 
            "IceGrid", "IcePatch2", "IceSSL", "IceStorm"
        };

        class OldConfiguration
        {
            public bool Enabled
            {
                get;
                set;
            }

            public String OutputDir
            {
                get;
                set;
            }

            public String HeaderExt
            {
                get;
                set;
            }

            public String SourceExt
            {
                get;
                set;
            }

            public String AdditionalOptions
            {
                get;
                set;
            }

            public String IncludeDirectories
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

            public String DLLExport
            {
                get;
                set;
            }

            public bool Load(Microsoft.Build.Evaluation.Project project, bool remove)
            {
                bool loaded = false;
                foreach (ProjectElement element in project.Xml.Children)
                {
                    ProjectExtensionsElement extension = element as ProjectExtensionsElement;
                    if (extension != null && !String.IsNullOrEmpty(extension.Content))
                    {
                        try
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(extension.Content);
                            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                            ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");
                            XmlNode userProperties = doc.DocumentElement.SelectSingleNode("/msb:VisualStudio/msb:UserProperties", ns);
                            if (userProperties != null)
                            {
                                Enabled = userProperties.Attributes[PropertyIce] != null &&
                                          userProperties.Attributes[PropertyIce].Value.Equals("True");

                                OutputDir = userProperties.Attributes[PropertyIceOutputDir] != null ?
                                            userProperties.Attributes[PropertyIceOutputDir].Value : String.Empty;

                                HeaderExt = userProperties.Attributes[PropertyIceHeaderExt] != null ?
                                            userProperties.Attributes[PropertyIceHeaderExt].Value : String.Empty;

                                SourceExt = userProperties.Attributes[PropertyIceSourceExt] != null ?
                                            userProperties.Attributes[PropertyIceSourceExt].Value : String.Empty;

                                AdditionalOptions = userProperties.Attributes[PropertyIceExtraOptions] != null ?
                                                    userProperties.Attributes[PropertyIceExtraOptions].Value : String.Empty;

                                IncludeDirectories =
                                    userProperties.Attributes[PropertyIceIncludePath] != null ?
                                    userProperties.Attributes[PropertyIceIncludePath].Value : String.Empty;


                                Stream = userProperties.Attributes[PropertyIceStreaming] != null &&
                                         userProperties.Attributes[PropertyIceStreaming].Value.Equals("True");

                                Checksum = userProperties.Attributes[PropertyIceChecksum] != null &&
                                           userProperties.Attributes[PropertyIceChecksum].Value.Equals("True");

                                Tie = userProperties.Attributes[PropertyIceTie] != null &&
                                      userProperties.Attributes[PropertyIceTie].Value.Equals("True");

                                Ice = userProperties.Attributes[PropertyIcePrefix] != null &&
                                      userProperties.Attributes[PropertyIcePrefix].Value.Equals("True");

                                DLLExport = userProperties.Attributes[PropertyIceDllExport] != null ?
                                            userProperties.Attributes[PropertyIceDllExport].Value : String.Empty;

                                if (remove)
                                {
                                    foreach (String name in OldPropertyNames)
                                    {
                                        if (userProperties.Attributes[name] != null)
                                        {
                                            userProperties.Attributes.Remove(userProperties.Attributes[name]);
                                        }
                                    }

                                    if (userProperties.Attributes.Count == 0)
                                    {
                                        project.Xml.RemoveChild(extension);
                                    }
                                }
                                loaded = true;
                                break;
                            }
                        }
                        catch (XmlException)
                        { 
                        }
                    }
                }
                return loaded;
            }
        }

        public static void TryUpgrade(List<EnvDTE.Project> projects)
        {
            String solutionDir = Path.GetDirectoryName(Package.Instance.DTE2.Solution.FullName);
            Dictionary<String, EnvDTE.Project> upgradeProjects = new Dictionary<String, EnvDTE.Project>();
            foreach (EnvDTE.Project project in projects)
            {
                if (new OldConfiguration().Load(MSBuildUtils.LoadedProject(project.FullName), false))
                {
                    upgradeProjects.Add(
                        FileUtil.RelativePath(solutionDir, project.FullName),
                        project);
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

        public static void Upgrade(List<EnvDTE.Project> projects, UpgradeProgressCallback progressCallback)
        {
            String solutionDir = Path.GetDirectoryName(Package.Instance.DTE2.Solution.FullName);
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Thread t = new Thread(() =>
            {
                for (int i = 0; i < projects.Count; ++i)
                {
                    if (progressCallback.Canceled)
                    {
                        break;
                    }

                    EnvDTE.Project project = projects[i];
                    String projectName = FileUtil.RelativePath(solutionDir, project.FullName);
                    if (Upgrade(project))
                    {
                        if (project.Globals != null)
                        {
                            foreach (String name in ProjectConverter.OldPropertyNames)
                            {
                                if (project.Globals.get_VariableExists(name))
                                {
                                    project.Globals[name] = String.Empty;
                                    project.Globals.set_VariablePersists(name, false);
                                }
                            }
                        }
                        Package.Instance.AddIceBuilderToProject(project);
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

        public static bool Upgrade(EnvDTE.Project project)
        {
            OldConfiguration oldConfiguration = new OldConfiguration();
            Microsoft.Build.Evaluation.Project msbuildProject = MSBuildUtils.LoadedProject(project.FullName);

            if (oldConfiguration.Load(msbuildProject, true))
            {
                if(DTEUtil.IsCppProject(project))
                {
                    return UpgadeCppConfiguration(msbuildProject, oldConfiguration);
                }
                else if (DTEUtil.IsCSharpProject(project))
                {
                    return UpgradeCSharpConfiguration(project, msbuildProject, oldConfiguration);
                }
            }
            return false;
        }

        private static bool UpgradeCSharpConfiguration(EnvDTE.Project dteProject,
            Microsoft.Build.Evaluation.Project project, OldConfiguration cfg)
        {
            ProjectPropertyGroupElement propertyGroup = project.Xml.PropertyGroups.FirstOrDefault(g => g.Label.Equals("IceBuilder"));
            if (propertyGroup == null)
            {
                propertyGroup = project.Xml.AddPropertyGroup();
                propertyGroup.Label = "IceBuilder";
            }

            if (!String.IsNullOrEmpty(cfg.OutputDir))
            {
                propertyGroup.AddProperty(PropertyNames.OutputDir, cfg.OutputDir);
            }

            if (!String.IsNullOrEmpty(cfg.AdditionalOptions))
            {
                propertyGroup.AddProperty(PropertyNames.AdditionalOptions, cfg.AdditionalOptions);
            }

            if (!String.IsNullOrEmpty(cfg.IncludeDirectories))
            {
                propertyGroup.AddProperty(PropertyNames.IncludeDirectories,
                    String.Format(@"{0};$(IceHome)\slice", cfg.IncludeDirectories));
            }
            else
            {
                propertyGroup.AddProperty(PropertyNames.IncludeDirectories, @"$(IceHome)\slice");
            }

            if (cfg.Stream)
            {
                propertyGroup.AddProperty(PropertyNames.Stream, "True");
            }

            if (cfg.Checksum)
            {
                propertyGroup.AddProperty(PropertyNames.Checksum, "True");
            }

            if (cfg.Ice)
            {
                propertyGroup.AddProperty(PropertyNames.AllowIcePrefix, "True");
            }

            if (cfg.Tie)
            {
                propertyGroup.AddProperty(PropertyNames.Tie, "True");
            }

            foreach (String assembly in AssemblyNames)
            {
                VSLangProj80.Reference3 reference = DTEUtil.FindAssemblyReference(dteProject, assembly) as VSLangProj80.Reference3;
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
            if (String.IsNullOrEmpty(cfg.OutputDir))
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
            if (propertyGroup == null)
            {
                propertyGroup = project.Xml.AddPropertyGroup();
                propertyGroup.Label = "IceBuilder";
            }

            if (!String.IsNullOrEmpty(cfg.OutputDir))
            {
                propertyGroup.AddProperty(PropertyNames.OutputDir, cfg.OutputDir);
            }

            if (!String.IsNullOrEmpty(cfg.HeaderExt))
            {
                propertyGroup.AddProperty(PropertyNames.HeaderExt, cfg.HeaderExt);
            }

            if (!String.IsNullOrEmpty(cfg.SourceExt))
            {
                propertyGroup.AddProperty(PropertyNames.SourceExt, cfg.SourceExt);
            }

            if (!String.IsNullOrEmpty(cfg.AdditionalOptions))
            {
                propertyGroup.AddProperty(PropertyNames.AdditionalOptions, cfg.AdditionalOptions);
            }

            if (!String.IsNullOrEmpty(cfg.IncludeDirectories))
            {
                propertyGroup.AddProperty(PropertyNames.IncludeDirectories, 
                    String.Format("{0};$({1})", cfg.IncludeDirectories, PropertyNames.IncludeDirectories));
            }

            if (cfg.Stream)
            {
                propertyGroup.AddProperty(PropertyNames.Stream, "True");
            }

            if (cfg.Checksum)
            {
                propertyGroup.AddProperty(PropertyNames.Checksum, "True");
            }

            if (cfg.Ice)
            {
                propertyGroup.AddProperty(PropertyNames.AllowIcePrefix, "True");
            }

            if (!String.IsNullOrEmpty(cfg.DLLExport))
            {
                propertyGroup.AddProperty(PropertyNames.DLLExport, cfg.DLLExport);
            }

            foreach (ProjectItemDefinitionGroupElement group in project.Xml.ItemDefinitionGroups)
            {
                //
                // Remove old property sheet from all configurations
                //
                IEnumerable<ProjectImportElement> imports = project.Xml.Imports.Where(
                    p => p.Project.Equals("$(ALLUSERSPROFILE)\\ZeroC\\Ice.props"));
                if(imports != null)
                {
                    foreach (ProjectImportElement import in imports)
                    {
                        import.Parent.RemoveChild(import);
                    }
                }
                //
                // WinRT SDK old property sheet
                //
                imports = project.Xml.Imports.Where(
                    p => p.Project.IndexOf("CommonConfiguration\\Neutral\\Ice.props") != -1);
                if(imports != null)
                {
                    foreach (ProjectImportElement import in imports)
                    {
                        import.Parent.RemoveChild(import);
                    }
                }
                
                foreach (ProjectItemDefinitionElement i in group.ItemDefinitions)
                {
                    if (i.ItemType.Equals("ClCompile"))
                    {
                        if (!String.IsNullOrEmpty(cfg.OutputDir))
                        {
                            ProjectMetadataElement metaData = i.Metadata.FirstOrDefault(
                                e => e.Name.Equals("AdditionalIncludeDirectories"));

                            if (metaData != null)
                            {
                                List<String> values = new List<String>(metaData.Value.Split(new char[] { ';' }));
                                values.Remove(cfg.OutputDir);
                                metaData.Value = String.Join(";", values);

                                if (values.Count == 0 ||
                                    (values.Count == 1 && values[0].Equals("%(AdditionalIncludeDirectories)")))
                                {
                                    i.RemoveChild(metaData);
                                }
                                else
                                {
                                    if (!values.Contains("%(AdditionalIncludeDirectories)"))
                                    {
                                        values.Add("%(AdditionalIncludeDirectories)");
                                    }
                                    metaData.Value = String.Join(";", values);
                                }
                            }
                        }
                    }
                    else if (i.ItemType.Equals("Link"))
                    {
                        ProjectMetadataElement metaData = i.Metadata.FirstOrDefault(
                                e => e.Name.Equals("AdditionalDependencies"));

                        if (metaData != null)
                        {
                            List<String> values = new List<String>(metaData.Value.Split(new char[] { ';' }));
                            foreach (String name in CppLibNames)
                            {
                                values.Remove(String.Format("{0}.lib", name));
                                values.Remove(String.Format("{0}d.lib", name));
                            }

                            if (values.Count == 0 || (values.Count == 1 && values[0].Equals("%(AdditionalDependencies)")))
                            {
                                i.RemoveChild(metaData);
                            }
                            else
                            {
                                metaData.Value = String.Join(";", values);
                            }
                        }

                        metaData = i.Metadata.FirstOrDefault(e => e.Name.Equals("AdditionalLibraryDirectories"));
                        if (metaData != null)
                        {
                            if (metaData.Value.Equals("%(AdditionalLibraryDirectories)"))
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
            if(String.IsNullOrEmpty(cfg.OutputDir))
            {
                foreach (String itemType in new String[] { "ClInclude", "ClCompile" })
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
