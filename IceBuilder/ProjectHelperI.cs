// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder;

public static class ProjectHelper
{
    public static T WithProject<T>(IVsProject project, Func<MSBuildProject, T> func, bool switchToMainThread = false)
    {
        var data = default(T);
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var unconfiguredProject = GetUnconfiguredProject(project);
            if (unconfiguredProject != null)
            {
                data = await WithProjectAsync(unconfiguredProject, func, switchToMainThread);
            }
            else
            {
                var msproject = project.GetMSBuildProject();
                msproject.ReevaluateIfNecessary();
                if (switchToMainThread)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                }
                data = func(msproject);
            }
        });
        return data;
    }
    public static void UpdateProject(IVsProject project, Action<MSBuildProject> action, bool switchToMainThread = false)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        project.EnsureIsCheckout();
        ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var unconfiguredProject = GetUnconfiguredProject(project);
                if (unconfiguredProject != null)
                {
                    await UpdateProjectAsync(unconfiguredProject, action, switchToMainThread);
                }
                else
                {
                    var msproject = project.GetMSBuildProject();
                    msproject.ReevaluateIfNecessary();
                    if (switchToMainThread)
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    }
                    action(msproject);
                }
            });
        project.GetDTEProject().Save();
    }

    private static async Task<T> WithProjectAsync<T>(
        UnconfiguredProject unconfiguredProject,
        Func<MSBuildProject, T> func,
        bool switchToMainThread = false)
    {
        T result = default;
        var service = unconfiguredProject.ProjectService.Services.ProjectLockService;
        if (service != null)
        {
            using (var access = await service.ReadLockAsync())
            {
                var configuredProject = await unconfiguredProject.GetSuggestedConfiguredProjectAsync();
                var buildProject = await access.GetProjectAsync(configuredProject);
                if (buildProject != null)
                {
                    if (switchToMainThread)
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    }
                    result = func(buildProject);
                }
                await access.ReleaseAsync();
            }
            await unconfiguredProject.ProjectService.Services.ThreadingPolicy.SwitchToUIThread();
        }
        return result;
    }

    private static async System.Threading.Tasks.Task UpdateProjectAsync(
        UnconfiguredProject unconfiguredProject,
        Action<MSBuildProject> action,
        bool switchToMainThread = false)
    {
        var service = unconfiguredProject.ProjectService.Services.ProjectLockService;
        if (service != null)
        {
            using (var access = await service.WriteLockAsync())
            {
                await access.CheckoutAsync(unconfiguredProject.FullPath);
                var configuredProject = await unconfiguredProject.GetSuggestedConfiguredProjectAsync();
                var buildProject = await access.GetProjectAsync(configuredProject);
                if (buildProject != null)
                {
                    if (switchToMainThread)
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    }
                    action(buildProject);
                }
                await access.ReleaseAsync();
            }
            await unconfiguredProject.ProjectService.Services.ThreadingPolicy.SwitchToUIThread();
        }
    }

    public static UnconfiguredProject GetUnconfiguredProject(IVsProject project)
    {
        UnconfiguredProject unconfiguredProject = null;
        ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsBrowseObjectContext context = project as IVsBrowseObjectContext;
                if (context == null)
                {
                    var dteproject = project.GetDTEProject();
                    if (dteproject != null)
                    {
                        context = dteproject.Object as IVsBrowseObjectContext;
                    }
                }
                unconfiguredProject = context?.UnconfiguredProject;
            });
        return unconfiguredProject;
    }

    public static string GetItemMetadata(IVsProject project, string identity, string name, string defaultValue = "") =>
        WithProject(project, msproject => msproject.GetItemMetadata(identity, name, defaultValue));

    public static string GetDefaultItemMetadata(IVsProject project, string name, bool evaluated, string defaultValue = "") =>
        WithProject(project, msproject => msproject.GetDefaultItemMetadata(name, evaluated, defaultValue));

    public static void SetItemMetadata(IVsProject project, string itemType, string label, string name, string value) =>
        UpdateProject(project, msproject => msproject.SetItemMetadata(itemType, label, name, value));

    public static void SetItemMetadata(IVsProject project, string name, string value) =>
        UpdateProject(
            project,
            msproject => msproject.SetItemMetadata("SliceCompile", "IceBuilder", name, value));

    public static void AddFromFile(IVsProject project, string file)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (project.IsCppProject() || GetUnconfiguredProject(project) == null)
        {
            project.GetDTEProject().ProjectItems.AddFromFile(file);
        }
        else
        {
            project.UpdateProject(msproject =>
            {
                if (Path.GetExtension(file).Equals(".cs", StringComparison.CurrentCultureIgnoreCase))
                {
                    var path = FileUtil.RelativePath(Path.GetDirectoryName(msproject.FullPath), file);
                    var items = msproject.GetItemsByEvaluatedInclude(path);
                    if (items.Count == 0)
                    {
                        msproject.AddItem("Compile", path);
                    }
                }
            });
        }
    }

    private static bool HasGeneratedItemDuplicates(IVsProject project)
    {
        if (!project.IsCppProject() && GetUnconfiguredProject(project) != null)
        {
            return project.WithProject(msproject =>
            {
                var all = msproject.Xml.Items.Where(item => item.ItemType.Equals("Compile"));

                foreach (var item in all)
                {
                    // If there is a glob item that already match the evaluated include path we can remove the non
                    // glob item as it is a duplicate.
                    var globItem = msproject.AllEvaluatedItems.FirstOrDefault(i =>
                        i.GetMetadata("SliceCompileSource") != null &&
                        !i.EvaluatedInclude.Equals(i.UnevaluatedInclude, StringComparison.OrdinalIgnoreCase) &&
                        i.EvaluatedInclude.Equals(item.Include, StringComparison.OrdinalIgnoreCase));
                    if (globItem != null)
                    {
                        return true;
                    }
                }
                return false;
            });
        }
        return false;
    }

    public static void RemoveGeneratedItemDuplicates(IVsProject project)
    {
        // With .NET project system when default compile items are enabled we can end up with duplicate generated
        // items, as the call to AddItem doesn't detect that the new create file is already part of a glob and adds
        // a second item with the given include.
        if (HasGeneratedItemDuplicates(project))
        {
            project.UpdateProject(msproject =>
            {
                var all = msproject.Xml.Items.Where(item => item.ItemType.Equals("Compile"));

                foreach (var item in all)
                {
                    // If there is a glob item that already match the evaluated include path we can remove the
                    // non glob item as it is a duplicate.
                    var globItem = msproject.AllEvaluatedItems.FirstOrDefault(i =>
                        i.GetMetadata("SliceCompileSource") != null &&
                        !i.EvaluatedInclude.Equals(i.UnevaluatedInclude, StringComparison.OrdinalIgnoreCase) &&
                        i.EvaluatedInclude.Equals(item.Include, StringComparison.OrdinalIgnoreCase));
                    if (globItem != null)
                    {
                        item.Parent.RemoveChild(item);
                    }
                }
            });
        }
    }

    public static void RemoveGeneratedItemCustomMetadata(IVsProject project, List<string> paths)
    {
        var projectDir = project.GetProjectBaseDirectory();
        project.UpdateProject(msproject =>
        {
            var items = msproject.Xml.Items.Where(item =>
                {
                    if (item.ItemType.Equals("Compile", StringComparison.OrdinalIgnoreCase) ||
                        item.ItemType.Equals("ClCompile", StringComparison.OrdinalIgnoreCase) ||
                        item.ItemType.Equals("ClInclude", StringComparison.OrdinalIgnoreCase))
                    {
                        return paths.Contains(Path.Combine(projectDir, item.Update));
                    }
                    else
                    {
                        return false;
                    }
                });

            foreach (var item in items)
            {
                item.Parent.RemoveChild(item);
            }
        });
    }

    public static void SetGeneratedItemCustomMetadata(IVsProject project, string slice, string generated,
        List<string> excludedConfigurations = null)
    {
        project.UpdateProject(msproject =>
        {
            var item = msproject.AllEvaluatedItems.FirstOrDefault(i => generated.Equals(i.EvaluatedInclude));
            if (item != null)
            {
                var element = item.Xml;
                if (excludedConfigurations != null)
                {
                    foreach (var conf in excludedConfigurations)
                    {
                        var metadata = element.AddMetadata("ExcludedFromBuild", "true");
                        metadata.Condition = string.Format("'$(Configuration)|$(Platform)'=='{0}'", conf);
                    }
                }
                // Only set SliceCompileSource if the item doesn't originate from a glob expression
                if (item.EvaluatedInclude.Equals(item.UnevaluatedInclude))
                {
                    item.SetMetadataValue("SliceCompileSource", slice);
                }

                // With Visual Studio 2017 and abvove if the item originate from a glob we update the item
                // medata using the Update attribute.
                else
                {
                    var updateItem = msproject.Xml.Items.FirstOrDefault(i => generated.Equals(i.Update));
                    if (updateItem == null)
                    {
                        updateItem = msproject.Xml.CreateItemElement(item.ItemType);
                        var group = msproject.Xml.ItemGroups.FirstOrDefault() ?? msproject.Xml.AddItemGroup();
                        updateItem.Update = generated;
                        group.AppendChild(updateItem);
                    }
                    var metadata = updateItem.Metadata.FirstOrDefault(m => m.Name.Equals("SliceCompileSource"));
                    if (metadata != null)
                    {
                        metadata.Value = slice;
                    }
                    else
                    {
                        updateItem.AddMetadata("SliceCompileSource", slice);
                    }
                }
            }
        });
    }

    public static IDisposable OnProjectUpdate(IVsProject project, Action onProjectUpdate)
    {
        var unconfiguredProject = GetUnconfiguredProject(project);
        if (unconfiguredProject != null)
        {
            var activeConfiguredProjectSubscription = unconfiguredProject.Services.ActiveConfiguredProjectSubscription;
            var projectSource = activeConfiguredProjectSubscription.ProjectSource;

            return projectSource.SourceBlock.LinkTo(
                new ActionBlock<IProjectVersionedValue<IProjectSnapshot>>(update => onProjectUpdate()));
        }
        return null;
    }
}
