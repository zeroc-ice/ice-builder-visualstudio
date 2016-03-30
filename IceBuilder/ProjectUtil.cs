// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

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
        public static String GetItemName(IVsProject project, uint itemid)
        {
            Object value;
            (project as IVsHierarchy).GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out value);
            return value == null ? String.Empty : value.ToString();
        }

        public static IVsProject GetParentProject(IVsProject project)
        {
            Object value = null;
            ErrorHandler.ThrowOnFailure(((IVsHierarchy)project).GetProperty(
                VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ParentHierarchy, out value));
            return value as IVsProject;
        }

        public static List<String> GetIceBuilderItems(IVsProject project)
        {
            IVsProject4 project4 = project as IVsProject4;
            return project4 != null ? GetIceBuilderItems(project4) : GetIceBuilderItems(project as IVsHierarchy);
        }

        public static List<String> GetIceBuilderItems(IVsProject4 project)
        {
            List<String> items = new List<String>();
            uint sz = 0;
            project.GetFilesWithItemType("IceBuilder", 0, null, out sz);
            if (sz > 0)
            {
                uint[] ids = new uint[sz];
                project.GetFilesWithItemType("IceBuilder", sz, ids, out sz);
                foreach (uint id in ids)
                {
                    items.Add(GetItemName(project, id));
                }
            }
            return items;
        }
        public static List<String> GetIceBuilderItems(IVsHierarchy project)
        {
            List<String> items = new List<String>();
            GetIceBuilderItems(project, VSConstants.VSITEMID_ROOT, ref items);
            return items;
        }

        public static void GetIceBuilderItems(IVsHierarchy h, uint itemId, ref List<String> items)
        {
            IntPtr nestedValue = IntPtr.Zero;
            uint nestedId = 0;
            Guid nestedGuid = typeof(IVsHierarchy).GUID;
            int result = h.GetNestedHierarchy(itemId, ref nestedGuid, out nestedValue, out nestedId);
            if (ErrorHandler.Succeeded(result) && nestedValue != IntPtr.Zero && nestedId == VSConstants.VSITEMID_ROOT)
            {
                // Get the nested hierachy
                IVsProject project = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(nestedValue) as IVsProject;
                System.Runtime.InteropServices.Marshal.Release(nestedValue);
                if (project != null)
                {
                    GetIceBuilderItems(project as IVsHierarchy, VSConstants.VSITEMID_ROOT, ref items);
                }
            }
            else
            {
                // Get the first visible child node
                object value;
                result = h.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_FirstVisibleChild, out value);
                while (result == VSConstants.S_OK && value != null)
                {
                    if (value is int && (uint)(int)value == VSConstants.VSITEMID_NIL)
                    {
                        // No more nodes
                        break;
                    }
                    else
                    {
                        uint child = Convert.ToUInt32(value);

                        value = null;
                        result = h.GetProperty(child, (int)__VSHPROPID.VSHPROPID_Name, out value);
                        String path = value as String;
                        if(ProjectUtil.IsSliceFileName(path))
                        {
                            items.Add(path);
                        }

                        GetIceBuilderItems(h, child, ref items);

                        // Get the next visible sibling node
                        value = null;
                        result = h.GetProperty(child, (int)__VSHPROPID.VSHPROPID_NextVisibleSibling, out value);
                    }
                }
            }
        }

        public static bool IsSliceFileName(String name)
        {
            return !String.IsNullOrEmpty(name) && Path.GetExtension(name).Equals(".ice");
        }

        //
        // Non DTE
        //
        public static String GetCSharpGeneratedItemPath(String sliceName)
        {
            return GetGeneratedItemPath(sliceName, ".cs");
        }

        public static String GetCppGeneratedSourceItemPath(IVsProject project, String sliceName)
        {
            return GetGeneratedItemPath(sliceName, GetEvaluatedProperty(project, PropertyNames.SourceExt, ".h"));
        }

        public static String GetCppGeneratedHeaderItemPath(IVsProject project, String sliceName)
        {
            return GetGeneratedItemPath(sliceName, GetEvaluatedProperty(project, PropertyNames.HeaderExt, ".h"));
        }

        private static String GetGeneratedItemPath(String sliceName, String extension)
        {
            return Path.GetFileName(Path.ChangeExtension(sliceName, extension));
        }

        public static String GetPathRelativeToProject(IVsProject project, String path)
        {
            return FileUtil.RelativePath(GetProjectBaseDirectory(project), path);
        }

        public static String GetProjectBaseDirectory(IVsProject project)
        {
            String fullPath;
            ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out fullPath));
            return Path.GetFullPath(Path.GetDirectoryName(fullPath));
        }

        public static String GetProjectFullPath(IVsProject project)
        {
            try
            {
                String fullPath;
                ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out fullPath));
                return Path.GetFullPath(fullPath);
            }
            catch(NotImplementedException)
            {
                return String.Empty;
            }
        }

        public static EnvDTE.ProjectItem GetProjectItem(IVsProject project, uint item)
        {
            IVsHierarchy hierarchy = project as IVsHierarchy;
            object value = null;
            if (hierarchy != null)
            {
                hierarchy.GetProperty(item, (int)__VSHPROPID.VSHPROPID_ExtObject, out value);
            }
            return value as EnvDTE.ProjectItem;
        }

        public static String GetOutputDir(IVsProject project, bool isHeader, bool evaluated)
        {
            String outputdir = null;
            if (isHeader)
            {
                outputdir = evaluated ? GetEvaluatedProperty(project, PropertyNames.HeaderOutputDir) :
                                        GetProperty(project, PropertyNames.HeaderOutputDir);
            }

            if (String.IsNullOrEmpty(outputdir))
            {
                outputdir = evaluated ? GetEvaluatedProperty(project, PropertyNames.OutputDir) :
                                        GetProperty(project, PropertyNames.OutputDir);
            }
            if (evaluated)
            {
                return Path.GetFullPath(Path.Combine(GetProjectBaseDirectory(project), outputdir));
            }
            else
            {
                return outputdir;
            }
        }

        public static String GetProperty(IVsProject project, String name)
        {
            return GetProperty(project, name, String.Empty);
        }

        public static String GetProperty(IVsProject project, String name, String defaultValue)
        {
            String value = MSBuildUtils.GetProperty(MSBuildUtils.LoadedProject(GetProjectFullPath(project), DTEUtil.IsCppProject(project), true), name);
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static void SetProperty(IVsProject project, String name, String value)
        {
            MSBuildUtils.SetProperty(MSBuildUtils.LoadedProject(GetProjectFullPath(project), DTEUtil.IsCppProject(project), true), "IceBuilder", name, value);
        }

        public static String GetEvaluatedProperty(IVsProject project, String name)
        {
            return GetEvaluatedProperty(project, name, String.Empty);
        }

        public static String GetEvaluatedProperty(IVsProject project, String name, String defaultValue)
        {
            String value = MSBuildUtils.GetEvaluatedProperty(MSBuildUtils.LoadedProject(GetProjectFullPath(project), DTEUtil.IsCppProject(project), true), name);
            return String.IsNullOrEmpty(value) ? defaultValue : value;
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
        public static EnvDTE.ProjectItem FindProjectItem(String path)
        {
            return Package.Instance.DTE2.Solution.FindProjectItem(path);
        }

        public static Dictionary<String, List<String>> GetGeneratedFiles(IVsProject project)
        {
            return GetGeneratedFiles(project, DTEUtil.IsIceBuilderEnabled(project));
        }

        public static Dictionary<String, List<String>> GetGeneratedFiles(IVsProject project, IceBuilderProjectType type)
        {
            return type == IceBuilderProjectType.CsharpProjectType ?
                GetCSharpGeneratedFiles(project) : GetCppGeneratedFiles(GetCppGeneratedFiles(project));
        }

        public static Dictionary<String, List<String>> GetCSharpGeneratedFiles(IVsProject project)
        {
            Dictionary<string, List<string>> generated = new Dictionary<string, List<string>>();
            String outputDir = GetOutputDir(project, false, true);
            List<string> items = GetIceBuilderItems(project);
            foreach (String item in items)
            {
                generated[item] = new List<string>(
                    new String[]
                    {
                        Path.GetFullPath(Path.Combine(outputDir, GetCSharpGeneratedItemPath(item)))
                    });
            }
            return generated;
        }

        public struct CppGeneratedFileSet
        {
            public EnvDTE.Configuration configuration;
            public String filename;
            public List<String> headers;
            public List<String> sources;
        }

        public static Dictionary<String, List<String>>
        GetCppGeneratedFiles(List<CppGeneratedFileSet> filesets)
        {
            Dictionary<string, List<string>> generated = new Dictionary<string, List<string>>();
            foreach(CppGeneratedFileSet fileset in filesets)
            {
                if(generated.ContainsKey(fileset.filename))
                {
                    generated[fileset.filename] = generated[fileset.filename].Union(fileset.headers).Union(fileset.sources).ToList();
                }
                else
                {
                    generated[fileset.filename] = fileset.headers.Union(fileset.sources).ToList();
                }
            }
            return generated;
        }

        public static IVsCfg[]
        GetProjectConfigurations(IVsProject project)
        {
            IVsCfgProvider provider = project as IVsCfgProvider;
            uint[] sz = new uint[1];
            provider.GetCfgs(0, null, sz, null);
            if (sz[0] > 0)
            {
                IVsCfg[] cfgs = new IVsCfg[sz[0]];
                provider.GetCfgs(sz[0], cfgs, sz, null);
                return cfgs;
            }
            return new IVsCfg[0];
        }

        public static List<CppGeneratedFileSet>
        GetCppGeneratedFiles(IVsProject project)
        {
            List<string> outputDirectories = new List<string>();
            List<string> headerOutputDirectories = new List<string>();

            //
            // Check if the output directories expand to different values in each configuration, if that is the case we
            // add generated files per configuration, and use ExcludeFromBuild to disable the file in all the configurations
            // but the one matching the configuration expanded value of the properties.
            //
            //
            IVsBuildPropertyStorage propertyStorage = project as IVsBuildPropertyStorage;
            IVsCfg[] configurations = GetProjectConfigurations(project);
            foreach(IVsCfg config in configurations)
            {
                String value;
                String configName;
                config.get_DisplayName(out configName);
                propertyStorage.GetPropertyValue("IceBuilderOutputDir", configName, (uint)_PersistStorageType.PST_PROJECT_FILE, out value);
                if(!String.IsNullOrEmpty(value) && !outputDirectories.Contains(value))
                {
                    outputDirectories.Add(value);
                }
                propertyStorage.GetPropertyValue("IceBuilderHeaderOutputDir", configName, (uint)_PersistStorageType.PST_PROJECT_FILE, out value);
                if(!String.IsNullOrEmpty(value) && !outputDirectories.Contains(value))
                {
                    headerOutputDirectories.Add(value);
                }
            }
            bool generateFilesPerConfiguration = headerOutputDirectories.Count > 1 || outputDirectories.Count > 1;

            List<CppGeneratedFileSet> generated = new List<CppGeneratedFileSet>();
            List<string> items = GetIceBuilderItems(project);
            if (items.Count > 0)
            {
                String projectDir = GetProjectBaseDirectory(project);

                String outputDir = GetOutputDir(project, false, false);
                String headerOutputDir = GetOutputDir(project, true, false);

                String sourceExt = GetEvaluatedProperty(project, PropertyNames.SourceExt, ".cpp");
                String headerExt = GetEvaluatedProperty(project, PropertyNames.HeaderExt, ".h");

                EnvDTE.Project p = DTEUtil.GetProject(project as IVsHierarchy);
                if (generateFilesPerConfiguration)
                {
                    foreach (EnvDTE.Configuration configuration in p.ConfigurationManager)
                    {
                        String outputDirEvaluated = Path.Combine(projectDir, Package.Instance.VCUtil.Evaluate(configuration, outputDir));
                        String headerOutputDirEvaluated = Path.Combine(projectDir, Package.Instance.VCUtil.Evaluate(configuration, headerOutputDir));

                        CppGeneratedFileSet fileset = new CppGeneratedFileSet();
                        fileset.configuration = configuration;
                        fileset.headers = new List<string>();
                        fileset.sources = new List<string>();

                        foreach (String item in items)
                        {
                            fileset.filename = item;
                            fileset.sources.Add(Path.GetFullPath(Path.Combine(outputDirEvaluated, GetGeneratedItemPath(item, sourceExt))));
                            fileset.headers.Add(Path.GetFullPath(Path.Combine(headerOutputDirEvaluated, GetGeneratedItemPath(item, headerExt))));
                        }
                        generated.Add(fileset);
                    }
                }
                else
                {
                    EnvDTE.Configuration configuration = p.ConfigurationManager.ActiveConfiguration;
                    String outputDirEvaluated = Path.Combine(projectDir, Package.Instance.VCUtil.Evaluate(configuration, outputDir));
                    String headerOutputDirEvaluated = Path.Combine(projectDir, Package.Instance.VCUtil.Evaluate(configuration, headerOutputDir));

                    CppGeneratedFileSet fileset = new CppGeneratedFileSet();
                    fileset.configuration = configuration;
                    fileset.headers = new List<string>();
                    fileset.sources = new List<string>();

                    foreach (String item in items)
                    {
                        fileset.filename = item;
                        fileset.sources.Add(Path.GetFullPath(Path.Combine(outputDirEvaluated, GetGeneratedItemPath(item, sourceExt))));
                        fileset.headers.Add(Path.GetFullPath(Path.Combine(headerOutputDirEvaluated, GetGeneratedItemPath(item, headerExt))));
                    }
                    generated.Add(fileset);
                }
            }
            return generated;
        }

        public static bool
        CheckGenerateFileIsValid(IVsProject project, IceBuilderProjectType projectType, String path)
        {
            if(projectType == IceBuilderProjectType.CsharpProjectType)
            {
                String outputDir = ProjectUtil.GetOutputDir(project, false, true);
                String generatedSource = ProjectUtil.GetCSharpGeneratedItemPath(Path.GetFileName(path));
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
            else if(projectType == IceBuilderProjectType.CppProjectType)
            {
                String source = ProjectUtil.GetCppGeneratedSourceItemPath(project, path);
                String header = ProjectUtil.GetCppGeneratedHeaderItemPath(project, path);

                EnvDTE.Project p = DTEUtil.GetProject(project as IVsHierarchy);

                foreach (EnvDTE.Configuration config in p.ConfigurationManager)
                {
                    String outputDir = Package.Instance.VCUtil.Evaluate(config, "$(IceBuilderOutputDir)");
                    outputDir = Path.GetFullPath(Path.Combine(GetProjectBaseDirectory(project), outputDir));
                    String headerOutputDir = Package.Instance.VCUtil.Evaluate(config, "$(IceBuilderHeaderOutputDir)");
                    if (String.IsNullOrEmpty(headerOutputDir))
                    {
                        headerOutputDir = outputDir;
                    }
                    else
                    {
                        headerOutputDir = Path.GetFullPath(Path.Combine(GetProjectBaseDirectory(project), headerOutputDir));
                    }
                    String generatedSource = Path.GetFullPath(Path.Combine(outputDir, source));
                    String generatedHeader = Path.GetFullPath(Path.Combine(headerOutputDir, header));

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
            }
            return true;
        }

        public static void DeleteItems(List<String> paths)
        {
            foreach (String path in paths)
            {
                EnvDTE.ProjectItem item = ProjectUtil.FindProjectItem(path);
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

        public static void SetupGenerated(IVsProject project, EnvDTE.Configuration configuration, String filter, List<string> files, bool generatedFilesPerConfiguration)
        {
            List<string> missing = new List<string>();
            foreach (String file in files)
            {
                if (!Directory.Exists(Path.GetDirectoryName(file)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                }

                if (!File.Exists(file))
                {
                    File.Create(file).Dispose();
                }

                int found;
                uint id;
                VSDOCUMENTPRIORITY[] priority = new VSDOCUMENTPRIORITY[1];
                project.IsDocumentInProject(file, out found, priority, out id);
                if (found == 0)
                {
                    missing.Add(file);
                }
            }

            Package.Instance.VCUtil.AddGeneratedFiles(DTEUtil.GetProject(project as IVsHierarchy), configuration, filter, missing, generatedFilesPerConfiguration);
        }
            static List<String> KnownHeaderExtension = new List<String>(new String[] { ".h", ".hpp", ".hh", ".hxx" });
        public static void SetupGenerated(IVsProject project, IceBuilderProjectType type)
        {
            if(type == IceBuilderProjectType.CppProjectType)
            {
                //
                // This will ensure that property reads don't use a cached project.
                //
                MSBuildUtils.LoadedProject(ProjectUtil.GetProjectFullPath(project), true, false);

                List<CppGeneratedFileSet> generated = GetCppGeneratedFiles(project);
                foreach(CppGeneratedFileSet fileset in generated)
                {
                    SetupGenerated(project, fileset.configuration, "Source Files", fileset.sources, generated.Count > 1);
                    SetupGenerated(project, fileset.configuration, "Header Files", fileset.headers, generated.Count > 1);
                }
                Package.Instance.FileTracker.Reap(GetProjectFullPath(project), GetCppGeneratedFiles(generated));
            }
            else // C# project
            {
                EnvDTE.Project p = DTEUtil.GetProject(project as IVsHierarchy);
                Dictionary<String, List<String>> generated = GetCSharpGeneratedFiles(project);
                foreach (KeyValuePair<String, List<String>> i in generated)
                {
                    foreach (String file in i.Value)
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(file)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(file));
                        }

                        if (!File.Exists(file))
                        {
                            File.Create(file).Dispose();
                        }

                        EnvDTE.ProjectItem item = FindProjectItem(file);
                        if (item == null)
                        {
                            p.ProjectItems.AddFromFile(file);
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
                Package.Instance.FileTracker.Reap(GetProjectFullPath(project), generated);
            }
        }

        public static bool AddAssemblyReference(EnvDTE.Project project, String component)
        {
            VSLangProj.VSProject vsProject = (VSLangProj.VSProject)project.Object;
            try
            {
                VSLangProj80.Reference3 reference = (VSLangProj80.Reference3)vsProject.References.Add(component + ".dll");
                reference.CopyLocal = true;
                //
                // We set SpecificVersion to false so that references still work
                // when Ice Home setting is updated.
                //
                reference.SpecificVersion = false;
                return true;
            }
            catch (COMException)
            {
            }
            return false;
        }

        public static bool RemoveAssemblyReference(EnvDTE.Project project, String component)
        {
            foreach (VSLangProj.Reference r in ((VSLangProj.VSProject)project.Object).References)
            {
                if (r.Name.Equals(component, StringComparison.OrdinalIgnoreCase))
                {
                    r.Remove();
                    return true;
                }
            }
            return false;
        }

        public static bool HasAssemblyReference(EnvDTE.Project project, String component)
        {
            foreach (VSLangProj.Reference r in ((VSLangProj.VSProject)project.Object).References)
            {
                if (r.Name.Equals(component, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static VSLangProj.Reference FindAssemblyReference(EnvDTE.Project project, String component)
        {
            foreach (VSLangProj.Reference r in ((VSLangProj.VSProject)project.Object).References)
            {
                if (r.Name.Equals(component, StringComparison.OrdinalIgnoreCase))
                {
                    return r;
                }
            }
            return null;
        }
    }
}
