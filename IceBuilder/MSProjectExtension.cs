// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.Build.Evaluation;
using System;
using System.Linq;
using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder;

public static class MSProjectExtension
{
    public static MSBuildProject LoadedProject(string path) =>
        ProjectCollection.GlobalProjectCollection.GetLoadedProjects(path).FirstOrDefault() ??
        ProjectCollection.GlobalProjectCollection.LoadProject(path);

    public static string GetItemMetadata(
        this MSBuildProject project,
        string identity,
        string name,
        string defaultValue = "")
    {
        var item = project.AllEvaluatedItems.FirstOrDefault(
            i => i.ItemType.Equals("SliceCompile") && i.EvaluatedInclude.Equals(identity));
        if (item == null)
        {
            return project.GetDefaultItemMetadata(name, false, defaultValue);
        }
        else
        {
            var metadata = item.GetMetadata(name);
            return metadata != null ? metadata.UnevaluatedValue : defaultValue;
        }
    }

    public static string GetDefaultItemMetadata(
        this MSBuildProject project,
        string name,
        bool evaluated,
        string defaultValue = "")
    {
        var meta = project.AllEvaluatedItemDefinitionMetadata.LastOrDefault(
            m => m.ItemType.Equals("SliceCompile") && m.Name.Equals(name));
        if (meta != null)
        {
            return evaluated ? meta.EvaluatedValue : meta.UnevaluatedValue;
        }
        return defaultValue;
    }

    public static void SetItemMetadata(
        this MSBuildProject project,
        string itemType,
        string label,
        string name,
        string value)
    {
        var group = project.Xml.ItemDefinitionGroups.FirstOrDefault(
            g => g.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));
        if (group == null)
        {
            group = project.Xml.CreateItemDefinitionGroupElement();
            group.Label = label;

            while (true)
            {
                // For C++ projects we create our ItemDefinitionGroup after the last existing ItemDefinitionGroup.
                var lastItemDefinitionGroups = project.Xml.ItemDefinitionGroups.LastOrDefault();
                if (lastItemDefinitionGroups != null)
                {
                    project.Xml.InsertAfterChild(group, lastItemDefinitionGroups);
                    break;
                }
                // For .NET projects we create our ItemDefinitionGroup after the last existing PropertyGroup.
                var lastPropertyGroup = project.Xml.PropertyGroups.LastOrDefault();
                if (lastPropertyGroup != null)
                {
                    project.Xml.InsertAfterChild(group, lastPropertyGroup);
                    break;
                }

                // Otherwise add our ItemDefinitionGroup as project first child
                project.Xml.PrependChild(group);
                break;
            }
        }

        var item = group.ItemDefinitions.FirstOrDefault(i => i.ItemType.Equals(itemType));
        item ??= group.AddItemDefinition(itemType);

        var metadata = item.Metadata.FirstOrDefault(m => m.Name.Equals(name));
        if (metadata == null)
        {
            metadata = item.AddMetadata(name, value);
        }
        else
        {
            metadata.Value = value;
        }
    }

    public static string GetPropertyWithDefault(
        this MSBuildProject project,
        string name,
        string defaultValue,
        bool imported = true)
    {
        var value = project.GetProperty(name, imported);
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    public static string GetProperty(this MSBuildProject project, string name, bool imported = true)
    {
        var property = project.GetProperty(name);
        if (property != null)
        {
            if (imported || !property.IsImported)
            {
                return property.UnevaluatedValue;
            }
        }
        return string.Empty;
    }

    public static string GetEvaluatedProperty(this MSBuildProject project, string name) =>
        project.GetPropertyValue(name);

    public static bool HasProjectFlavor(this MSBuildProject project, string flavor)
    {
        var property = project.Xml.Properties.FirstOrDefault(
           p => p.Name.Equals("ProjectTypeGuids", StringComparison.CurrentCultureIgnoreCase));
        return property != null && property.Value.IndexOf(flavor) != -1;
    }

    public static void AddProjectFlavorIfNotExists(this MSBuildProject project, string flavor)
    {
        var property = project.Xml.Properties.FirstOrDefault(
            p => p.Name.Equals("ProjectTypeGuids", StringComparison.CurrentCultureIgnoreCase));

        if (property != null)
        {
            if (property.Value.IndexOf(flavor) == -1)
            {
                if (string.IsNullOrEmpty(property.Value))
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

    public static readonly string CSharpProjectGUI = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
}
