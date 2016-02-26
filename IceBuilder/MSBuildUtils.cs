// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    class MSBuildUtils
    {
        public static readonly String IceBuilderProjectFlavorGUID = "{3C53C28F-DC44-46B0-8B85-0C96B85B2042}";
        public static readonly String CSharpProjectGUI = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

        public static readonly String IceCppPropsPath =
            "$(LOCALAPPDATA)\\ZeroC\\IceBuilder\\IceBuilder.Cpp.props";

        public static readonly String IceCppTargetsPath =
            "$(LOCALAPPDATA)\\ZeroC\\IceBuilder\\IceBuilder.Cpp.targets";

        public static readonly String IceCSharpPropsPath =
            "$(LOCALAPPDATA)\\ZeroC\\IceBuilder\\IceBuilder.CSharp.props";

        public static readonly String IceCSharpTargetsPath =
            "$(LOCALAPPDATA)\\ZeroC\\IceBuilder\\IceBuilder.CSharp.targets";


        public static Microsoft.Build.Evaluation.Project LoadedProject(String path)
        {
            return LoadedProject(path, true);
        }

        public static Microsoft.Build.Evaluation.Project LoadedProject(String path, bool cached)
        {
            Microsoft.Build.Evaluation.Project project = null;
            ICollection<Microsoft.Build.Evaluation.Project> projects = 
                ProjectCollection.GlobalProjectCollection.GetLoadedProjects(path);


            if (projects.Count == 0)
            {
                project = new Microsoft.Build.Evaluation.Project(path);
            }
            else
            {
                project = projects.First();
                if(!cached)
                {
                    //
                    // That is required to get C++ project properties re-evaluated
                    // with Visual Studio 2013 and Visual Studio 2015
                    //
                    ProjectCollection.GlobalProjectCollection.UnloadProject(project);
                    ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
                    project = ProjectCollection.GlobalProjectCollection.LoadProject(path);
                }
            }
            return project;
        }

        public static bool HasImport(Microsoft.Build.Evaluation.Project project, String path)
        {
            return project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(path)) != null;
        }

        public static bool AddProjectFlavorIfNotExists(Microsoft.Build.Evaluation.Project project, String flavor)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids"));

            if(property != null)
            {
                if(property.Value.IndexOf(flavor) == -1)
                {
                    if(String.IsNullOrEmpty(property.Value))
                    {
                        property.Value = String.Format("{0};{1}", flavor, CSharpProjectGUI);
                    }
                    else
                    {
                        property.Value = String.Format("{0};{1}", flavor, property.Value);
                    }
                    return true; //ProjectTypeGuids updated
                }
                else
                {
                    return false; //ProjectTypeGuids already has this flavor
                }
            }

            // ProjectTypeGuids not present
            project.Xml.AddProperty("ProjectTypeGuids", String.Format("{0};{1}", flavor, CSharpProjectGUI));
            return true;
        }

        public static bool RemoveProjectFlavorIfExists(Microsoft.Build.Evaluation.Project project, String flavor)
        {
            ProjectPropertyElement property = project.Xml.Properties.FirstOrDefault(
                p => p.Name.Equals("ProjectTypeGuids"));

            if (property != null && property.Value.IndexOf(flavor) != -1)
            {
                property.Value = property.Value.Replace(flavor, "").Trim(new char[] { ';' });
                if (property.Value.Equals(CSharpProjectGUI))
                {
                    property.Parent.RemoveChild(property);
                }
                return true; //flavor removed
            }
            return false;
        }

        public static bool HasProjectFlavor(Microsoft.Build.Evaluation.Project project, String flavor)
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
            if (IsCppProject(project))
            {
                value = HasImport(project, IceCppPropsPath) &&
                       HasImport(project, IceCppTargetsPath);
            }
            else if(IsCSharpProject(project))
            {
                value = HasImport(project, IceCSharpPropsPath) &&
                       HasImport(project, IceCSharpTargetsPath) &&
                       HasProjectFlavor(project, IceBuilderProjectFlavorGUID);
            }

            return value;
        }

        public static bool AddIceBuilderToProject(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;
            if (project != null)
            {
                if (IsCppProject(project))
                {
                    if(!HasImport(project, IceCppPropsPath))
                    {
                        ProjectImportElement iceBuilderProps = project.Xml.CreateImportElement(IceCppPropsPath);
                        ProjectImportElement vcProps = project.Xml.Imports.FirstOrDefault(p => p.Project.IndexOf("Microsoft.Cpp.props") != -1);
                        project.Xml.InsertAfterChild(iceBuilderProps, vcProps);
                        modified = true;
                    }

                    if(!HasImport(project, IceCppTargetsPath))
                    {
                        ProjectImportElement iceBuilderTargets = project.Xml.CreateImportElement(IceCppTargetsPath);
                        ProjectImportElement vcTargets = project.Xml.Imports.FirstOrDefault(p => p.Project.IndexOf("Microsoft.Cpp.targets") != -1);
                        project.Xml.InsertAfterChild(iceBuilderTargets, vcTargets);
                        modified = true;
                    }
                }
                else if (IsCSharpProject(project))
                {
                    if (!HasImport(project, IceCSharpPropsPath))
                    {
                        ProjectImportElement iceBuilderProps = project.Xml.CreateImportElement(IceCSharpPropsPath);
                        ProjectImportElement vcProps = project.Xml.Imports.FirstOrDefault(p => p.Project.IndexOf("Microsoft.CSharp.targets") != -1);
                        project.Xml.InsertAfterChild(iceBuilderProps, vcProps);
                        modified = true;
                    }

                    if(!HasImport(project, IceCSharpTargetsPath))
                    {
                        ProjectImportElement iceBuilderTargets = project.Xml.CreateImportElement(IceCSharpTargetsPath);
                        ProjectImportElement vcTargets = project.Xml.Imports.FirstOrDefault(p => p.Project.IndexOf("IceBuilder.CSharp.props") != -1);
                        project.Xml.InsertAfterChild(iceBuilderTargets, vcTargets);
                        modified = true;
                    }

                    if(!HasProjectFlavor(project, IceBuilderProjectFlavorGUID))
                    {
                        AddProjectFlavorIfNotExists(project, IceBuilderProjectFlavorGUID);
                    }
                }
            }
            return modified;
        }

        public static bool RemoveIceBuilderFromProject(Microsoft.Build.Evaluation.Project project)
        {
            bool modified = false;
            if (project != null)
            {
                if (IsCppProject(project))
                {
                    ProjectImportElement iceBuilderProps = project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(IceCppPropsPath));
                    if (iceBuilderProps != null)
                    {
                        project.Xml.RemoveChild(iceBuilderProps);
                        modified = true;
                    }

                    ProjectImportElement iceBuilderTargets = project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(IceCppTargetsPath));
                    if (iceBuilderTargets != null)
                    {
                        project.Xml.RemoveChild(iceBuilderTargets);
                        modified = true;
                    }
                }
                else if (IsCSharpProject(project))
                {
                    ProjectImportElement iceBuilderProps = project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(IceCSharpPropsPath));
                    if (iceBuilderProps != null)
                    {
                        project.Xml.RemoveChild(iceBuilderProps);
                        modified = true;
                    }

                    ProjectImportElement iceBuilderTargets = project.Xml.Imports.FirstOrDefault(p => p.Project.Equals(IceCSharpTargetsPath));
                    if (IceCppTargetsPath != null)
                    {
                        project.Xml.RemoveChild(iceBuilderTargets);
                        modified = true;
                    }

                    RemoveProjectFlavorIfExists(project, IceBuilderProjectFlavorGUID);
                }

                ProjectPropertyGroupElement group = project.Xml.PropertyGroups.FirstOrDefault(g => g.Label.Equals("IceBuilder"));
                if (group != null)
                {
                    group.Parent.RemoveChild(group);
                }
            }
            return modified;
        }

        public static void SetProperty(Microsoft.Build.Evaluation.Project project, String label, String name, String value)
        {
            ProjectPropertyGroupElement group = project.Xml.PropertyGroups.FirstOrDefault(g => g.Label.Equals(label));
            if(group == null)
            {
                //
                // Create our property group after the main language targets are imported so we can use the properties
                // defined in this files.
                //
                ProjectImportElement import = project.Xml.Imports.FirstOrDefault(
                    p => (p.Project.IndexOf("Microsoft.Cpp.targets") != -1 || p.Project.IndexOf("IceBuilder.CSharp.props") != -1));
                if (import != null)
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
            if (property != null)
            {
                property.Value = value;
            }
            else
            {
                group.AddProperty(name, value);
            }
        }

        public static String GetProperty(Microsoft.Build.Evaluation.Project project, String name)
        {
            ProjectProperty property = project.GetProperty(name);
            return property != null ? property.UnevaluatedValue : String.Empty;
        }

        public static String GetEvaluatedProperty(Microsoft.Build.Evaluation.Project project, String name)
        {
            return project.GetPropertyValue(name);
        }

        //
        // Set Ice Home and force projects to re evaluate changes in the imported project
        //
        public static void SetIceHome(List<IVsProject> projects, String iceHome, String iceVersion, String iceIntVersion, String iceVersionMM)
        {
            foreach (IVsProject p in projects)
            {
                if(DTEUtil.IsIceBuilderEnabled(p) != IceBuilderProjectType.None)
                {
                    Microsoft.Build.Evaluation.Project project = MSBuildUtils.LoadedProject(ProjectUtil.GetProjectFullPath(p));
                    ResolvedImport import = project.Imports.FirstOrDefault(i => i.ImportedProject.FullPath.EndsWith("IceBuilder.Common.props"));

                    if (import.ImportedProject != null)
                    {
                        ProjectPropertyGroupElement group = import.ImportedProject.PropertyGroups.FirstOrDefault(g => g.Label.Equals("IceHome"));
                        if (group != null)
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
