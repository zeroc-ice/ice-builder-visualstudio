// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    class MSBuildUtils
    {
        public static readonly string IceBuilderProjectFlavorGUID = "{3C53C28F-DC44-46B0-8B85-0C96B85B2042}";
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

        static readonly ProjectCollection cppProjectColletion = new ProjectCollection();

        public static Microsoft.Build.Evaluation.Project LoadedProject(string path, bool cpp, bool cached)
        {
            Microsoft.Build.Evaluation.Project project = null;
            ProjectCollection collection = cpp ? cppProjectColletion : ProjectCollection.GlobalProjectCollection;
            project = collection.GetLoadedProjects(path).FirstOrDefault();

            if(project == null)
            {
                project = collection.LoadProject(path);
            }
            else
            {
                if(cpp && !cached)
                {
                    //
                    // That is required to get C++ project properties re-evaluated
                    // with Visual Studio 2013 and Visual Studio 2015
                    //
                    collection.UnloadProject(project);
                    collection.UnloadAllProjects();
                    project = collection.LoadProject(path);
                }
            }
            return project;
        }

        public static bool HasImport(Microsoft.Build.Evaluation.Project project, string path)
        {
            return project.Xml.Imports.FirstOrDefault(
                p => p.Project.Equals(path, StringComparison.CurrentCultureIgnoreCase)) != null;
        }

        public static bool AddProjectFlavorIfNotExists(Microsoft.Build.Evaluation.Project project, string flavor)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids", StringComparison.CurrentCultureIgnoreCase));

            if (property != null)
            {
                if (property.Value.IndexOf(flavor) == -1)
                {
                    DTEUtil.EnsureFileIsCheckout(project.FullPath);
                    if (string.IsNullOrEmpty(property.Value))
                    {
                        property.Value = string.Format("{0};{1}", flavor, CSharpProjectGUI);
                    }
                    else
                    {
                        property.Value = string.Format("{0};{1}", flavor, property.Value);
                    }
                    return true; //ProjectTypeGuids updated
                }
                else
                {
                    return false; //ProjectTypeGuids already has this flavor
                }
            }

            // ProjectTypeGuids not present
            DTEUtil.EnsureFileIsCheckout(project.FullPath);
            project.Xml.AddProperty("ProjectTypeGuids", string.Format("{0};{1}", flavor, CSharpProjectGUI));
            return true;
        }

        public static bool RemoveProjectFlavorIfExists(Microsoft.Build.Evaluation.Project project, string flavor)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids", StringComparison.CurrentCultureIgnoreCase));

            if(property != null && property.Value.IndexOf(flavor) != -1)
            {
                DTEUtil.EnsureFileIsCheckout(project.FullPath);
                property.Value = property.Value.Replace(flavor, "").Trim(new char[] { ';' });
                if(property.Value.Equals(CSharpProjectGUI, StringComparison.CurrentCultureIgnoreCase))
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

        public static bool IsCppProject(Microsoft.Build.Evaluation.Project project)
        {
            return project != null &&
                project.Xml.Imports.FirstOrDefault(p => p.Project.IndexOf("Microsoft.Cpp.targets") != -1) != null;
        }

        public static bool IsCSharpProject(Microsoft.Build.Evaluation.Project project)
        {
            if(project != null)
            {
                foreach (var p in project.Imports)
                {
                    if(p.ImportedProject.FullPath.EndsWith("Microsoft.CSharp.targets") ||
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
            if(IsCppProject(project))
            {
                value = HasImport(project, IceBuilderCppProps) && HasImport(project, IceBuilderCppTargets);
                value = value || (HasImport(project, IceBuilderCppPropsPathOld) && HasImport(project, IceBuilderCppTargetsPathOld));
            }
            else if(IsCSharpProject(project))
            {
                value = HasImport(project, IceBuilderCSharpProps) && HasImport(project, IceBuilderCSharpTargets);
                value = value || (HasImport(project, IceBuilderCSharpPropsPathOld) && HasImport(project, IceBuilderCSharpTargetsPathOld));
                value = value && HasProjectFlavor(project, IceBuilderProjectFlavorGUID);
            }
            return value;
        }

        private static bool RemoveGlobalProperty(Microsoft.Build.Evaluation.Project project, string name)
        {
            ProjectPropertyGroupElement globals = project.Xml.PropertyGroups.FirstOrDefault(
                p => p.Label.Equals("Globals", StringComparison.CurrentCultureIgnoreCase));
            ProjectPropertyElement property = globals.Properties.FirstOrDefault(
                p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if(property != null)
            {
                DTEUtil.EnsureFileIsCheckout(project.FullPath);
                globals.RemoveChild(property);
                return true;
            }
            return false;
        }

        private static bool RemoveProperty(Microsoft.Build.Evaluation.Project project, string name)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if(property != null)
            {
                DTEUtil.EnsureFileIsCheckout(project.FullPath);
                property.Parent.RemoveChild(property);
                return true;
            }
            return false;
        }

        private static bool UpdateImport(Microsoft.Build.Evaluation.Project project, string oldValue, string newValue)
        {
            ProjectImportElement import = project.Xml.Imports.FirstOrDefault(
                p => p.Project.Equals(oldValue, StringComparison.CurrentCultureIgnoreCase));
            if(import != null)
            {
                DTEUtil.EnsureFileIsCheckout(project.FullPath);
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
            if(element != null)
            {
                DTEUtil.EnsureFileIsCheckout(project.FullPath);
                if(element.Parent != null)
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

        private static bool RemoveGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            return RemoveGlobalProperty(project, "IceBuilderInstallDir");
        }

        private static bool RemoveCppGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = RemoveGlobalProperties(project);
            modified = RemoveGlobalProperty(project, "IceBuilderCppProps") || modified;
            return RemoveGlobalProperty(project, "IceBuilderCppTargets");
        }

        private static bool RemoveCsharpGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = RemoveGlobalProperties(project);
            modified = RemoveGlobalProperty(project, "IceBuilderCsharpProps") || modified;
            return RemoveGlobalProperty(project, "IceBuilderCsharpTargets");
        }

        public static bool UpgradeProjectImports(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;
            if(IsCppProject(project))
            {
                modified = UpdateImport(project, IceBuilderCppPropsPathOld, IceBuilderCppProps) || modified;
                modified = UpdateImport(project, IceBuilderCppTargetsPathOld, IceBuilderCppTargets) || modified;
            }
            else if(IsCSharpProject(project))
            {
                modified = UpdateImport(project, IceBuilderCSharpPropsPathOld, IceBuilderCSharpProps) || modified;
                modified = UpdateImport(project, IceBuilderCSharpTargetsPathOld, IceBuilderCSharpTargets) || modified;
            }
            return modified;
        }

        public static bool UpgradeProjectProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;

            string value = GetProperty(project, PropertyNames.Old.OutputDir, false);
            if(!string.IsNullOrEmpty(value))
            {
                RemoveProperty(project, PropertyNames.Old.OutputDir);
                value = value.Replace("IceBuilder", "SliceCompile");
                SetProperty(project, "IceBuilder", PropertyNames.New.OutputDir, value);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.IncludeDirectories, false);
            if(!string.IsNullOrEmpty(value))
            {
                RemoveProperty(project, PropertyNames.Old.IncludeDirectories);
                value = value.Replace("$(IceHome)\\slice", "");
                value = value.Replace("$(IceHome)/slice", "");
                value = value.Trim(';');
                value = value.Replace("IceBuilder", "SliceCompile");
                if (!string.IsNullOrEmpty(value))
                {
                    SetProperty(project, "IceBuilder", PropertyNames.New.IncludeDirectories, value);
                }
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.HeaderOutputDir, false);
            if (!string.IsNullOrEmpty(value))
            {
                RemoveProperty(project, PropertyNames.Old.HeaderOutputDir);
                value = value.Replace("IceBuilder", "SliceCompile");
                SetProperty(project, "IceBuilder", PropertyNames.New.HeaderOutputDir, value);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.HeaderExt, false);
            if (!string.IsNullOrEmpty(value))
            {
                RemoveProperty(project, PropertyNames.Old.HeaderExt);
                SetProperty(project, "IceBuilder", PropertyNames.New.HeaderExt, value);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.BaseDirectoryForGeneratedInclude);
            if(!string.IsNullOrEmpty(value))
            {
                RemoveProperty(project, PropertyNames.Old.BaseDirectoryForGeneratedInclude);
                value = value.Replace("IceBuilder", "SliceCompile");
                SetProperty(project, "IceBuilder", PropertyNames.New.BaseDirectoryForGeneratedInclude, value);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.SourceExt, false);
            if (!string.IsNullOrEmpty(value))
            {
                RemoveProperty(project, PropertyNames.Old.SourceExt);
                SetProperty(project, "IceBuilder", PropertyNames.New.SourceExt, value);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.AllowIcePrefix, false);
            string additionalOptions = GetProperty(project, PropertyNames.Old.AdditionalOptions);
            if(!string.IsNullOrEmpty(additionalOptions))
            {
                RemoveProperty(project, PropertyNames.Old.AdditionalOptions);
                modified = true;
            }

            if(!string.IsNullOrEmpty(value))
            {
                if(value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = String.Format("{0} --ice", additionalOptions).Trim();
                }
                RemoveProperty(project, PropertyNames.Old.AllowIcePrefix);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.Underscore, false);
            if(!string.IsNullOrEmpty(value))
            {
                if(value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = String.Format("{0} --underscore", additionalOptions).Trim();
                }
                RemoveProperty(project, PropertyNames.Old.Underscore);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.Stream, false);
            if(!string.IsNullOrEmpty(value))
            {
                if(value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = String.Format("{0} --stream ", additionalOptions).Trim();
                }
                RemoveProperty(project, PropertyNames.Old.Stream);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.DLLExport, false);
            if(!string.IsNullOrEmpty(value))
            {
                additionalOptions = String.Format("{0} --dll-export {1}", additionalOptions, value).Trim();
                RemoveProperty(project, PropertyNames.Old.DLLExport);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.Checksum, false);
            if(!string.IsNullOrEmpty(value))
            {
                additionalOptions = String.Format("{0} --checksum", additionalOptions).Trim();
                RemoveProperty(project, PropertyNames.Old.Checksum);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Old.Tie, false);
            if(!string.IsNullOrEmpty(value))
            {
                additionalOptions = String.Format("{0} --tie", additionalOptions).Trim();
                RemoveProperty(project, PropertyNames.Old.Tie);
                modified = true;
            }

            if(!string.IsNullOrEmpty(additionalOptions))
            {
                additionalOptions = additionalOptions.Replace("IceBuilder", "SliceCompile");
                SetProperty(project, "IceBuilder", PropertyNames.New.AdditionalOptions, additionalOptions);
            }

            return modified;
        }

        public static bool UpgradeProjectItems(Microsoft.Build.Evaluation.Project project, bool cpp)
        {
            bool modified = false;

            foreach (var item in project.Xml.Items)
            {
                if(item.Include.EndsWith(".ice") && !item.ItemType.Equals("SliceCompile"))
                {
                    item.ItemType = "SliceCompile";
                    modified = true;
                }
            }

            var sliceItems = project.Items.Where(i => i.ItemType.Equals("SliceCompile")).Select(i =>  i.GetMetadataValue("FileName"));

            //
            // The default output directory for C++ generated sources changed to $(IntDir) in Ice Builder 5.x
            // projects that were using the default will be automatically upgrade, we remove the old generated
            // project items and the new generated items will be automatically add during project initialization
            // if required.
            //
            if (cpp)
            {
                bool defaultOuputDir = string.IsNullOrEmpty(GetProperty(project, PropertyNames.New.OutputDir, false)) &&
                                       GetProperty(project, PropertyNames.New.OutputDir, true).Equals("$(IntDir)");
                if (defaultOuputDir)
                {
                    var oldGenerated = project.Items.Where(item =>
                        {
                            if (item.ItemType.Equals("ClCompile") || item.ItemType.Equals("ClInclude"))
                            {
                                return System.IO.Path.GetDirectoryName(item.EvaluatedInclude).Equals("generated") &&
                                    sliceItems.Contains(System.IO.Path.GetFileNameWithoutExtension(item.EvaluatedInclude));
                            }
                            else
                            {
                                return false;
                            }
                        });
                    project.RemoveItems(oldGenerated);
                }
            }

            return modified;
        }

        public static bool RemoveIceBuilderFromProject(Microsoft.Build.Evaluation.Project project, bool keepProjectFlavor = false)
        {
            bool modified = false;
            if(project != null)
            {
                if(IsCppProject(project))
                {
                    modified = RemoveCppGlobalProperties(project);
                    modified = RemoveImport(project, IceBuilderCppProps) || modified;
                    modified = RemoveImport(project, IceBuilderCppTargets) || modified;
                }
                else if(IsCSharpProject(project))
                {
                    modified = RemoveCsharpGlobalProperties(project);
                    modified = RemoveImport(project, IceBuilderCSharpProps) || modified;
                    modified = RemoveImport(project, IceBuilderCSharpTargets) || modified;
                    if(!keepProjectFlavor)
                    {
                        modified = RemoveProjectFlavorIfExists(project, IceBuilderProjectFlavorGUID) || modified;
                    }
                }

                //
                // Remove EnsureIceBuilderImports target
                //
                var target = project.Xml.Targets.FirstOrDefault(
                    t => t.Name.Equals("EnsureIceBuilderImports", StringComparison.CurrentCultureIgnoreCase));
                if(target != null)
                {
                    DTEUtil.EnsureFileIsCheckout(project.FullPath);
                    if(target.Parent != null)
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

        public static void SetProperty(Microsoft.Build.Evaluation.Project project, string label, string name, string value)
        {
            DTEUtil.EnsureFileIsCheckout(project.FullPath);
            ProjectPropertyGroupElement group = project.Xml.PropertyGroups.FirstOrDefault(
                g => g.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));
            if(group == null)
            {
                //
                // Create our property group after the main language targets are imported so we can use the properties
                // defined in this files.
                //
                ProjectImportElement import = project.Xml.Imports.FirstOrDefault(
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

            ProjectPropertyElement property = group.Properties.FirstOrDefault(
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

        public static string GetProperty(Microsoft.Build.Evaluation.Project project, string name, bool imported = true)
        {
            ProjectProperty property = project.GetProperty(name);
            return property == null || (!imported && property.IsImported) ? String.Empty : property.UnevaluatedValue;
        }

        public static string GetEvaluatedProperty(Microsoft.Build.Evaluation.Project project, string name)
        {
            return project.GetPropertyValue(name);
        }

        public static bool HasIceBuilderPackageReference(Microsoft.Build.Evaluation.Project project)
        {
            return project.Items.FirstOrDefault(
                item =>
                {
                    return item.ItemType.Equals("PackageReference") &&
                           item.EvaluatedInclude.Equals("zeroc.icebuilder.msbuild");
                }) != null;
        }
    }
}
