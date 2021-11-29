// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder
{
    public static class IVsProjectExtension
    {
        public static void EnsureIsCheckout(this IVsProject project) =>
            EnsureIsCheckout(project.GetDTEProject(), project.GetProjectFullPath());

        private static void EnsureIsCheckout(EnvDTE.Project project, string path)
        {
            var sc = project.DTE.SourceControl;
            if (sc != null)
            {
                if (sc.IsItemUnderSCC(path) && !sc.IsItemCheckedOut(path))
                {
                    sc.CheckOutItem(path);
                }
            }
        }

        public static EnvDTE.Project GetDTEProject(this IVsProject project)
        {
            object obj = null;
            if (project is IVsHierarchy hierarchy)
            {
                hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            }
            var dteproject = obj as EnvDTE.Project;
            return dteproject;
        }

        public static List<string> GetIceBuilderItems(this IVsProject project) =>
            project.WithProject((MSProject msproject) =>
                msproject.Items.Where(item => item.ItemType.Equals("SliceCompile"))
                               .Select(item => item.EvaluatedInclude)
                               .ToList());

        public static MSProject GetMSBuildProject(this IVsProject project) =>
            MSProjectExtension.LoadedProject(project.GetProjectFullPath());

        public static string GetProjectBaseDirectory(this IVsProject project)
        {
            ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out string fullPath));
            return Path.GetFullPath(Path.GetDirectoryName(fullPath));
        }

        public static string GetProjectFullPath(this IVsProject project)
        {
            try
            {
                ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out string fullPath));
                return Path.GetFullPath(fullPath);
            }
            catch (NotImplementedException)
            {
                return string.Empty;
            }
        }

        // Get the Guid that identifies the type of the project
        public static Guid GetProjecTypeGuid(this IVsProject project)
        {
            if (project is IVsHierarchy hierarchy)
            {
                try
                {
                    ErrorHandler.ThrowOnFailure(hierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out Guid type));
                    return type;
                }
                catch (Exception)
                {
                }
            }
            return new Guid();
        }

        public static bool IsCppProject(this IVsProject project)
        {
            Guid type = project.GetProjecTypeGuid();
            return type.Equals(cppProjectGUID) || type.Equals(cppStoreAppProjectGUID);
        }

        public static bool IsCSharpProject(this IVsProject project)
        {
            Guid type = project.GetProjecTypeGuid();
            return type.Equals(csharpProjectGUID);
        }

        public static bool IsMSBuildIceBuilderInstalled(this IVsProject project)
        {
            var type = project.GetProjecTypeGuid();
            if (type.Equals(cppProjectGUID) || type.Equals(cppStoreAppProjectGUID) || type.Equals(csharpProjectGUID))
            {
                // Find the full path of MSBuild Ice Builder props and target files and check they exists
                var props = project.WithProject((MSProject msproject) =>
                    {
                        return msproject.Imports.Where(
                            import => import.ImportedProject.FullPath.EndsWith("zeroc.icebuilder.msbuild.props")).Select(
                            import => import.ImportedProject.FullPath).FirstOrDefault();
                    });

                var targets = project.WithProject((MSProject msproject) =>
                    {
                        return msproject.Imports.Where(
                            import => import.ImportedProject.FullPath.EndsWith("zeroc.icebuilder.msbuild.targets")).Select(
                            import => import.ImportedProject.FullPath).FirstOrDefault();
                    });
                return !string.IsNullOrEmpty(props) && !string.IsNullOrEmpty(targets) && File.Exists(props) && File.Exists(targets);
            }
            return false;
        }

        public static bool IsIceBuilderGeneratedItem(this IVsProject project, string path)
        {
            var projectDir = project.GetProjectBaseDirectory();
            var includeValue = FileUtil.RelativePath(projectDir, path);
            return project.WithProject((MSProject msproject) =>
                {
                    return msproject.AllEvaluatedItems.FirstOrDefault(
                        item =>
                        {
                            return (item.ItemType.Equals("Compile") ||
                                    item.ItemType.Equals("ClCompile") ||
                                    item.ItemType.Equals("ClInclude")) &&
                                    item.EvaluatedInclude.Equals(includeValue) &&
                                    item.HasMetadata("SliceCompileSource");
                        }) != null;
                });
        }

        public static void UpdateProject(
            this IVsProject project,
            Action<MSProject> action,
            bool switchToMainThread = false) =>
            ProjectFactoryHelperInstance.ProjectHelper.UpdateProject(project, action, switchToMainThread);

        public static T WithProject<T>(
            this IVsProject project,
            Func<MSProject, T> func,
            bool switchToMainThread = false) =>
            ProjectFactoryHelperInstance.ProjectHelper.WithProject(project, func, switchToMainThread);

        public static IDisposable OnProjectUpdate(this IVsProject project, Action onProjectUpdate) =>
            ProjectFactoryHelperInstance.ProjectHelper.OnProjectUpdate(project, onProjectUpdate);

        public static string GetItemMetadata(
            this IVsProject project,
            string identity,
            string name,
            string defaultValue = "") =>
            ProjectFactoryHelperInstance.ProjectHelper.GetItemMetadata(project, identity, name, defaultValue);

        public static string GetDefaultItemMetadata(
            this IVsProject project,
            string name,
            bool evaluated,
            string defaultValue = "") =>
            ProjectFactoryHelperInstance.ProjectHelper.GetDefaultItemMetadata(project, name, evaluated, defaultValue);

        public static void SetItemMetadata(
            this IVsProject project,
            string itemType,
            string label,
            string name,
            string value)
        {
            project.EnsureIsCheckout();
            ProjectFactoryHelperInstance.ProjectHelper.SetItemMetadata(project, itemType, label, name, value);
        }

        public static void SetItemMetadata(this IVsProject project, string name, string value)
        {
            project.EnsureIsCheckout();
            ProjectFactoryHelperInstance.ProjectHelper.SetItemMetadata(project, name, value);
        }

        public static bool HasProjectFlavor(this IVsProject project, string flavor) =>
            project.WithProject((MSProject msproject) => msproject.HasProjectFlavor(flavor), true);

        public static void AddProjectFlavorIfNotExists(this IVsProject project, string flavor) =>
            ProjectFactoryHelperInstance.ProjectHelper.UpdateProject(project,
                (MSProject msproject) => msproject.AddProjectFlavorIfNotExists(flavor), true);

        public static void RemoveGeneratedItemCustomMetadata(this IVsProject project, List<string> paths)
        {
            project.EnsureIsCheckout();
            ProjectFactoryHelperInstance.ProjectHelper.RemoveGeneratedItemCustomMetadata(project, paths);
        }

        public static void SetGeneratedItemCustomMetadata(
            this IVsProject project,
            string slice,
            string generated,
            List<string> excludedConfigurations = null)
        {
            project.EnsureIsCheckout();
            ProjectFactoryHelperInstance.ProjectHelper.SetGeneratedItemCustomMetadata(
                project,
                slice,
                generated,
                excludedConfigurations);
        }

        public static string GetEvaluatedProperty(this IVsProject project, string name) =>
            project.GetEvaluatedProperty(name, string.Empty);

        public static string GetProperty(this IVsProject project, string name) =>
            project.WithProject((MSProject msproject) => msproject.GetProperty(name, true));

        public static string GetPropertyWithDefault(this IVsProject project, string name, string defaultValue) =>
            project.WithProject((MSProject msproject) => msproject.GetPropertyWithDefault(name, defaultValue));

        public static string GetEvaluatedProperty(this IVsProject project, string name, string defaultValue) =>
            project.WithProject((MSProject msproject) =>
            {
                var value = msproject.GetEvaluatedProperty(name);
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            });

        public static EnvDTE.ProjectItem GetProjectItem(this IVsProject project, uint item)
        {
            object value = null;
            if (project is IVsHierarchy hierarchy)
            {
                hierarchy.GetProperty(item, (int)__VSHPROPID.VSHPROPID_ExtObject, out value);
            }
            return value as EnvDTE.ProjectItem;
        }

        public static EnvDTE.ProjectItem GetProjectItem(this IVsProject project, string path)
        {
            var priority = new VSDOCUMENTPRIORITY[1];
            ErrorHandler.ThrowOnFailure(project.IsDocumentInProject(path, out int found, priority, out uint item));
            if (found == 0 || (priority[0] != VSDOCUMENTPRIORITY.DP_Standard && priority[0] != VSDOCUMENTPRIORITY.DP_Intrinsic))
            {
                return null;
            }
            return project.GetProjectItem(item);
        }

        public static void AddFromFile(this IVsProject project, string file)
        {
            project.EnsureIsCheckout();
            ProjectFactoryHelperInstance.ProjectHelper.AddFromFile(project, file);
        }

        public static void DeleteItems(this IVsProject project, List<string> paths)
        {
            if (paths.Count > 0)
            {
                project.EnsureIsCheckout();
                var projectDir = project.GetProjectBaseDirectory();

                project.RemoveGeneratedItemCustomMetadata(paths);

                var sliceCompileDependencies = paths.Distinct().Select(
                    p =>
                    {
                        return Path.Combine(Path.GetDirectoryName(p),
                            string.Format("SliceCompile.{0}.d", Path.GetFileNameWithoutExtension(p)));
                    });

                foreach (var path in paths.Concat(sliceCompileDependencies))
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (IOException)
                        {
                        }
                    }

                    var projectItem = project.GetProjectItem(path);
                    if (projectItem != null)
                    {
                        try
                        {
                            projectItem.Remove();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public static void RemoveGeneratedItemDuplicates(this IVsProject project) =>
            ProjectFactoryHelperInstance.ProjectHelper.RemoveGeneratedItemDuplicates(project);

        public static readonly Guid cppProjectGUID =
           new Guid("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}");
        public static readonly Guid cppStoreAppProjectGUID =
            new Guid("{BC8A1FFA-BEE3-4634-8014-F334798102B3}");
        public static readonly Guid csharpProjectGUID =
            new Guid("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
        public static readonly Guid unloadedProjectGUID =
            new Guid("{67294A52-A4F0-11D2-AA88-00C04F688DDE}");
    }
}
