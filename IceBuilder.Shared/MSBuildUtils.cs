// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.Build.Construction;
using System;
using System.IO;
using System.Linq;

namespace IceBuilder
{
    class MSBuildUtils
    {
        public static readonly string CSharpProjectGUI = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

        public static readonly string IceBuilderCppProps = "$(IceBuilderCppProps)";
        public static readonly string IceBuilderCppTargets = "$(IceBuilderCppTargets)";
        public static readonly string IceBuilderCSharpProps = "$(IceBuilderCSharpProps)";
        public static readonly string IceBuilderCSharpTargets = "$(IceBuilderCSharpTargets)";

        public static readonly string IceBuilderCppPropsPathOld =
            @"$(LOCALAPPDATA)\ZeroC\IceBuilder\IceBuilder.Cpp.props";
        public static readonly string IceBuilderCppTargetsPathOld =
            @"$(LOCALAPPDATA)\ZeroC\IceBuilder\IceBuilder.Cpp.targets";
        public static readonly string IceBuilderCSharpPropsPathOld =
            @"$(LOCALAPPDATA)\ZeroC\IceBuilder\IceBuilder.CSharp.props";
        public static readonly string IceBuilderCSharpTargetsPathOld =
            @"$(LOCALAPPDATA)\ZeroC\IceBuilder\IceBuilder.CSharp.targets";

        public static readonly string IceBuilderInstallDir =
            @"$([MSBuild]::GetRegistryValue('HKEY_CURRENT_USER\SOFTWARE\ZeroC\IceBuilder', 'InstallDir.$(VisualStudioVersion)'))";

        public static readonly string IceBuilderCppPropsPath =
            @"$(IceBuilderInstallDir)\Resources\IceBuilder.Cpp.props";

        public static readonly string IceBuilderCppTargetsPath =
            @"$(IceBuilderInstallDir)\Resources\IceBuilder.Cpp.targets";

        public static readonly string IceBuilderCSharpPropsPath =
            @"$(IceBuilderInstallDir)\Resources\IceBuilder.CSharp.props";

        public static readonly string IceBuilderCSharpTargetsPath =
            @"$(IceBuilderInstallDir)\Resources\IceBuilder.CSharp.targets";

        public static readonly string EnsureIceBuilderImportsError =
            @"This project requires the Ice Builder for Visual Studio extension. " +
            @"Use ""Tools &gt; Extensions and Updates"" to install it. " +
            @"For more information, see https://visualstudiogallery.msdn.microsoft.com/1a64e701-63f2-4740-8004-290e6c682ce0.";

        public static bool HasImport(Microsoft.Build.Evaluation.Project project, string path) =>
            project.Xml.Imports.FirstOrDefault(
                p => p.Project.Equals(path, StringComparison.CurrentCultureIgnoreCase)) != null;

        public static bool RemoveProjectFlavorIfExists(Microsoft.Build.Evaluation.Project project, string flavor)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids", StringComparison.CurrentCultureIgnoreCase));

            if (property != null && property.Value.IndexOf(flavor) != -1)
            {
                property.Value = property.Value.Replace(flavor, "").Trim(new char[] { ';' });
                if (property.Value.Equals(CSharpProjectGUI, StringComparison.CurrentCultureIgnoreCase))
                {
                    property.Parent.RemoveChild(property);
                }
                return true; //flavor removed
            }
            return false;
        }

