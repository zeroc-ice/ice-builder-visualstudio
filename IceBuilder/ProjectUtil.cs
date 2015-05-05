// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace IceBuilder
{
    public class ProjectUtil
    {
        public static bool IsSliceFileName(String name)
        {
            return !String.IsNullOrEmpty(name) && Path.GetExtension(name).Equals(".ice");
        }

        public static bool IsGeneratedItem(EnvDTE.ProjectItem item)
        {
            return item == null ? false :
                Package.Instance.FileTracker.Contains(item.ContainingProject, item.FileNames[1]);
        }

        public static String GetCSharpGeneratedItemFullPath(EnvDTE.Project project, String sliceName)
        {
            return GetGeneratedItemFullPath(project, sliceName, ".cs");
        }

        public static String GetCppGeneratedSourceItemFullPath(EnvDTE.Project project, String sliceName)
        {
            return GetGeneratedItemFullPath(project, sliceName, GetGeneratedSourceExtension(project));
        }

        public static String GetCppGeneratedHeaderItemFullPath(EnvDTE.Project project, String sliceName)
        {
            return GetGeneratedItemFullPath(project, sliceName, GetGeneratedHeaderExtension(project));
        }

        private static String GetGeneratedItemFullPath(EnvDTE.Project project, String sliceName, String extension)
        {
            EnvDTE.ProjectItem item = FindProjectItem(sliceName, project.ProjectItems);
            
            return Path.GetFullPath(
                Path.Combine(
                    (item == null ? GetOutputDir(project) : GetOutputDir(item)), 
                    Path.GetFileName(Path.ChangeExtension(sliceName, extension))));
        }

        public static String GetProjectBaseDirectory(EnvDTE.Project project)
        {
            return Path.GetFullPath(Path.GetDirectoryName(project.FullName));
        }

        public static String GetGeneratedSourceExtension(EnvDTE.Project project)
        {
            return GetEvaluatedProperty(project, PropertyNames.SourceExt);
        }

        public static String GetGeneratedHeaderExtension(EnvDTE.Project project)
        {
            return GetEvaluatedProperty(project, PropertyNames.HeaderExt);
        }

        public static String GetOutputDir(EnvDTE.ProjectItem item)
        {
            return GetOutputDir(item.ContainingProject);
        }

        public static String GetOutputDir(EnvDTE.Project project)
        {
            String outputdir = GetEvaluatedProperty(project, PropertyNames.OutputDir);
            String d = Path.GetFullPath(
                Path.Combine(Path.GetDirectoryName(project.FullName),
                             outputdir));
            return d;
        }

        public static String GetProperty(EnvDTE.Project project, String name)
        {
            return GetProperty(project, name, String.Empty);
        }

        public static String GetProperty(EnvDTE.Project project, String name, String defaultValue)
        {
            String value = MSBuildUtils.GetProperty(MSBuildUtils.LoadedProject(project.FullName), "IceBuilder", name);
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static void SetProperty(EnvDTE.Project project, String name, String value)
        {
            MSBuildUtils.SetProperty(MSBuildUtils.LoadedProject(project.FullName), "IceBuilder", name, value);
        }

        public static String GetEvaluatedProperty(EnvDTE.Project project, String name)
        {
            return GetEvaluatedProperty(project, name, String.Empty);
        }

        public static String GetEvaluatedProperty(EnvDTE.Project project, String name, String defaultValue)
        {
            String value = MSBuildUtils.GetEvaluatedProperty(MSBuildUtils.LoadedProject(project.FullName), name);
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }

       
        public static String GetPathRelativeToProject(EnvDTE.Project project, String path)
        {
            return FileUtil.RelativePath(GetProjectBaseDirectory(project), path);
        }

        public static String GetPathRelativeToProject(EnvDTE.ProjectItem item)
        {
            StringBuilder sb = new StringBuilder();
            if (item == null)
            {
                return String.Empty;
            }
            else
            {
                return Path.Combine(GetPathRelativeToProject(item.Collection.Parent as EnvDTE.ProjectItem), item.Name); 
            }
        }

        public static EnvDTE.ProjectItem FindProjectItem(String path, EnvDTE.ProjectItems items)
        {
            foreach(EnvDTE.ProjectItem i in items)
            {
                if(IsProjectItemFile(i))
                {
                    if (Path.GetFullPath(i.Properties.Item("FullPath").Value.ToString()).Equals(path))
                    {
                        return i;
                    }
                }
                else if (IsProjectItemFolder(i) || IsProjectItemFilter(i))
                {
                    EnvDTE.ProjectItem item = FindProjectItem(path, i.ProjectItems);
                    if (item != null)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public static bool IsProjectItemFolder(EnvDTE.ProjectItem item)
        {
            return item != null && !String.IsNullOrEmpty(item.Kind) &&
                   item.Kind.Equals("{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}");
        }

        public static bool IsProjectItemFilter(EnvDTE.ProjectItem item)
        {
            return item != null && !String.IsNullOrEmpty(item.Kind) &&
                   item.Kind.Equals("{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}");
        }

        public static bool IsProjectItemFile(EnvDTE.ProjectItem item)
        {
            return item != null && !String.IsNullOrEmpty(item.Kind) &&
                   item.Kind.Equals("{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}");
        }

        public static string GetProjectName(EnvDTE.Project project)
        {
            if(project.ParentProjectItem != null && project.ParentProjectItem.ContainingProject != null)
            {
                return Path.Combine(GetProjectName(project.ParentProjectItem.ContainingProject), project.Name);
            }
            else
            {
                return project.Name;
            }
        }

        public static Dictionary<String, List<String>> GetGeneratedFiles(EnvDTE.Project project)
        {
            Dictionary<String, List<String>> generated = new Dictionary<String, List<String>>();
            GetGeneratedFiles(project.ProjectItems, ref generated);
            return generated;
        }

        private static void
        GetGeneratedFiles(EnvDTE.ProjectItems items, ref Dictionary<String, List<String>> generated)
        {
            foreach (EnvDTE.ProjectItem item in items)
            {
                if(IsProjectItemFile(item) && IsSliceFileName(item.Name))
                {
                    generated[item.FileNames[1]] = GetGeneratedFiles(item.ContainingProject, item.FileNames[1]);
                }
                else if (IsProjectItemFolder(item) || IsProjectItemFilter(item))
                {
                    GetGeneratedFiles(item.ProjectItems, ref generated);
                }
            }
        }

        public static List<String> GetGeneratedFiles(EnvDTE.Project project, String sliceFile)
        {
            List<String> generated = new List<String>();
            if(DTEUtil.IsCSharpProject(project))
            {
                generated.Add(ProjectUtil.GetCSharpGeneratedItemFullPath(project, sliceFile));
            }
            else
            {
                generated.Add(ProjectUtil.GetCppGeneratedSourceItemFullPath(project, sliceFile));
                generated.Add(ProjectUtil.GetCppGeneratedHeaderItemFullPath(project, sliceFile));
            }
            return generated;
        }

        public static bool CheckGenerateFileIsValid(EnvDTE.Project project, String path)
        {
            if (DTEUtil.IsCSharpProject(project))
            {
                String generatedSource = ProjectUtil.GetCSharpGeneratedItemFullPath(project, path);
                if (File.Exists(generatedSource))
                {
                    const String message =
                        "A file named '{0}' already exists.\nIf you want to add '{1}' first remove '{0}'.";

                    UIUtil.ShowErrorDialog("Ice Builder",
                        String.Format(message,
                            ProjectUtil.GetPathRelativeToProject(project, generatedSource),
                            ProjectUtil.GetPathRelativeToProject(project, path)));
                    return false;
                }
            }
            else
            {
                String generatedSource = ProjectUtil.GetCppGeneratedSourceItemFullPath(project, path);
                String generatedHeader = ProjectUtil.GetCppGeneratedHeaderItemFullPath(project, path);
                if (File.Exists(generatedSource) || File.Exists(generatedHeader))
                {
                    const String message =
                        "A file named '{0}' or '{1}' already exists.\nIf you want to add '{2}' first remove '{0}' and '{1}'.";

                    UIUtil.ShowErrorDialog("Ice Builder",
                        String.Format(message,
                            ProjectUtil.GetPathRelativeToProject(project, generatedSource),
                            ProjectUtil.GetPathRelativeToProject(project, generatedHeader),
                            ProjectUtil.GetPathRelativeToProject(project, path)));
                    return false;
                }
            }
            return true;
        }

        public static void AddItems(EnvDTE.Project project, List<String> paths)
        {
            foreach (String path in paths)
            {
                String directoryName = Path.GetDirectoryName(path);
                if(!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                File.Create(path).Dispose();
                project.ProjectItems.AddFromFile(path);
            }
        }

        public static void DeleteItems(EnvDTE.Project project, List<String> paths)
        {
            foreach (String path in paths)
            {
                EnvDTE.ProjectItem item = ProjectUtil.FindProjectItem(path, project.ProjectItems);
                if (item != null)
                {
                    item.Remove();
                }

                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (System.IO.IOException)
                    {
                        // can happen if the file is being used by other process
                    }
                }
            }
        }

        //
        // Ensure that generated files are part of the project
        //
        public static void SetupGenerated(EnvDTE.Project project)
        {
            List<EnvDTE.ProjectItem> iceBuilderItems = new List<EnvDTE.ProjectItem>();
            IceBuilderItems(project.ProjectItems, ref iceBuilderItems);
            foreach (EnvDTE.ProjectItem i in iceBuilderItems)
            {
                SetupGenerated(i.ContainingProject, i.FileNames[1]);
            }
        }

        private static void IceBuilderItems(EnvDTE.ProjectItems allItems, ref List<EnvDTE.ProjectItem> iceBuilderItems)
        {
            foreach (EnvDTE.ProjectItem i in allItems)
            {
                if (IsProjectItemFile(i))
                {
                    if (IsSliceFileName(i.Name))
                    {
                        iceBuilderItems.Add(i);
                    }
                }
                else if (IsProjectItemFolder(i) || IsProjectItemFilter(i))
                {
                    IceBuilderItems(i.ProjectItems, ref iceBuilderItems);
                }
            }
        }

        private static void SetupGenerated(EnvDTE.Project project, String file)
        {
            List<String> generated = GetGeneratedFiles(project, file);
            foreach (String generatedFile in generated)
            {
                if (!Directory.Exists(Path.GetDirectoryName(generatedFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(generatedFile));
                }

                if(!File.Exists(generatedFile))
                {
                    File.Create(generatedFile).Dispose();
                }

                EnvDTE.ProjectItem item = FindProjectItem(generatedFile, project.ProjectItems);
                if(item == null)
                {
                    project.ProjectItems.AddFromFile(generatedFile);  
                }
            }
            Package.Instance.FileTracker.Add(project, file, generated);
        }
    }
}
