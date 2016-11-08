// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

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

        public static readonly string IceBuilderCppPropsPath =
            string.Format(@"$([System.IO.Directory]::GetFiles('{0}', '{1}', SearchOption.AllDirectories))",
                          @"$(LocalAppData)\Microsoft\VisualStudio\$(VisualStudioVersion)\Extensions",
                          "IceBuilder.Cpp.props");

        public static readonly string IceBuilderCppTargetsPath =
                    string.Format(@"$([System.IO.Directory]::GetFiles('{0}', '{1}', SearchOption.AllDirectories))",
                                  @"$(LocalAppData)\Microsoft\VisualStudio\$(VisualStudioVersion)\Extensions",
                                  "IceBuilder.Cpp.targets");

        public static readonly string IceBuilderCsharpPropsPath =
            string.Format(@"$([System.IO.Directory]::GetFiles('{0}', '{1}', SearchOption.AllDirectories))",
                          @"$(LocalAppData)\Microsoft\VisualStudio\$(VisualStudioVersion)\Extensions",
                          "IceBuilder.Csharp.props");

        public static readonly string IceBuilderCsharpTargetsPath =
                    string.Format(@"$([System.IO.Directory]::GetFiles('{0}', '{1}', SearchOption.AllDirectories))",
                                  @"$(LocalAppData)\Microsoft\VisualStudio\$(VisualStudioVersion)\Extensions",
                                  "IceBuilder.Csharp.targets");

        public static readonly string IceBuilderCppProps = "$(IceBuilderCppProps)";
        public static readonly string IceBuilderCppTargets = "$(IceBuilderCppTargets)";
        public static readonly string IceBuilderCsharpProps = "$(IceBuilderCsharpProps)";
        public static readonly string IceBuilderCsharpTargets = "$(IceBuilderCsharpTargets)";

        public static readonly string IceBuilderCppPropsPathOld =
            "$(LOCALAPPDATA)\\ZeroC\\IceBuilder\\IceBuilder.Cpp.props";

        public static readonly string IceBuilderCppTargetsPathOld =
            "$(LOCALAPPDATA)\\ZeroC\\IceBuilder\\IceBuilder.Cpp.targets";

        public static readonly string IceBuilderCSharpPropsPathOld =
            "$(LOCALAPPDATA)\\ZeroC\\IceBuilder\\IceBuilder.CSharp.props";

        public static readonly string IceBuilderCSharpTargetsPathOld =
            "$(LOCALAPPDATA)\\ZeroC\\IceBuilder\\IceBuilder.CSharp.targets";


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
            return project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(path)) != null;
        }

        public static bool AddProjectFlavorIfNotExists(Microsoft.Build.Evaluation.Project project, string flavor)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids"));

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
                p => p.Name.Equals("ProjectTypeGuids"));

            if(property != null && property.Value.IndexOf(flavor) != -1)
            {
                property.Value = property.Value.Replace(flavor, "").Trim(new char[] { ';' });
                if(property.Value.Equals(CSharpProjectGUI))
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
                p => p.Name.Equals("ProjectTypeGuids"));
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
                value = HasImport(project, IceBuilderCsharpProps) && HasImport(project, IceBuilderCsharpTargets);
                value = value || (HasImport(project, IceBuilderCSharpPropsPathOld) && HasImport(project, IceBuilderCSharpTargetsPathOld));
                value = value && HasProjectFlavor(project, IceBuilderProjectFlavorGUID);
            }
            return value;
        }

        private static bool AddGlobalProperty(Microsoft.Build.Evaluation.Project project, string name, string value)
        {
            ProjectPropertyGroupElement globals = project.Xml.PropertyGroups.FirstOrDefault(p => p.Label.Equals("Globals"));
            if(globals == null)
            {
                globals = project.Xml.AddPropertyGroup();
                globals.Label = "Globals";
                globals.Parent.RemoveChild(globals);
                project.Xml.InsertBeforeChild(globals, project.Xml.FirstChild);
            }

            ProjectPropertyElement property = globals.Properties.FirstOrDefault(p => p.Name.Equals(name));
            if(property == null)
            {
                property = globals.AddProperty(name, value);
                property.Condition = string.Format("!Exists('$({0})')", name);
                return true;
            }
            return false;
        }

        private static bool RemoveGlobalProperty(Microsoft.Build.Evaluation.Project project, string name)
        {
            ProjectPropertyGroupElement globals = project.Xml.PropertyGroups.FirstOrDefault(p => p.Label.Equals("Globals"));
            ProjectPropertyElement property = globals.Properties.FirstOrDefault(p => p.Name.Equals(name));
            if(property != null)
            {
                globals.RemoveChild(property);
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
            ProjectImportElement import = project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(oldValue));
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
            ProjectElement element = project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(import));
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

        private static bool AddCppGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = AddGlobalProperty(project, "IceBuilderCppProps", IceBuilderCppPropsPath);
            return AddGlobalProperty(project, "IceBuilderCppTargets", IceBuilderCppTargetsPath) || modified;
        }

        private static bool RemoveCppGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = RemoveGlobalProperty(project, "IceBuilderCppProps");
            return RemoveGlobalProperty(project, "IceBuilderCppTargets");
        }

        private static bool AddCsharpGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = AddGlobalProperty(project, "IceBuilderCsharpProps", IceBuilderCsharpPropsPath);
            return AddGlobalProperty(project, "IceBuilderCsharpTargets", IceBuilderCsharpTargetsPath) || modified;
        }

        private static bool RemoveCsharpGlobalProperties(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = RemoveGlobalProperty(project, "IceBuilderCsharpProps");
            return RemoveGlobalProperty(project, "IceBuilderCsharpTargets");
        }

        private static bool SetupCppProject(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = AddCppGlobalProperties(project);
            modified = AddImportAfter(project, IceBuilderCppProps,
                project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(@"$(VCTargetsPath)\Microsoft.Cpp.props"))) || modified;

            modified = AddImportAfter(project, IceBuilderCppTargets,
                project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(@"$(VCTargetsPath)\Microsoft.Cpp.targets"))) || modified;
            return modified;
        }

        private static bool SetupCsharpProject(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = AddCsharpGlobalProperties(project);
            modified = AddImportAfter(project, IceBuilderCsharpProps,
                project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets"))) || modified;

            modified = AddImportAfter(project, IceBuilderCsharpTargets,
                project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(@"$(IceBuilderCsharpProps)"))) || modified;

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
                modified = UpdateImport(project, IceBuilderCSharpPropsPathOld, IceBuilderCsharpProps) || modified;
                modified = UpdateImport(project, IceBuilderCSharpTargetsPathOld, IceBuilderCsharpTargets) || modified;
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
                    modified = RemoveImport(project, IceBuilderCsharpProps) || modified;
                    modified = RemoveImport(project, IceBuilderCsharpTargets) || modified;

                    RemoveProjectFlavorIfExists(project, IceBuilderProjectFlavorGUID);
                }

                ProjectPropertyGroupElement group = project.Xml.PropertyGroups.FirstOrDefault(g => g.Label.Equals("IceBuilder"));
                if(group != null)
                {
                    group.Parent.RemoveChild(group);
                }
            }
            return modified;
        }

        public static void SetProperty(Microsoft.Build.Evaluation.Project project, string label, string name, string value)
        {
            ProjectPropertyGroupElement group = project.Xml.PropertyGroups.FirstOrDefault(g => g.Label.Equals(label));
            if(group == null)
            {
                //
                // Create our property group after the main language targets are imported so we can use the properties
                // defined in this files.
                //
                ProjectImportElement import = project.Xml.Imports.FirstOrDefault(
                    p => (p.Project.IndexOf("Microsoft.Cpp.targets") != -1 || p.Project.Equals(IceBuilderCsharpProps)));
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

            ProjectPropertyElement property = group.Properties.FirstOrDefault(p => p.Name.Equals(name));
            if(property != null)
            {
                property.Value = value;
            }
            else
            {
                group.AddProperty(name, value);
            }
        }

        public static string GetProperty(Microsoft.Build.Evaluation.Project project, string name)
        {
            ProjectProperty property = project.GetProperty(name);
            return property != null ? property.UnevaluatedValue : string.Empty;
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
                        ProjectPropertyGroupElement group = import.ImportedProject.PropertyGroups.FirstOrDefault(g => g.Label.Equals("IceHome"));
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