        public static bool HasProjectFlavor(Microsoft.Build.Evaluation.Project project, string flavor)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids", StringComparison.CurrentCultureIgnoreCase));
            return property != null && property.Value.IndexOf(flavor) != -1;
        }

        public static bool IsCppProject(Microsoft.Build.Evaluation.Project project) =>
            project != null &&
                project.Xml.Imports.FirstOrDefault(p => p.Project.IndexOf("Microsoft.Cpp.targets") != -1) != null;

        public static bool IsCSharpProject(Microsoft.Build.Evaluation.Project project)
        {
            if (project != null)
            {
                foreach (var p in project.Imports)
                {
                    if (p.ImportedProject.FullPath.EndsWith("Microsoft.CSharp.targets") ||
                       p.ImportedProject.FullPath.EndsWith("Microsoft.Windows.UI.Xaml.CSharp.targets"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsIceBuilderEnabled(Microsoft.Build.Evaluation.Project project)
        {
            bool value = false;
            if (IsCppProject(project))
            {
                value = HasImport(project, IceBuilderCppProps) && HasImport(project, IceBuilderCppTargets);
                value = value || (HasImport(project, IceBuilderCppPropsPathOld) && HasImport(project, IceBuilderCppTargetsPathOld));
            }
            else if (IsCSharpProject(project))
            {
                value = HasImport(project, IceBuilderCSharpProps) && HasImport(project, IceBuilderCSharpTargets);
                value = value || (HasImport(project, IceBuilderCSharpPropsPathOld) && HasImport(project, IceBuilderCSharpTargetsPathOld));
                value = value && HasProjectFlavor(project, Package.IceBuilderOldFlavor);
            }
            return value;
        }

        private static bool RemoveGlobalProperty(Microsoft.Build.Evaluation.Project project, string name)
        {
            ProjectPropertyGroupElement globals = project.Xml.PropertyGroups.FirstOrDefault(
                p => p.Label.Equals("Globals", StringComparison.CurrentCultureIgnoreCase));
            ProjectPropertyElement property = globals.Properties.FirstOrDefault(
                p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (property != null)
            {
                globals.RemoveChild(property);
                return true;
            }
            return false;
        }

        private static bool UpdateImport(Microsoft.Build.Evaluation.Project project, string oldValue, string newValue)
        {
            ProjectImportElement import = project.Xml.Imports.FirstOrDefault(
                p => p.Project.Equals(oldValue, StringComparison.CurrentCultureIgnoreCase));
            if (import != null)
            {
                import.Project = newValue;
                import.Condition = string.Format("Exists('{0}')", newValue);
                return true;
            }
            return false;
        }

        private static bool RemoveImport(Microsoft.Build.Evaluation.Project project, string import)
        {
            ProjectElement element = project.Xml.Imports.FirstOrDefault(
                p => p.Project.Equals(import, StringComparison.CurrentCultureIgnoreCase));
            if (element != null)
            {
                if (element.Parent != null)
                {
                    element.Parent.RemoveChild(element);
                }
                else
                {
                    project.Xml.RemoveChild(element);
                }
            }
            return element != null;
        }

        private static bool RemoveGlobalProperties(Microsoft.Build.Evaluation.Project project) =>
            RemoveGlobalProperty(project, "IceBuilderInstallDir");

        private static bool RemoveCppGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = RemoveGlobalProperties(project);
            modified = RemoveGlobalProperty(project, "IceBuilderCppProps") || modified;
            return RemoveGlobalProperty(project, "IceBuilderCppTargets") || modified;
        }

        private static bool RemoveCsharpGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = RemoveGlobalProperties(project);
            modified = RemoveGlobalProperty(project, "IceBuilderCsharpProps") || modified;
            return RemoveGlobalProperty(project, "IceBuilderCsharpTargets") || modified;
        }

        public static bool UpgradeProjectImports(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;
            if (IsCppProject(project))
            {
                modified = UpdateImport(project, IceBuilderCppPropsPathOld, IceBuilderCppProps) || modified;
                modified = UpdateImport(project, IceBuilderCppTargetsPathOld, IceBuilderCppTargets) || modified;
            }
            else if (IsCSharpProject(project))
            {
                modified = UpdateImport(project, IceBuilderCSharpPropsPathOld, IceBuilderCSharpProps) || modified;
                modified = UpdateImport(project, IceBuilderCSharpTargetsPathOld, IceBuilderCSharpTargets) || modified;
            }
            return modified;
        }

        public static bool UpgradeProjectProperties(Microsoft.Build.Evaluation.Project project, bool cpp)
        {
            bool modified = false;

            string outputDir = null;
            string headerOutputDir = null;
            string baseDirectoryForGeneratedInclude = null;

            string value = project.GetProperty(PropertyNames.Old.OutputDir, false);
            if (!string.IsNullOrEmpty(value))
            {
                project.RemovePropertyWithName(PropertyNames.Old.OutputDir);
                value = value.Replace("$(IceBuilder", "%(");
                project.SetItemMetadata(ItemMetadataNames.OutputDir, value);
                modified = true;
                outputDir = value;
            }
            else if (cpp)
            {
                // The default output directory for C++ generated items was changed to $(IntDir) but we keep the old
                // default when converted projects by setting it in the project file.
                project.SetItemMetadata(ItemMetadataNames.OutputDir, "generated");
                outputDir = "generated";
            }

            value = project.GetProperty(PropertyNames.Old.IncludeDirectories, false);
            if (!string.IsNullOrEmpty(value))
            {
                project.RemovePropertyWithName(PropertyNames.Old.IncludeDirectories);
                value = value.Replace("$(IceHome)\\slice", "");
                value = value.Replace("$(IceHome)/slice", "");
                value = value.Trim(';');
                value = value.Replace("$(IceBuilder", "%(");
                if (!string.IsNullOrEmpty(value))
                {
                    project.SetItemMetadata(ItemMetadataNames.IncludeDirectories, value);
                }
                modified = true;
            }

            value = project.GetProperty(PropertyNames.Old.HeaderOutputDir, false);
            if (!string.IsNullOrEmpty(value))
            {
                project.RemovePropertyWithName(PropertyNames.Old.HeaderOutputDir);
                value = value.Replace("$(IceBuilder", "%(");
                project.SetItemMetadata(ItemMetadataNames.HeaderOutputDir, value);
                modified = true;
                headerOutputDir = value;
            }

            value = project.GetProperty(PropertyNames.Old.HeaderExt, false);
            if (!string.IsNullOrEmpty(value))
            {
                project.RemovePropertyWithName(PropertyNames.Old.HeaderExt);
                project.SetItemMetadata(ItemMetadataNames.HeaderExt, value);
                modified = true;
            }

            value = project.GetProperty(PropertyNames.Old.BaseDirectoryForGeneratedInclude, false);
            if (!string.IsNullOrEmpty(value))
            {
                project.RemovePropertyWithName(PropertyNames.Old.BaseDirectoryForGeneratedInclude);
                project.SetItemMetadata(ItemMetadataNames.BaseDirectoryForGeneratedInclude, value);
                modified = true;
                baseDirectoryForGeneratedInclude = value;
            }

            value = project.GetProperty(PropertyNames.Old.SourceExt, false);
            if (!string.IsNullOrEmpty(value))
            {
                project.RemovePropertyWithName(PropertyNames.Old.SourceExt);
                project.SetItemMetadata(ItemMetadataNames.SourceExt, value);
                modified = true;
            }

            value = project.GetProperty(PropertyNames.Old.AllowIcePrefix, false);
            string additionalOptions = project.GetProperty(PropertyNames.Old.AdditionalOptions, false);
            if (!string.IsNullOrEmpty(additionalOptions))
            {
                project.RemovePropertyWithName(PropertyNames.Old.AdditionalOptions);
                modified = true;
            }

            if (!string.IsNullOrEmpty(value))
            {
                if (value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = String.Format("{0} --ice", additionalOptions).Trim();
                }
                project.RemovePropertyWithName(PropertyNames.Old.AllowIcePrefix);
                modified = true;
            }

            value = project.GetProperty(PropertyNames.Old.Underscore, false);
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = String.Format("{0} --underscore", additionalOptions).Trim();
                }
                project.RemovePropertyWithName(PropertyNames.Old.Underscore);
                modified = true;
            }

            value = project.GetProperty(PropertyNames.Old.Stream, false);
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = $"{additionalOptions} --stream ".Trim();
                }
                project.RemovePropertyWithName(PropertyNames.Old.Stream);
                modified = true;
            }

            value = project.GetProperty(PropertyNames.Old.DLLExport, false);
            if (!string.IsNullOrEmpty(value))
            {
                additionalOptions = $"{additionalOptions} --dll-export {value}".Trim();
                project.RemovePropertyWithName(PropertyNames.Old.DLLExport);
                modified = true;
            }

            value = project.GetProperty(PropertyNames.Old.Checksum, false);
            if (!string.IsNullOrEmpty(value))
            {
                additionalOptions = "{additionalOptions} --checksum".Trim();
                project.RemovePropertyWithName(PropertyNames.Old.Checksum);
                modified = true;
            }

            value = project.GetProperty(PropertyNames.Old.Tie, false);
            if (!string.IsNullOrEmpty(value))
            {
                additionalOptions = $"{additionalOptions} --tie".Trim();
                project.RemovePropertyWithName(PropertyNames.Old.Tie);
                modified = true;
            }

            if (!string.IsNullOrEmpty(additionalOptions))
            {
                additionalOptions = additionalOptions.Replace("IceBuilder", "SliceCompile");
                project.SetItemMetadata(ItemMetadataNames.AdditionalOptions, additionalOptions);
            }

            if (string.IsNullOrEmpty(baseDirectoryForGeneratedInclude))
            {
                if (!string.IsNullOrEmpty(headerOutputDir))
                {
                    project.SetClCompileAdditionalIncludeDirectories(headerOutputDir);
                }
                else
                {
                    project.SetClCompileAdditionalIncludeDirectories(outputDir);
                }
            }

            value = project.GetProperty("ProjectTypeGuids", false);
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace(Package.IceBuilderOldFlavor, Package.IceBuilderNewFlavor);
                project.SetProperty("ProjectTypeGuids", value, "");
                modified = true;
            }

            return modified;
        }

        public static bool UpgradeProjectItems(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;

            foreach (var item in project.Xml.Items)
            {
                if (item.Include.EndsWith(".ice") && !item.ItemType.Equals("SliceCompile"))
                {
                    item.ItemType = "SliceCompile";
                    modified = true;
                }
            }

            return modified;
        }

        public static bool UpgradeCSharpGeneratedItems(Microsoft.Build.Evaluation.Project project, string outputDir) =>
            UpgradeGeneratedItems(project, outputDir, "cs", "Compile");

        public static bool UpgradeGeneratedItems(Microsoft.Build.Evaluation.Project project, string outputDir, string ext, string itemType)
        {
            string projectDir = Path.GetDirectoryName(project.FullPath);
            bool modified = false;
            var sliceItems = project.AllEvaluatedItems.Where(item => item.ItemType.Equals("SliceCompile"));
            foreach (var sliceItem in sliceItems)
            {
                var generatedPath = FileUtil.RelativePath(projectDir,
                    Path.Combine(outputDir, string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(sliceItem.EvaluatedInclude), ext)));
                var generated = project.AllEvaluatedItems.FirstOrDefault(
                    item => item.ItemType.Equals(itemType) && item.EvaluatedInclude.Equals(generatedPath));
                if (generated != null)
                {
                    generated.SetMetadataValue("SliceCompileSource", sliceItem.EvaluatedInclude);
                    modified = true;
                }
            }
            return modified;
        }

        public static bool RemoveIceBuilderFromProject(Microsoft.Build.Evaluation.Project project, bool keepProjectFlavor = false)
        {
            bool modified = false;
            if (project != null)
            {
                if (IsCppProject(project))
                {
                    modified = RemoveCppGlobalProperties(project);
                    modified = RemoveImport(project, IceBuilderCppProps) || modified;
                    modified = RemoveImport(project, IceBuilderCppTargets) || modified;
                }
                else if (IsCSharpProject(project))
                {
                    modified = RemoveCsharpGlobalProperties(project);
                    modified = RemoveImport(project, IceBuilderCSharpProps) || modified;
                    modified = RemoveImport(project, IceBuilderCSharpTargets) || modified;
                    if (!keepProjectFlavor)
                    {
                        modified = RemoveProjectFlavorIfExists(project, Package.IceBuilderOldFlavor) || modified;
                    }
                }

                // Remove EnsureIceBuilderImports target
                var target = project.Xml.Targets.FirstOrDefault(
                    t => t.Name.Equals("EnsureIceBuilderImports", StringComparison.CurrentCultureIgnoreCase));
                if (target != null)
                {
                    if (target.Parent != null)
                    {
                        target.Parent.RemoveChild(target);
                    }
                    else
                    {
                        project.Xml.RemoveChild(target);
                    }
                }
            }
            return modified;
        }
    }
}
