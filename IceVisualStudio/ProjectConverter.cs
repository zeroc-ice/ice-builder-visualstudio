using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

using System.Xml;

namespace ZeroC.IceVisualStudio
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

        public static string[] PropertyNames = new string[]
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

            public String AdditionalIncludeDirectories
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

            public bool Load(Project project)
            {
                bool loaded = false;
                foreach (ProjectElement element in project.Xml.Children)
                {
                    ProjectExtensionsElement extension = element as ProjectExtensionsElement;
                    if (extension != null)
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

                            AdditionalIncludeDirectories =
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


                            foreach (String name in PropertyNames)
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
                            loaded = true;
                            break;
                        }
                    }
                }
                return loaded;
            }
        }

        public static bool Update(Project project)
        {
            OldConfiguration oldConfiguration = new OldConfiguration();
            if(oldConfiguration.Load(project))
            {
                if(MSBuildUtils.IsCppProject(project))
                {
                    return UpdateCppConfiguration(project, oldConfiguration);
                }
                else if (MSBuildUtils.IsCSharpProject(project))
                {
                    return UpdateCSharpConfiguration(project, oldConfiguration);
                }
            }
            return false;
        }

        private static bool UpdateCSharpConfiguration(Project project, OldConfiguration cfg)
        {
            foreach (ProjectPropertyGroupElement group in project.Xml.PropertyGroups)
            {
                if (group.Condition.IndexOf("'$(Configuration)|$(Platform)'") != -1)
                {
                    if (!String.IsNullOrEmpty(cfg.OutputDir))
                    {
                        group.AddProperty("OutputDir", cfg.OutputDir);
                    }

                    if (!String.IsNullOrEmpty(cfg.HeaderExt))
                    {
                        group.AddProperty("HeaderExt", cfg.HeaderExt);
                    }

                    if (!String.IsNullOrEmpty(cfg.SourceExt))
                    {
                        group.AddProperty("SourceExt", cfg.SourceExt);
                    }

                    if (!String.IsNullOrEmpty(cfg.AdditionalOptions))
                    {
                        group.AddProperty("AdditionalOptions", cfg.AdditionalOptions);
                    }

                    if (!String.IsNullOrEmpty(cfg.AdditionalIncludeDirectories))
                    {
                        group.AddProperty("AdditionalIncludeDirectories", cfg.AdditionalIncludeDirectories);
                    }

                    if (cfg.Stream)
                    {
                        group.AddProperty("Stream", "True");
                    }

                    if (cfg.Checksum)
                    {
                        group.AddProperty("CheckSum", "True");
                    }

                    if (cfg.Ice)
                    {
                        group.AddProperty("Ice", "True");
                    }

                    if (cfg.Tie)
                    {
                        group.AddProperty("Tie", "True");
                    }
                }
            }

            ICollection<ProjectItem> items = project.GetItems("None");
            ICollection<ProjectItem> generatedItems = project.GetItems("Compile");


            foreach (ProjectItem j in generatedItems)
            {
                foreach (ProjectItem k in items)
                {
                    if (Path.GetExtension(k.UnevaluatedInclude).Equals(".ice"))
                    {
                        if (Path.GetFullPath(j.UnevaluatedInclude).Equals(
                            String.IsNullOrEmpty(cfg.OutputDir) ?
                                Path.GetFullPath(Path.ChangeExtension(k.UnevaluatedInclude, Path.GetExtension(j.UnevaluatedInclude))) :
                                Path.GetFullPath(
                                    Path.Combine(cfg.OutputDir,
                                                    Path.ChangeExtension(k.UnevaluatedInclude, Path.GetExtension(j.UnevaluatedInclude))))))
                        {
                            j.SetMetadataValue("DependentUpon", k.UnevaluatedInclude);
                            j.SetMetadataValue("AutoGen", "True");
                            break;
                        }
                    }
                }
            }
            return true;
        }

        private static bool UpdateCppConfiguration(Project project, OldConfiguration cfg)
        {
            foreach (ProjectItemDefinitionGroupElement group in project.Xml.ItemDefinitionGroups)
            {
                ProjectItemDefinitionElement item = group.AddItemDefinition("IceBuilder");

                if (!String.IsNullOrEmpty(cfg.OutputDir))
                {
                    item.AddMetadata("OutputDir", cfg.OutputDir);
                }

                if (!String.IsNullOrEmpty(cfg.HeaderExt))
                {
                    item.AddMetadata("HeaderExt", cfg.HeaderExt);
                }

                if (!String.IsNullOrEmpty(cfg.SourceExt))
                {
                    item.AddMetadata("SourceExt", cfg.SourceExt);
                }

                if (!String.IsNullOrEmpty(cfg.AdditionalOptions))
                {
                    item.AddMetadata("AdditionalOptions", cfg.AdditionalOptions);
                }

                if (!String.IsNullOrEmpty(cfg.AdditionalIncludeDirectories))
                {
                    item.AddMetadata("AdditionalIncludeDirectories", cfg.AdditionalIncludeDirectories);
                }

                if (cfg.Stream)
                {
                    item.AddMetadata("Stream", "True");
                }

                if (cfg.Checksum)
                {
                    item.AddMetadata("CheckSum", "True");
                }

                if (cfg.Ice)
                {
                    item.AddMetadata("Ice", "True");
                }

                if (!String.IsNullOrEmpty(cfg.DLLExport))
                {
                    item.AddMetadata("DLLExport", cfg.DLLExport);
                }

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
                    return true;
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
                    return true;
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

            ICollection<ProjectItem> items = project.GetItems("None");
            List<ProjectItem> sliceItems = new List<ProjectItem>();
            foreach (ProjectItem j in items)
            {
                if (Path.GetExtension(j.UnevaluatedInclude).Equals(".ice"))
                {
                    sliceItems.Add(j);
                }
            }

            foreach (ProjectItem j in sliceItems)
            {
                project.RemoveItem(j);
                project.AddItem("IceBuilder", j.UnevaluatedInclude);
            }


            items = project.GetItems("IceBuilder");
            ICollection<ProjectItem> generatedSourcesItems = project.GetItems("ClCompile");
            ICollection<ProjectItem> generatedHeaderItems = project.GetItems("ClInclude");

            foreach (ICollection<ProjectItem> generatedItems in
                    new ICollection<ProjectItem>[] { generatedHeaderItems, generatedSourcesItems })
            {
                foreach (ProjectItem j in generatedItems)
                {
                    foreach (ProjectItem k in items)
                    {
                        if (Path.GetFullPath(j.UnevaluatedInclude).Equals(
                            String.IsNullOrEmpty(cfg.OutputDir) ?
                                Path.GetFullPath(Path.ChangeExtension(k.UnevaluatedInclude, Path.GetExtension(j.UnevaluatedInclude))) :
                                Path.GetFullPath(
                                    Path.Combine(cfg.OutputDir,
                                                    Path.ChangeExtension(k.UnevaluatedInclude, Path.GetExtension(j.UnevaluatedInclude))))))
                        {
                            j.SetMetadataValue("DependentUpon", k.UnevaluatedInclude);
                            j.SetMetadataValue("AutoGen", "True");
                            break;
                        }
                    }
                }
            }
            return true;
        }
    }
}
