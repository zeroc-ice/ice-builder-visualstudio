// **********************************************************************
//
// Copyright (c) ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text.RegularExpressions;
using MSProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder
{
    public class ProjectUtil
    {
        public static void SaveProject(IVsProject project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ErrorHandler.ThrowOnFailure(Package.Instance.IVsSolution.SaveSolutionElement(
                (uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave, project as IVsHierarchy, 0));
        }

        //
        // Get the name of a IVsHierachy item give is item id.
        //
        public static string GetItemName(IVsProject project, uint itemid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            object value;
            (project as IVsHierarchy).GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out value);
            return value == null ? string.Empty : value.ToString();
        }

        public static IVsProject GetParentProject(IVsProject project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            object value = null;
            ErrorHandler.ThrowOnFailure(((IVsHierarchy)project).GetProperty(
                VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ParentHierarchy, out value));
            return value as IVsProject;
        }

        public static bool IsSliceFileName(string name)
        {
            return !string.IsNullOrEmpty(name) && Path.GetExtension(name).Equals(".ice");
        }

        public static string GetDefaultOutputDir(IVsProject project, bool evaluated)
        {
            return project.GetDefaultItemMetadata(ItemMetadataNames.OutputDir, evaluated);
        }

        public static string GetDefaultHeaderOutputDir(IVsProject project, bool evaluated)
        {
            string outputdir = project.GetDefaultItemMetadata(ItemMetadataNames.HeaderOutputDir, evaluated);
            if(string.IsNullOrEmpty(outputdir))
            {
                outputdir = GetDefaultOutputDir(project, evaluated);
            }
            return outputdir;
        }

        public static string GetProjectName(IVsProject project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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

        public static GeneratedFileSet
        GetCppGeneratedFiles(IVsProject project, EnvDTE.Project dteproject, VCUtil vcutil, string projectDir, string item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var fileset = new GeneratedFileSet
            {
                filename = item,
                sources = new Dictionary<string, List<string>>(),
                headers = new Dictionary<string, List<string>>()
            };

            var outputDir = project.GetItemMetadata(item, "OutputDir");
            var headerOutputDir = project.GetItemMetadata(item, "HeaderOutputDir");
            var headerExt = project.GetItemMetadata(item, "HeaderExt");
            var sourceExt = project.GetItemMetadata(item, "SourceExt");
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
                    fileset.sources[cppFilename].Add(ConfigurationString(configuration));
                }
                else
                {
                    var configurations = new List<string>();
                    configurations.Add(ConfigurationString(configuration));
                    fileset.sources[cppFilename] = configurations;
                }

                if(fileset.headers.ContainsKey(hFilename))
                {
                    fileset.headers[hFilename].Add(ConfigurationString(configuration));
                }
                else
                {
                    var configurations = new List<string>();
                    configurations.Add(ConfigurationString(configuration));
                    fileset.headers[hFilename] = configurations;
                }
            }
            return fileset;
        }

        public static string
        Evaluate(IVsBuildPropertyStorage propertyStorage, string configName, string input)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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

        public static GeneratedFileSet
        GetCsharpGeneratedFiles(IVsProject project, EnvDTE.Project dteproject, string projectDir, string item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var fileset = new GeneratedFileSet
            {
                filename = item,
                sources = new Dictionary<string, List<string>>(),
                headers = new Dictionary<string, List<string>>()
            };
            var propertyStorage = project as IVsBuildPropertyStorage;
            var outputDir = project.GetItemMetadata(item, "OutputDir");
            foreach(EnvDTE.Configuration configuration in dteproject.ConfigurationManager)
            {
                var configName = string.Format("{0}|{1}", configuration.ConfigurationName, configuration.PlatformName);
                var evaluatedOutputDir = Evaluate(propertyStorage, configName, outputDir);

                var csFilename = string.Format("{0}.cs", Path.GetFileNameWithoutExtension(item));
                csFilename = Path.GetFullPath(Path.Combine(projectDir, evaluatedOutputDir, csFilename));

                if(fileset.sources.ContainsKey(csFilename))
                {
                    fileset.sources[csFilename].Add(
                        string.Format("{0}|{1}", configuration.ConfigurationName, configuration.PlatformName));
                }
                else
                {
                    var configurations = new List<string>();
                    configurations.Add(
                        string.Format("{0}|{1}", configuration.ConfigurationName, configuration.PlatformName));
                    fileset.sources[csFilename] = configurations;
                }
            }
            return fileset;
        }

        public static bool
        CheckGenerateFileIsValid(IVsProject project, string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projectDir = project.GetProjectBaseDirectory();
            var dteproject = project.GetDTEProject();
            if(project.IsCSharpProject())
            {
                var fileset = GetCsharpGeneratedFiles(project, dteproject, projectDir, path);
                foreach(var source in fileset.sources.Keys)
                {
                    if (File.Exists(source))
                    {
                        const string message =
                        "A file named '{0}' already exists. If you want to add '{1}' first remove '{0}'.";

                        UIUtil.ShowErrorDialog("Ice Builder",
                            string.Format(message,
                                FileUtil.RelativePath(projectDir, source),
                                FileUtil.RelativePath(projectDir, path)));
                        return false;
                    }
                }
            }
            else
            {
                var fileset = GetCppGeneratedFiles(project, dteproject, ProjectFactoryHelperInstance.VCUtil, projectDir, path);

                foreach (var source in fileset.sources.Keys)
                {
                    if (File.Exists(source))
                    {
                        const string message =
                            "A file named '{0}' already exists. If you want to add '{1}' first remove '{0}' .";

                        UIUtil.ShowErrorDialog("Ice Builder",
                            string.Format(message,
                                FileUtil.RelativePath(projectDir, source),
                                FileUtil.RelativePath(projectDir, path)));
                        return false;
                    }
                }

                foreach (var header in fileset.headers.Keys)
                {
                    if (File.Exists(header))
                    {
                        const string message =
                            "A file named '{0}' already exists. If you want to add '{1}' first remove '{0}' .";

                        UIUtil.ShowErrorDialog("Ice Builder",
                            string.Format(message,
                                FileUtil.RelativePath(projectDir, header),
                                FileUtil.RelativePath(projectDir, path)));
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool TryAddItem(IVsProject project, string path)
        {
            var item = project.GetProjectItem(path);
            if (item == null)
            {
                project.EnsureIsCheckout();
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }
                //
                // Check again in case the item gets auto included once the file
                // has been created
                //
                item = project.GetProjectItem(path);
                if(item == null)
                {
                    project.AddFromFile(path);
                }
                return true;
            }
            return false;
        }

        public static void AddGeneratedFiles(IVsProject project, string file)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project.IsCppProject())
            {
                AddCppGeneratedFiles(project, file);
            }
            else
            {
                AddCSharpGeneratedFiles(project, file);
                //
                // Workaround for .NET Core project system adding duplicate
                // items when the added item is part of a glob
                project.RemoveGeneratedItemDuplicates();
            }
        }

        public static void AddCSharpGeneratedFiles(IVsProject project, string file)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projectDir = project.GetProjectBaseDirectory();
            var dteproject = project.GetDTEProject();
            var fileset = GetCsharpGeneratedFiles(project, dteproject, projectDir, file);

            //
            // First remove any generated items that are not in the
            // current generated items set for this Slice file
            //
            project.DeleteItems(project.WithProject((MSProject msproject) =>
                {
                    return msproject.AllEvaluatedItems.Where(
                        item =>
                        {
                            if(item.ItemType.Equals("Compile") && item.HasMetadata("SliceCompileSource"))
                            {
                                if(item.GetMetadataValue("SliceCompileSource").Equals(fileset.filename))
                                {
                                    return !fileset.sources.ContainsKey(Path.GetFullPath(Path.Combine(projectDir, item.EvaluatedInclude)));
                                }
                            }
                            return false;
                        }).Select(item => Path.Combine(projectDir, item.EvaluatedInclude)).ToList() ;
                }));

            foreach(var entry in fileset.sources)
            {
                AddCSharpGeneratedItem(project, dteproject,
                    projectDir,
                    FileUtil.RelativePath(projectDir, fileset.filename),
                    FileUtil.RelativePath(projectDir, entry.Key));
            }
        }

        public static void AddCppGeneratedFiles(IVsProject project, string file)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var vcutil = Package.Instance.VCUtil;
            var projectDir = project.GetProjectBaseDirectory();
            var dteproject = project.GetDTEProject();

            var fileset = GetCppGeneratedFiles(project, dteproject, vcutil, projectDir, file);

            var allConfigurations = new List<string>();
            foreach(EnvDTE.Configuration configuration in dteproject.ConfigurationManager)
            {
                allConfigurations.Add(ConfigurationString(configuration));
            }

            //
            // First remove any generated items that are not in the
            // current generated items set for this Slice file
            //
            project.DeleteItems(project.WithProject((MSProject msproject) =>
            {
                return msproject.AllEvaluatedItems.Where(
                    item =>
                    {
                        if (item.ItemType.Equals("ClCompile") && item.HasMetadata("SliceCompileSource"))
                        {
                            if (item.GetMetadataValue("SliceCompileSource").Equals(fileset.filename))
                            {
                                return !fileset.sources.ContainsKey(Path.GetFullPath(Path.Combine(projectDir, item.EvaluatedInclude)));
                            }
                        }
                        else if (item.ItemType.Equals("ClInclude") && item.HasMetadata("SliceCompileSource"))
                        {
                            if (item.GetMetadataValue("SliceCompileSource").Equals(fileset.filename))
                            {
                                return !fileset.headers.ContainsKey(Path.GetFullPath(Path.Combine(projectDir, item.EvaluatedInclude)));
                            }
                        }
                        return false;
                    }).Select(item => Path.Combine(projectDir, item.EvaluatedInclude)).ToList();
            }));

            foreach(var entry in fileset.sources)
            {
                AddCppGeneratedItem(project, dteproject, vcutil,
                    projectDir,
                    FileUtil.RelativePath(projectDir, fileset.filename),
                    FileUtil.RelativePath(projectDir, entry.Key),
                    "Source Files",
                    allConfigurations,
                    entry.Value);
            }

            foreach(var entry in fileset.headers)
            {
                AddCppGeneratedItem(project, dteproject, vcutil,
                    projectDir,
                    FileUtil.RelativePath(projectDir, fileset.filename),
                    FileUtil.RelativePath(projectDir, entry.Key), "Header Files", allConfigurations, entry.Value);
            }
        }

        public static void AddCppGeneratedItem(IVsProject project,
                                       EnvDTE.Project dteproject,
                                       VCUtil vcutil,
                                       string projectDir,
                                       string path,
                                       string generatedpath,
                                       string generatedfilter,
                                       List<string> allConfigurations,
                                       List<string> configurations)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (TryAddItem(project, Path.Combine(projectDir, generatedpath)))
            {
                var excludedConfigurations = allConfigurations.Where(c => !configurations.Contains(c)).ToList();
                project.SetGeneratedItemCustomMetadata(path, generatedpath, excludedConfigurations);

                //
                // If generated item applies only to one platform configuration we move it to the Platform/Configuration filter
                //
                if(configurations.Count == 1)
                {
                    string configurationName;
                    string platformName;
                    ParseConfiguration(configurations.First(), out configurationName, out platformName);
                    vcutil.AddGenerated(project, generatedpath, generatedfilter, platformName, configurationName);
                    dteproject.Save();
                }
            }
        }

        public static void AddCSharpGeneratedItem(IVsProject project,
                                                  EnvDTE.Project dteproject,
                                                  string projectDir,
                                                  string path,
                                                  string generatedpath)
        {
            if(TryAddItem(project, Path.Combine(projectDir, generatedpath)))
            {
                project.SetGeneratedItemCustomMetadata(path, generatedpath);
            }
        }

        public static void ParseConfiguration(string configuration, out string configurationName, out string platformName)
        {
            var tokens = configuration.Split('|');
            configurationName = tokens[0];
            platformName = tokens[1];
        }

        public static string ConfigurationString(EnvDTE.Configuration configuration)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return string.Format("{0}|{1}", configuration.ConfigurationName, configuration.PlatformName);
        }

        public static void SetupGenerated(IVsProject project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projectDir = project.GetProjectBaseDirectory();

            //
            // Remove all CompileClCompile and ClInclude items that have an associted SliceCompileSource
            // item metadata that doesn't much any of the project SliceCompile items
            //
            if(project.IsCppProject())
            {
                project.DeleteItems(project.WithProject((MSProject msproject) =>
                {
                    var sliceCompile = msproject.AllEvaluatedItems.Where(
                        item => item.ItemType.Equals("SliceCompile")).Select(
                        item => item.EvaluatedInclude);

                    return msproject.AllEvaluatedItems.Where(
                        item =>
                        {
                            if(item.ItemType.Equals("ClCompile") || item.ItemType.Equals("ClInclude"))
                            {
                                if(item.HasMetadata("SliceCompileSource"))
                                {
                                    var value = item.GetMetadataValue("SliceCompileSource");
                                    return !sliceCompile.Contains(value);
                                }
                            }
                            return false;
                        }).Select(item => Path.Combine(projectDir, item.EvaluatedInclude)).ToList();
                }));
            }
            else // C# project
            {
                project.DeleteItems(project.WithProject((MSProject msproject) =>
                    {
                        var sliceCompile = msproject.AllEvaluatedItems.Where(
                            item => item.ItemType.Equals("SliceCompile")).Select(
                            item => item.EvaluatedInclude);

                        return msproject.AllEvaluatedItems.Where(
                            item =>
                                {
                                    if(item.ItemType.Equals("Compile"))
                                    {
                                        if(item.HasMetadata("SliceCompileSource"))
                                        {
                                            var value = item.GetMetadataValue("SliceCompileSource");
                                            return !sliceCompile.Contains(value);
                                        }
                                    }
                                    return false;
                                }).Select(item => Path.Combine(projectDir, item.EvaluatedInclude)).ToList();
                    }));
            }

            // Now add any missing generated items
            var sliceItems = project.GetIceBuilderItems();
            foreach(var item in sliceItems)
            {
                AddGeneratedFiles(project, item);
            }
        }
    }
}
