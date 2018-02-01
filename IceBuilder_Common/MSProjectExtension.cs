// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;

namespace IceBuilder
{
    public static class MSProjectExtension
    {
        static readonly ProjectCollection cppProjectColletion = new ProjectCollection();

        public static Project LoadedProject(string path)
        {
            return ProjectCollection.GlobalProjectCollection.GetLoadedProjects(path).FirstOrDefault();
        }

        public static string GetItemMetadata(this Project project, string identity, string name, string defaultValue = "")
        {
            var item = project.AllEvaluatedItems.FirstOrDefault(
                i => i.ItemType.Equals("SliceCompile") && i.EvaluatedInclude.Equals(identity));
            if(item == null)
            {
                return project.GetDefaultItemMetadata(name, false, defaultValue);
            }
            else
            {
                return item.HasMetadata(name) ? item.GetMetadata(name).UnevaluatedValue : defaultValue;
            }
        }

        public static string GetDefaultItemMetadata(this Project project, string name, bool evaluated, string defaultValue = "")
        {
            var meta = project.AllEvaluatedItemDefinitionMetadata.LastOrDefault(
                m => m.ItemType.Equals("SliceCompile") && m.Name.Equals(name));
            if(meta != null)
            {
                return evaluated ? meta.EvaluatedValue : meta.UnevaluatedValue;
            }
            return defaultValue;
        }

        public static void SetItemMetadata(this Project project, string itemType, string label, string name, string value)
        {
            var group = project.Xml.ItemDefinitionGroups.FirstOrDefault(
                g => g.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));
            if(group == null)
            {
                group = project.Xml.CreateItemDefinitionGroupElement();
                group.Label = label;

                while(true)
                {
                    //
                    // For C++ projects we create our ItemDefinitionGroup after the last
                    // existing ItemDefinitionGroup
                    //
                    var lastItemDefinitionGroups = project.Xml.ItemDefinitionGroups.LastOrDefault();
                    if(lastItemDefinitionGroups != null)
                    {
                        project.Xml.InsertAfterChild(group, lastItemDefinitionGroups);
                        break;
                    }
                    //
                    // For .NET and .NET Core projects we create our ItemDefinitionGroup after the last
                    // existing PropertyGroup
                    //
                    var lastPropertyGroup = project.Xml.PropertyGroups.LastOrDefault();
                    if(lastPropertyGroup != null)
                    {
                        project.Xml.InsertAfterChild(group, lastPropertyGroup);
                        break;
                    }
                    //
                    // Otherwise add our ItemDefinitionGroup as project first child
                    //
                    project.Xml.PrependChild(group);
                    break;
                }
            }

            var item = group.ItemDefinitions.FirstOrDefault(i => i.ItemType.Equals(itemType));
            if(item == null)
            {
                item = group.AddItemDefinition(itemType);
            }

            var metadata = item.Metadata.FirstOrDefault(m => m.Name.Equals(name));
            if(metadata == null)
            {
                metadata = item.AddMetadata(name, value);
            }
            else
            {
                metadata.Value = value;
            }
        }

        public static void SetItemMetadata(this Project project, string name, string value)
        {
            project.SetItemMetadata("SliceCompile", "IceBuilder", name, value);
        }

        public static void SetClCompileAdditionalIncludeDirectories(this Project project, string value)
        {
            // Set AdditionalIncludeDirectories in all ClCompile ItemDefinitions

            project.Xml.ItemDefinitionGroups.SelectMany(def => def.ItemDefinitions)
                .Where(element => element.ItemType.Equals("ClCompile")).ToList()
                .ForEach(cl =>
                {
                    var metadata = cl.Metadata.FirstOrDefault(m => m.Name.Equals("AdditionalIncludeDirectories"));
                    if(metadata == null)
                    {
                        metadata = cl.AddMetadata("AdditionalIncludeDirectories",
                            string.Format("{0};%(AdditionalIncludeDirectories)", value));
                    }
                    else
                    {
                        metadata.Value = string.Format("{0};{1}", value, metadata.Value);
                    }
                });
        }

        public static bool RemovePropertyWithName(this Project project, string name)
        {
            var property = project.Xml.Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if(property != null)
            {
                property.Parent.RemoveChild(property);
                return true;
            }
            return false;
        }

        public static string
        GetPropertyWithDefault(this Project project, string name, string defaultValue, bool imported = true)
        {
            var value = project.GetProperty(name, imported);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static string GetProperty(this Project project, string name, bool imported = true)
        {
            var property = project.GetProperty(name);
            if(property != null)
            {
                if(imported || !property.IsImported)
                {
                    return property.UnevaluatedValue;
                }
            }
            return string.Empty;
        }

        public static string GetEvaluatedProperty(this Project project, string name)
        {
            return project.GetPropertyValue(name);
        }

        public static void SetProperty(this Project project, string name, string value, string label)
        {
            if(string.IsNullOrEmpty(label))
            {
                project.SetProperty(name, value);
            }
            else
            {
                var group = project.Xml.PropertyGroups.FirstOrDefault(
                    g => g.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));
                if(group == null)
                {
                    //
                    // Create our property group after the main language targets are imported so we can use the properties
                    // defined in this files.
                    //
                    var import = project.Xml.Imports.FirstOrDefault(
                        p => (p.Project.IndexOf("Microsoft.Cpp.targets", StringComparison.CurrentCultureIgnoreCase) != -1 ||
                              p.Project.Equals("Microsoft.CSharp.targets", StringComparison.CurrentCultureIgnoreCase)));
                    if(import != null)
                    {
                        group = project.Xml.CreatePropertyGroupElement();
                        project.Xml.InsertAfterChild(group, import);
                    }
                    else
                    {
                        group = project.Xml.CreatePropertyGroupElement();
                        project.Xml.AppendChild(group);
                    }
                    group.Label = label;
                }

                var property = group.Properties.FirstOrDefault(
                    p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                if(property != null)
                {
                    property.Value = value;
                }
                else
                {
                    group.AddProperty(name, value);
                }
            }
        }

        public static void AddProjectFlavorIfNotExists(this Project project, string flavor)
        {
            var property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids", StringComparison.CurrentCultureIgnoreCase));

            if(property != null)
            {
                if(property.Value.IndexOf(flavor) == -1)
                {
                    if(string.IsNullOrEmpty(property.Value))
                    {
                        property.Value = string.Format("{0};{1}", flavor, CSharpProjectGUI);
                    }
                    else
                    {
                        property.Value = string.Format("{0};{1}", flavor, property.Value);
                    }
                }
            }
            else
            {
                project.Xml.AddProperty("ProjectTypeGuids", string.Format("{0};{1}", flavor, CSharpProjectGUI));
            }
        }

        public static void SetGeneratedItemCustomMetadata(this Project project, string slice, string generated,
            List<string> excludedConfigurations = null)
        {
            var item = project.AllEvaluatedItems.FirstOrDefault(i => generated.Equals(i.EvaluatedInclude));
            if(item != null)
            {
                var element = item.Xml;
                if(excludedConfigurations != null)
                {
                    foreach(var conf in excludedConfigurations)
                    {
                        var metadata = element.AddMetadata("ExcludedFromBuild", "true");
                        metadata.Condition = string.Format("'$(Configuration)|$(Platform)'=='{0}'", conf);
                    }
                }
                element.AddMetadata("SliceCompileSource", slice);
            }
        }

        public static readonly string CSharpProjectGUI = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
    }
}
