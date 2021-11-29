// Copyright (c) ZeroC, Inc. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using MSProject = Microsoft.Build.Evaluation.Project;
using Microsoft.VisualStudio.Shell;
using System.Linq;
using System.Collections.Generic;

#if VS2017 || VS2019 || VS2022
using System.IO;
using System.Threading.Tasks.Dataflow;
#else
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.ProjectSystem.Designers;
#endif

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace IceBuilder
{
    class ProjectHelper : IVsProjectHelper
    {
        public T WithProject<T>(IVsProject project, Func<MSProject, T> func, bool switchToMainThread = false)
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
        public void UpdateProject(IVsProject project, Action<MSProject> action, bool switchToMainThread = false)
        {
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

        protected static async Task<T> WithProjectAsync<T>(
            UnconfiguredProject unconfiguredProject,
            Func<MSProject, T> func,
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

        protected static async System.Threading.Tasks.Task UpdateProjectAsync(
            UnconfiguredProject unconfiguredProject,
            Action<MSProject> action,
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

        public string GetItemMetadata(IVsProject project, string identity, string name, string defaultValue = "") =>
            WithProject(project, (MSProject msproject) => msproject.GetItemMetadata(identity, name, defaultValue));

        public string GetDefaultItemMetadata(IVsProject project, string name, bool evaluated, string defaultValue = "") =>
            WithProject(project, (MSProject msproject) => msproject.GetDefaultItemMetadata(name, evaluated, defaultValue));

        public void SetItemMetadata(IVsProject project, string itemType, string label, string name, string value) =>
            UpdateProject(project, (MSProject msproject) => msproject.SetItemMetadata(itemType, label, name, value));

        public void SetItemMetadata(IVsProject project, string name, string value) =>
            UpdateProject(
                project,
                (MSProject msproject) => msproject.SetItemMetadata("SliceCompile", "IceBuilder", name, value));

        public void AddFromFile(IVsProject project, string file)
        {
#if VS2017 || VS2019 || VS2022
            if (project.IsCppProject() || GetUnconfiguredProject(project) == null)
            {
                project.GetDTEProject().ProjectItems.AddFromFile(file);
            }
            else
            {
                project.UpdateProject((MSProject msproject) =>
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
#else
            project.GetDTEProject().ProjectItems.AddFromFile(file);
#endif
        }

#if VS2017 || VS2019 || VS2022
        private bool HasGeneratedItemDuplicates(IVsProject project)
        {
            if (!project.IsCppProject() && GetUnconfiguredProject(project) != null)
            {
                return project.WithProject((MSProject msproject) =>
                {
                    var all = msproject.Xml.Items.Where(item => item.ItemType.Equals("Compile"));

                    foreach (var item in all)
                    {
                        // If there is a glob item that already match the evaluated include path we can remove the non
                        // glob item as it is a duplicate.
                        var globItem = msproject.AllEvaluatedItems.FirstOrDefault(i =>
                            i.HasMetadata("SliceCompileSource") &&
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
#endif

        public void RemoveGeneratedItemDuplicates(IVsProject project)
        {
#if VS2017 || VS2019 || VS2022
            // With .NET project system when default compile items are enabled we can end up with duplicate generated
            // items, as the call to AddItem doesn't detect that the new create file is already part of a glob and adds
            // a second item with the given include.
            if (HasGeneratedItemDuplicates(project))
            {
                project.UpdateProject((MSProject msproject) =>
                    {
                        var all = msproject.Xml.Items.Where(item => item.ItemType.Equals("Compile"));

                        foreach (var item in all)
                        {
                            // If there is a glob item that already match the evaluated include path we can remove the
                            // non glob item as it is a duplicate.
                            var globItem = msproject.AllEvaluatedItems.FirstOrDefault(i =>
                                i.HasMetadata("SliceCompileSource") &&
                                !i.EvaluatedInclude.Equals(i.UnevaluatedInclude, StringComparison.OrdinalIgnoreCase) &&
                                i.EvaluatedInclude.Equals(item.Include, StringComparison.OrdinalIgnoreCase));
                            if (globItem != null)
                            {
                                item.Parent.RemoveChild(item);
                            }
                        }
                    });
            }
#endif
        }

        public void RemoveGeneratedItemCustomMetadata(IVsProject project, List<string> paths)
        {
#if VS2017 || VS2019 || VS2022
            var projectDir = project.GetProjectBaseDirectory();
            project.UpdateProject((MSProject msproject) =>
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
#endif
        }

        public void SetGeneratedItemCustomMetadata(IVsProject project, string slice, string generated,
            List<string> excludedConfigurations = null)
        {
            project.UpdateProject((MSProject msproject) =>
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
#if VS2017 || VS2019 || VS2022
                        // With Visual Studio 2017 and abvove if the item originate from a glob we update the item
                        // medata using the Update attribute.
                        else
                        {
                            var updateItem = msproject.Xml.Items.FirstOrDefault(i => generated.Equals(i.Update));
                            if (updateItem == null)
                            {
                                updateItem = msproject.Xml.CreateItemElement(item.ItemType);
                                var group = msproject.Xml.ItemGroups.FirstOrDefault();
                                if (group == null)
                                {
                                    group = msproject.Xml.AddItemGroup();
                                }
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
#endif
                    }
                });
        }

        public IDisposable OnProjectUpdate(IVsProject project, Action onProjectUpdate)
        {
#if VS2017 || VS2019 || VS2022
            var unconfiguredProject = GetUnconfiguredProject(project);
            if (unconfiguredProject != null)
            {
                var activeConfiguredProjectSubscription = unconfiguredProject.Services.ActiveConfiguredProjectSubscription;
                var projectSource = activeConfiguredProjectSubscription.ProjectSource;

                return projectSource.SourceBlock.LinkTo(
                    new ActionBlock<IProjectVersionedValue<IProjectSnapshot>>(update => onProjectUpdate()));
            }
#endif
            return null;
        }
    }
}
