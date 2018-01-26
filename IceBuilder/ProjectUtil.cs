// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Text.RegularExpressions;

namespace IceBuilder
{
    public class ProjectUtil
    {
        //
        // Get the Guid that idenifies the type of the project
        //
        public static Guid GetProjecTypeGuid(IVsProject project)
        {
            IVsHierarchy hierarchy = project as IVsHierarchy;
            if(hierarchy != null)
            {
                try
                {
                    Guid type;
                    ErrorHandler.ThrowOnFailure(hierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out type));
                    return type;
                }
                catch(Exception)
                {
                }
            }
            return new Guid();
        }
        public static void SaveProject(IVsProject project)
        {
            ErrorHandler.ThrowOnFailure(Package.Instance.IVsSolution.SaveSolutionElement(
                (uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave, project as IVsHierarchy, 0));
        }

        //
        // Get the name of a IVsHierachy item give is item id.
        //
        public static string GetItemName(IVsProject project, uint itemid)
        {
            object value;
            (project as IVsHierarchy).GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out value);
            return value == null ? string.Empty : value.ToString();
        }

        public static IVsProject GetParentProject(IVsProject project)
        {
            object value = null;
            ErrorHandler.ThrowOnFailure(((IVsHierarchy)project).GetProperty(
                VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ParentHierarchy, out value));
            return value as IVsProject;
        }

        public static List<string> GetIceBuilderItems(IVsProject project)
        {
            var msproject = project.GetMSBuildProject();
            return msproject.Items.Where(item => item.ItemType.Equals("SliceCompile"))
                                  .Select(item => item.EvaluatedInclude)
                                  .ToList();
        }

