// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
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
                    return true; //ProjectTypeGuids updated
                }
                else
                {
                    return false; //ProjectTypeGuids already has this flavor
                }
            }

            // ProjectTypeGuids not present
            project.Xml.AddProperty("ProjectTypeGuids", string.Format("{0};{1}", flavor, CSharpProjectGUI));
            return true;
        }

        public static bool RemoveProjectFlavorIfExists(Microsoft.Build.Evaluation.Project project, string flavor)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids", StringComparison.CurrentCultureIgnoreCase));

            if(property != null && property.Value.IndexOf(flavor) != -1)
            {
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
            return project != null &&
                project.Xml.Imports.FirstOrDefault(p => p.Project.IndexOf("Microsoft.CSharp.targets") != -1) != null;
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

        private static bool AddGlobalProperty(Microsoft.Build.Evaluation.Project project, string name, string value)
        {
            ProjectPropertyGroupElement globals = project.Xml.PropertyGroups.FirstOrDefault(
                p => p.Label.Equals("Globals", StringComparison.CurrentCultureIgnoreCase));
            if(globals == null)
            {
                globals = project.Xml.AddPropertyGroup();
                globals.Label = "Globals";
                globals.Parent.RemoveChild(globals);
                project.Xml.InsertBeforeChild(globals, project.Xml.FirstChild);
            }

            ProjectPropertyElement property = globals.Properties.FirstOrDefault(
                p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if(property == null)
            {
                property = globals.AddProperty(name, value);
                return true;
            }
            return false;
        }

        public static bool EnsureIceBuilderImports(Microsoft.Build.Evaluation.Project project)
        {
            var target = project.Xml.Targets.FirstOrDefault(
                t => t.Name.Equals("EnsureIceBuilderImports", StringComparison.CurrentCultureIgnoreCase));
            if(target == null)
            {
                target = project.Xml.AddTarget("EnsureIceBuilderImports");
                target.BeforeTargets = "PrepareForBuild";

                var propertyGroup = target.AddPropertyGroup();
                propertyGroup.AddProperty("ErrorText", EnsureIceBuilderImportsError);

                var error = target.AddTask("Error");
                if(IsCppProject(project))
                {
                    error.Condition = "!Exists('$(IceBuilderCppProps)')";
                }
                else
                {
                    error.Condition = "!Exists('$(IceBuilderCSharpProps)')";
                }
                error.SetParameter("Text", "$(ErrorText)");
                return true;
            }
            return false;
        }

        private static bool RemoveGlobalProperty(Microsoft.Build.Evaluation.Project project, string name)
        {
            ProjectPropertyGroupElement globals = project.Xml.PropertyGroups.FirstOrDefault(
                p => p.Label.Equals("Globals", StringComparison.CurrentCultureIgnoreCase));
            ProjectPropertyElement property = globals.Properties.FirstOrDefault(
                p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if(property != null)
            {
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
                property.Parent.RemoveChild(property);
                return true;
            }
            return false;
        }

        private static bool AddImportAfter(Microsoft.Build.Evaluation.Project project,
                                           string import,
                                           ProjectElement after)
        {
            if(!HasImport(project, import))
            {
                ProjectImportElement props = project.Xml.CreateImportElement(import);
                props.Condition = string.Format("Exists('{0}')", import);
                project.Xml.InsertAfterChild(props, after);
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

        private static bool AddGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            return AddGlobalProperty(project, "IceBuilderInstallDir", IceBuilderInstallDir);
        }

        private static bool RemoveGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            return RemoveGlobalProperty(project, "IceBuilderInstallDir");
        }

        private static bool AddCppGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = AddGlobalProperties(project);
            if(modified)
            {
                RemoveGlobalProperty(project, "IceBuilderCppProps");
                RemoveGlobalProperty(project, "IceBuilderCppTargets");
            }
            modified = AddGlobalProperty(project, "IceBuilderCppProps", IceBuilderCppPropsPath) || modified;
            return AddGlobalProperty(project, "IceBuilderCppTargets", IceBuilderCppTargetsPath) || modified;
        }

        private static bool RemoveCppGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = RemoveGlobalProperties(project);
            modified = RemoveGlobalProperty(project, "IceBuilderCppProps") || modified;
            return RemoveGlobalProperty(project, "IceBuilderCppTargets");
        }

        private static bool AddCsharpGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = AddGlobalProperties(project);
            if(modified)
            {
                RemoveGlobalProperty(project, "IceBuilderCsharpProps");
                RemoveGlobalProperty(project, "IceBuilderCsharpTargets");
            }
            modified = AddGlobalProperty(project, "IceBuilderCsharpProps", IceBuilderCSharpPropsPath) || modified;
            return AddGlobalProperty(project, "IceBuilderCsharpTargets", IceBuilderCSharpTargetsPath) || modified;
        }

        private static bool RemoveCsharpGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = RemoveGlobalProperties(project);
            modified = RemoveGlobalProperty(project, "IceBuilderCsharpProps") || modified;
            return RemoveGlobalProperty(project, "IceBuilderCsharpTargets");
        }

        private static bool SetupCppProject(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = AddCppGlobalProperties(project);
            modified = AddImportAfter(project, IceBuilderCppProps,
                project.Xml.Imports.FirstOrDefault(
                    p => p.Project.Equals(@"$(VCTargetsPath)\Microsoft.Cpp.props",
                                          StringComparison.CurrentCultureIgnoreCase))) || modified;

            modified = AddImportAfter(project, IceBuilderCppTargets,
                project.Xml.Imports.FirstOrDefault(
                    p => p.Project.Equals(@"$(VCTargetsPath)\Microsoft.Cpp.targets",
                                          StringComparison.CurrentCultureIgnoreCase))) || modified;
            return modified;
        }

        private static bool SetupCsharpProject(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = AddCsharpGlobalProperties(project);
            modified = AddImportAfter(project, IceBuilderCSharpProps,
                project.Xml.Imports.FirstOrDefault(
                    p => p.Project.EndsWith(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets",
                                          StringComparison.CurrentCultureIgnoreCase) ||
                         p.Project.Equals(@"$(MSBuildBinPath)\Microsoft.CSharp.targets",
                                          StringComparison.CurrentCultureIgnoreCase))) || modified;

            modified = AddImportAfter(project, IceBuilderCSharpTargets,
                project.Xml.Imports.FirstOrDefault(
                    p => p.Project.Equals(@"$(IceBuilderCSharpProps)",
                                          StringComparison.CurrentCultureIgnoreCase))) || modified;

            modified = AddProjectFlavorIfNotExists(project, IceBuilderProjectFlavorGUID) || modified;
            return modified;
        }

        public static bool UpgradeProjectImports(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;
            if(IsCppProject(project))
            {
                modified = AddCppGlobalProperties(project);
                modified = UpdateImport(project, IceBuilderCppPropsPathOld, IceBuilderCppProps) || modified;
                modified = UpdateImport(project, IceBuilderCppTargetsPathOld, IceBuilderCppTargets) || modified;
            }
            else if(IsCSharpProject(project))
            {
                modified = AddCsharpGlobalProperties(project);
                modified = UpdateImport(project, IceBuilderCSharpPropsPathOld, IceBuilderCSharpProps) || modified;
                modified = UpdateImport(project, IceBuilderCSharpTargetsPathOld, IceBuilderCSharpTargets) || modified;
            }
            return modified;
        }

        public static bool UpgradeProjectProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;
            string value = GetProperty(project, PropertyNames.AllowIcePrefix, false);
            string additionalOptions = GetProperty(project, PropertyNames.AdditionalOptions);
            if(!string.IsNullOrEmpty(value))
            {
                if(value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = String.Format("{0} --ice", additionalOptions).Trim();
                }
                RemoveProperty(project, PropertyNames.AllowIcePrefix);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Underscore, false);
            if (!string.IsNullOrEmpty(value))
            {
                if(value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = String.Format("{0} --underscore", additionalOptions).Trim();
                }
                RemoveProperty(project, PropertyNames.Underscore);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Stream, false);
            if(!string.IsNullOrEmpty(value))
            {
                if(value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                   value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    additionalOptions = String.Format("{0} --stream ", additionalOptions).Trim();
                }
                RemoveProperty(project, PropertyNames.Stream);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.DLLExport, false);
            if(!string.IsNullOrEmpty(value))
            {
                additionalOptions = String.Format("{0} --dll-export {1}", additionalOptions, value).Trim();
                RemoveProperty(project, PropertyNames.DLLExport);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Checksum, false);
            if(!string.IsNullOrEmpty(value))
            {
                additionalOptions = String.Format("{0} --checksum", additionalOptions).Trim();
                RemoveProperty(project, PropertyNames.Checksum);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.Tie, false);
            if(!string.IsNullOrEmpty(value))
            {
                additionalOptions = String.Format("{0} --tie", additionalOptions).Trim();
                RemoveProperty(project, PropertyNames.Tie);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.DLLExport, false);
            if(!string.IsNullOrEmpty(value))
            {
                additionalOptions = String.Format("{0} --dll-export {1}", additionalOptions, value).Trim();
                RemoveProperty(project, PropertyNames.DLLExport);
                modified = true;
            }

            value = GetProperty(project, PropertyNames.BaseDirectoryForGeneratedInclude, false);
            if(!string.IsNullOrEmpty(value))
            {
                additionalOptions = String.Format("{0} --include-dir {1}", additionalOptions, value).Trim();
                RemoveProperty(project, PropertyNames.BaseDirectoryForGeneratedInclude);
                modified = true;
            }

            if(modified)
            {
                SetProperty(project, "IceBuilder", PropertyNames.AdditionalOptions, additionalOptions);
            }

            return modified;
        }

        public static bool AddIceBuilderToProject(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;
            if(project != null)
            {
                if(IsCppProject(project))
                {
                    modified = SetupCppProject(project);
                }
                else if(IsCSharpProject(project))
                {
                    modified = SetupCsharpProject(project);
                }
                modified = EnsureIceBuilderImports(project) || modified;
            }
            return modified;
        }

        public static bool RemoveIceBuilderFromProject(Microsoft.Build.Evaluation.Project project)
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

                    RemoveProjectFlavorIfExists(project, IceBuilderProjectFlavorGUID);
                }

                ProjectPropertyGroupElement group = project.Xml.PropertyGroups.FirstOrDefault(
                    g => g.Label.Equals("IceBuilder", StringComparison.CurrentCultureIgnoreCase));
                if(group != null)
                {
                    group.Parent.RemoveChild(group);
                }

                //
                // Remove EnsureIceBuilderImports target
                //
                var target = project.Xml.Targets.FirstOrDefault(
                    t => t.Name.Equals("EnsureIceBuilderImports", StringComparison.CurrentCultureIgnoreCase));
                if(target != null)
                {
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
                          p.Project.Equals(IceBuilderCSharpProps, StringComparison.CurrentCultureIgnoreCase)));
                if(import != null)
                {
                    group = project.Xml.CreatePropertyGroupElement();
                    project.Xml.InsertAfterChild(group, import);
                }
                else
                {
                    group = project.Xml.CreatePropertyGroupElement();
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

        //
        // Set Ice Home and force projects to re evaluate changes in the imported project
        //
        public static void SetIceHome(List<IVsProject> projects, string iceHome, string iceVersion, string iceIntVersion, string iceVersionMM)
        {
            foreach(IVsProject p in projects)
            {
                if(DTEUtil.IsIceBuilderEnabled(p) != IceBuilderProjectType.None)
                {
                    Microsoft.Build.Evaluation.Project project = LoadedProject(ProjectUtil.GetProjectFullPath(p), DTEUtil.IsCppProject(p), true);
                    ResolvedImport import = project.Imports.FirstOrDefault(i => i.ImportedProject.FullPath.EndsWith("IceBuilder.Common.props"));

                    if(import.ImportedProject != null)
                    {
                        ProjectPropertyGroupElement group = import.ImportedProject.PropertyGroups.FirstOrDefault(
                            g => g.Label.Equals("IceHome", StringComparison.CurrentCultureIgnoreCase));
                        if(group != null)
                        {
                            group.SetProperty(Package.IceHomeValue, iceHome);
                            group.SetProperty(Package.IceVersionValue, iceVersion);
                            group.SetProperty(Package.IceIntVersionValue, iceIntVersion);
                            group.SetProperty(Package.IceVersionMMValue, iceVersionMM);
                            project.ReevaluateIfNecessary();
                        }
                    }
                }
            }
        }
    }
}