        public static void GetIceBuilderItems(IVsHierarchy h, uint itemId, ref List<String> items)
        {
            IntPtr nestedValue = IntPtr.Zero;
            uint nestedId = 0;
            Guid nestedGuid = typeof(IVsHierarchy).GUID;
            int result = h.GetNestedHierarchy(itemId, ref nestedGuid, out nestedValue, out nestedId);
            if(ErrorHandler.Succeeded(result) && nestedValue != IntPtr.Zero && nestedId == VSConstants.VSITEMID_ROOT)
            {
                // Get the nested hierachy
                IVsProject project = Marshal.GetObjectForIUnknown(nestedValue) as IVsProject;
                Marshal.Release(nestedValue);
                if(project != null)
                {
                    GetIceBuilderItems(project as IVsHierarchy, VSConstants.VSITEMID_ROOT, ref items);
                }
            }
            else
            {
                // Get the first visible child node
                object value;
                result = h.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_FirstVisibleChild, out value);
                while(result == VSConstants.S_OK && value != null)
                {
                    uint child = DTEUtil.GetItemId(value);
                    if(child == VSConstants.VSITEMID_NIL)
                    {
                        // No more nodes
                        break;
                    }
                    else
                    {
                        result = h.GetProperty(child, (int)__VSHPROPID.VSHPROPID_Name, out value);
                        string path = value as string;
                        if(IsSliceFileName(path))
                        {
                            items.Add(path);
                        }
                        GetIceBuilderItems(h, child, ref items);

                        // Get the next visible sibling node
                        result = h.GetProperty(child, (int)__VSHPROPID.VSHPROPID_NextVisibleSibling, out value);
                    }
                }
            }
        }

        public static bool IsSliceFileName(string name)
        {
            return !string.IsNullOrEmpty(name) && Path.GetExtension(name).Equals(".ice");
        }

        //
        // Non DTE
        //
        public static string GetCSharpGeneratedItemPath(string sliceName)
        {
            return GetGeneratedItemPath(sliceName, ".cs");
        }

        public static string GetCppGeneratedSourceItemPath(Microsoft.Build.Evaluation.Project project, string sliceName)
        {
            return GetGeneratedItemPath(sliceName, project.GetDefaultItemMetadata(ItemMetadataNames.SourceExt, true, ".cpp"));
        }

        public static string GetCppGeneratedHeaderItemPath(Microsoft.Build.Evaluation.Project project, string sliceName)
        {
            return GetGeneratedItemPath(sliceName, project.GetDefaultItemMetadata(ItemMetadataNames.HeaderExt, true, ".h"));
        }

        private static string GetGeneratedItemPath(string sliceName, string extension)
        {
            return Path.GetFileName(Path.ChangeExtension(sliceName, extension));
        }

        public static string GetPathRelativeToProject(IVsProject project, string path)
        {
            return FileUtil.RelativePath(GetProjectBaseDirectory(project), path);
        }

        public static string GetProjectBaseDirectory(IVsProject project)
        {
            string fullPath;
            ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out fullPath));
            return Path.GetFullPath(Path.GetDirectoryName(fullPath));
        }

        public static string GetProjectFullPath(IVsProject project)
        {
            try
            {
                string fullPath;
                ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out fullPath));
                return Path.GetFullPath(fullPath);
            }
            catch(NotImplementedException)
            {
                return string.Empty;
            }
        }

        public static EnvDTE.ProjectItem GetProjectItem(IVsProject project, uint item)
        {
            IVsHierarchy hierarchy = project as IVsHierarchy;
            object value = null;
            if(hierarchy != null)
            {
                hierarchy.GetProperty(item, (int)__VSHPROPID.VSHPROPID_ExtObject, out value);
            }
            return value as EnvDTE.ProjectItem;
        }

        public static string GetDefaultOutputDir(Microsoft.Build.Evaluation.Project project, bool evaluated)
        {
            return project.GetDefaultItemMetadata(ItemMetadataNames.OutputDir, evaluated);
        }

        public static string GetDefaultHeaderOutputDir(Microsoft.Build.Evaluation.Project project, bool evaluated)
        {
            string outputdir = project.GetDefaultItemMetadata(ItemMetadataNames.HeaderOutputDir, evaluated);
            if(string.IsNullOrEmpty(outputdir))
            {
                outputdir = GetDefaultOutputDir(project, evaluated);
            }
            return outputdir;
        }

        public static string GetEvaluatedProperty(IVsProject project, string name)
        {
            return GetEvaluatedProperty(project, name, string.Empty);
        }

        public static string GetEvaluatedProperty(IVsProject project, string name, string defaultValue)
        {
            var msproject = project.GetMSBuildProject(true);
            string value = msproject.GetEvaluatedProperty(name);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static string GetProjectName(IVsProject project)
        {
            IVsProject parent = GetParentProject(project);
            if(parent != null)
            {
                return Path.Combine(GetProjectName(parent), GetItemName(project, VSConstants.VSITEMID_ROOT));
            }
            else
            {
                return GetItemName(project, VSConstants.VSITEMID_ROOT);
            }
        }

        //
        // Using DTE
        //
        public static EnvDTE.ProjectItem FindProjectItem(string path)
        {
            return Package.Instance.DTE2.Solution.FindProjectItem(path);
        }

        public static Dictionary<string, List<string>>
        GetGeneratedFiles(List<GeneratedFileSet> filesets)
        {
            return filesets.ToDictionary(
                item => item.filename,
                item => item.sources.Keys.Union(item.headers.Keys).ToList());
        }

        public static List<GeneratedFileSet>
        GetCppGeneratedFiles(IVsProject project)
        {
            var generated = new List<GeneratedFileSet>();
            var msproject = project.GetMSBuildProject();
            var dteproject = project.GetDTEProject();
            var items = GetIceBuilderItems(project);
            var propertyStorage = project as IVsBuildPropertyStorage;
            var projectDir = GetProjectBaseDirectory(project);

            var vcutil = Package.Instance.VCUtil;

            foreach(var item in items)
            {
                var fileset = new GeneratedFileSet
                {
                    filename = item,
                    sources = new Dictionary<string, List<EnvDTE.Configuration>>(),
                    headers = new Dictionary<string, List<EnvDTE.Configuration>>()
                };

                var outputDir = msproject.GetItemMetadata(item, "OutputDir");
                var headerOutputDir = msproject.GetItemMetadata(item, "HeaderOutputDir");
                var headerExt = msproject.GetItemMetadata(item, "HeaderExt");
                var sourceExt = msproject.GetItemMetadata(item, "SourceExt");
                foreach(EnvDTE.Configuration configuration in dteproject.ConfigurationManager)
                {
                    var evaluatedOutputDir = vcutil.Evaluate(configuration, outputDir);
                    var evaluatedHeaderOutputDir = string.IsNullOrEmpty(headerOutputDir) ?
                        evaluatedOutputDir : vcutil.Evaluate(configuration, headerOutputDir);
                    var cppFilename = string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(item), sourceExt);
                    var hFilename = string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(item), headerExt);

                    cppFilename = Path.GetFullPath(Path.Combine(projectDir, evaluatedOutputDir, cppFilename));
                    hFilename = Path.GetFullPath(Path.Combine(projectDir, evaluatedHeaderOutputDir, hFilename));

                    if(fileset.sources.ContainsKey(cppFilename))
                    {
                        fileset.sources[cppFilename].Add(configuration);
                    }
                    else
                    {
                        var configurations = new List<EnvDTE.Configuration>();
                        configurations.Add(configuration);
                        fileset.sources[cppFilename] = configurations;
                    }

                    if(fileset.headers.ContainsKey(hFilename))
                    {
                        fileset.headers[hFilename].Add(configuration);
                    }
                    else
                    {
                        var configurations = new List<EnvDTE.Configuration>();
                        configurations.Add(configuration);
                        fileset.headers[hFilename] = configurations;
                    }
                }
                generated.Add(fileset);
            }
            return generated;
        }

        public static string
        Evaluate(IVsBuildPropertyStorage propertyStorage, string configName, string input)
        {
            const string pattern = @"\$\((\w+)\)";
            MatchCollection matches = Regex.Matches(input, pattern);
            var output = input;
            foreach(Match match in matches)
            {
                var name = match.Groups[1].Value;
                string value;
                propertyStorage.GetPropertyValue(name, configName, (uint)_PersistStorageType.PST_PROJECT_FILE, out value);
                output = output.Replace(string.Format("$({0})", name), value);
            }
            return output;
        }

        public static List<GeneratedFileSet>
        GetCsharpGeneratedFiles(IVsProject project)
        {
            var generated = new List<GeneratedFileSet>();
            var msproject = project.GetMSBuildProject();
            var dteproject = project.GetDTEProject();
            var items = GetIceBuilderItems(project);
            var propertyStorage = project as IVsBuildPropertyStorage;
            var projectDir = GetProjectBaseDirectory(project);

            foreach(var item in items)
            {
                var fileset = new GeneratedFileSet
                {
                    filename = item,
                    sources = new Dictionary<string, List<EnvDTE.Configuration>>(),
                    headers = new Dictionary<string, List<EnvDTE.Configuration>>()
                };

                var outputDir = msproject.GetItemMetadata(item, "OutputDir");
                foreach(EnvDTE.Configuration configuration in dteproject.ConfigurationManager)
                {
                    var configName = string.Format("{0}|{1}", configuration.ConfigurationName, configuration.PlatformName);
                    var evaluatedOutputDir = Evaluate(propertyStorage, configName, outputDir);

                    var csFilename = string.Format("{0}.cs", Path.GetFileNameWithoutExtension(item));
                    csFilename = Path.GetFullPath(Path.Combine(projectDir, evaluatedOutputDir, csFilename));

                    if(fileset.sources.ContainsKey(csFilename))
                    {
                        fileset.sources[csFilename].Add(configuration);
                    }
                    else
                    {
                        var configurations = new List<EnvDTE.Configuration>();
                        configurations.Add(configuration);
                        fileset.sources[csFilename] = configurations;
                    }
                }
                generated.Add(fileset);
            }
            return generated;
        }

        public static bool
        CheckGenerateFileIsValid(IVsProject project, IceBuilderProjectType projectType, string path)
        {
            var projectDir = project.GetProjectBaseDirectory();
            var msproject = project.GetMSBuildProject();
            if(projectType == IceBuilderProjectType.CsharpProjectType)
            {
                string outputDir = GetDefaultOutputDir(msproject, true);
                string generatedSource = Path.Combine(projectDir, outputDir, GetCSharpGeneratedItemPath(Path.GetFileName(path)));
                if(File.Exists(generatedSource))
                {
                    const string message =
                        "A file named '{0}' already exists.\nIf you want to add '{1}' first remove '{0}'.";

                    UIUtil.ShowErrorDialog("Ice Builder",
                        string.Format(message,
                            GetPathRelativeToProject(project, generatedSource),
                            GetPathRelativeToProject(project, path)));
                    return false;
                }
            }
            else if(projectType == IceBuilderProjectType.CppProjectType)
            {
                var dteproject = project.GetDTEProject();
                var outputDir = GetDefaultOutputDir(msproject, false);
                var headerOutputDir = GetDefaultHeaderOutputDir(msproject, false);
                var source = GetCppGeneratedSourceItemPath(msproject, path);
                var header = GetCppGeneratedHeaderItemPath(msproject, path);

                foreach(EnvDTE.Configuration config in dteproject.ConfigurationManager)
                {
                    var evaluatedOutputDir = Package.Instance.VCUtil.Evaluate(config, outputDir);
                    var evaluatedHeaderOutputDir = headerOutputDir.Equals(outputDir) ? evaluatedOutputDir :
                        Package.Instance.VCUtil.Evaluate(config, headerOutputDir);

                    string generatedSource = Path.GetFullPath(Path.Combine(projectDir, evaluatedOutputDir, source));
                    string generatedHeader = Path.GetFullPath(Path.Combine(projectDir, evaluatedHeaderOutputDir, header));

                    if(File.Exists(generatedSource) || File.Exists(generatedHeader))
                    {
                        const string message =
                            "A file named '{0}' or '{1}' already exists. If you want to add '{2}' first remove '{0}' and '{1}'.";

                        UIUtil.ShowErrorDialog("Ice Builder",
                            string.Format(message,
                                GetPathRelativeToProject(project, generatedSource),
                                GetPathRelativeToProject(project, generatedHeader),
                                GetPathRelativeToProject(project, path)));
                        return false;
                    }
                }
            }
            return true;
        }

        public static void DeleteItems(List<string> paths)
        {
            foreach(string path in paths)
            {
                EnvDTE.ProjectItem item = FindProjectItem(path);
                if(item != null)
                {
                    item.Remove();
                }

                if(File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch(IOException)
                    {
                        // can happen if the file is being used by other process
                    }
                }
            }
        }
        static List<string> KnownHeaderExtension = new List<string>(new string[] { ".h", ".hpp", ".hh", ".hxx" });
        public static void SetupGenerated(IVsProject project, IceBuilderProjectType type)
        {
            if(type == IceBuilderProjectType.CppProjectType)
            {
                //
                // This will ensure that property reads don't use a cached project.
                //
                project.GetMSBuildProject(false);

                var generated = GetCppGeneratedFiles(project);
                Package.Instance.FileTracker.Reap(GetProjectFullPath(project), GetGeneratedFiles(generated));
                Package.Instance.VCUtil.AddGeneratedFiles(project, generated);
            }
            else // C# project
            {
                var generated = GetCsharpGeneratedFiles(project);
                var dteproject = project.GetDTEProject();
                var activeConfiguration = dteproject.ConfigurationManager.ActiveConfiguration;
                Package.Instance.FileTracker.Reap(GetProjectFullPath(project), GetGeneratedFiles(generated));
                string configName = string.Format("{0}|{1}", activeConfiguration.ConfigurationName, activeConfiguration.PlatformName);
                foreach(var fileset in generated)
                {
                    if(fileset.sources.Count > 1)
                    {
                        const string message =
                            "The OutputDir SliceCompile item metadata must evaluate to the same value with all project configurations.";

                        UIUtil.ShowErrorDialog("Ice Builder", message);
                        break;
                    }

                    var file = fileset.sources.First().Key;
                    if(!Directory.Exists(Path.GetDirectoryName(file)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                    }

                    EnvDTE.ProjectItem item = FindProjectItem(file);
                    if(item == null)
                    {
                        if(!File.Exists(file))
                        {
                            File.Create(file).Dispose();
                        }
                        dteproject.ProjectItems.AddFromFile(file);
                        try
                        {
                            //
                            // Remove the file otherwise it will be considered up to date.
                            //
                            File.Delete(file);
                        }
                        catch(Exception)
                        {
                        }
                    }
                }
            }
        }
    }
}
