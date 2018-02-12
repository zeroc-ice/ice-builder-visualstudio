// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using MSProject = Microsoft.Build.Evaluation.Project;
using Microsoft.VisualStudio.Shell;
using System.Linq;
using System.Collections.Generic;
using System.IO;

#if VS2017
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
#elif VS2013 || VS2015
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Designers;
#endif

namespace IceBuilder
{
    class ProjectHelper : IVsProjectHelper
    {
#if VS2012
        public void UpdateProject(IVsProject project, Action<MSProject> action)
        {
            var msproject = project.GetMSBuildProject();
            msproject.ReevaluateIfNecessary();
            action(msproject);
            project.GetDTEProject().Save();
        }

        public T WithProject<T>(IVsProject project, Func<MSProject, T> func)
        {
            var msproject = project.GetMSBuildProject();
            msproject.ReevaluateIfNecessary();
            return func(msproject);
        }
#else
        public T WithProject<T>(IVsProject project, Func<MSProject, T> func)
        {
            var data = default(T);
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var unconfiguredProject = GetUnconfiguredProject(project);
                if(unconfiguredProject != null)
                {
                    data = await WithProjectAsync(unconfiguredProject, func);
                }
                else
                {
                    var msproject = project.GetMSBuildProject();
                    msproject.ReevaluateIfNecessary();
                    data = func(msproject);
                }
            });
            return data;
        }
        public void UpdateProject(IVsProject project, Action<MSProject> action)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    var unconfiguredProject = GetUnconfiguredProject(project);
                    if(unconfiguredProject != null)
                    {
                        await UpdateProjectAsync(unconfiguredProject, action);
                    }
                    else
                    {
                        var msproject = project.GetMSBuildProject();
                        msproject.ReevaluateIfNecessary();
                        action(msproject);
                    }
                });
            project.GetDTEProject().Save();
        }
#endif

#if !VS2012
        protected static async Task<T>
        WithProjectAsync<T>(UnconfiguredProject unconfiguredProject, Func<MSProject, T> func)
        {
            T result = default(T);
            var service = unconfiguredProject.ProjectService.Services.ProjectLockService;
            if(service != null)
            {
                using(var access = await service.ReadLockAsync())
                {
                    var configuredProject = await unconfiguredProject.GetSuggestedConfiguredProjectAsync();
                    var buildProject = await access.GetProjectAsync(configuredProject);
                    if(buildProject != null)
                    {
                        result = func(buildProject);
                    }
                    await access.ReleaseAsync();
                }
                await unconfiguredProject.ProjectService.Services.ThreadingPolicy.SwitchToUIThread();
            }
            return result;
        }

        protected static async System.Threading.Tasks.Task UpdateProjectAsync(UnconfiguredProject unconfiguredProject, Action<MSProject> action)
        {
            var service = unconfiguredProject.ProjectService.Services.ProjectLockService;
            if(service != null)
            {
                using(var access = await service.WriteLockAsync())
                {
                    await access.CheckoutAsync(unconfiguredProject.FullPath);
                    var configuredProject = await unconfiguredProject.GetSuggestedConfiguredProjectAsync();
                    var buildProject = await access.GetProjectAsync(configuredProject);
                    if(buildProject != null)
                    {
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
                    if(context == null)
                    {
                        var dteproject = project.GetDTEProject();
                        if(dteproject != null)
                        {
                            context = dteproject.Object as IVsBrowseObjectContext;
                        }
                    }
                    unconfiguredProject = context != null ? context.UnconfiguredProject : null;
                });
            return unconfiguredProject;
        }
#endif

        public string GetItemMetadata(IVsProject project, string identity, string name, string defaultValue = "")
        {
            return WithProject(project, (MSProject msproject) =>
                {
                    return msproject.GetItemMetadata(identity, name, defaultValue);
                });
        }

        public string GetDefaultItemMetadata(IVsProject project, string name, bool evaluated, string defaultValue = "")
        {
            return WithProject(project, (MSProject msproject) =>
            {
                return msproject.GetDefaultItemMetadata(name, evaluated, defaultValue);
            });
        }

        public void SetItemMetadata(IVsProject project, string itemType, string label, string name, string value)
        {
            UpdateProject(project, (MSProject msproject) =>
                {
                    msproject.SetItemMetadata(itemType, label, name, value);
                });
        }

        public void SetItemMetadata(IVsProject project, string name, string value)
        {
            UpdateProject(project, (MSProject msproject) =>
                {
                    msproject.SetItemMetadata("SliceCompile", "IceBuilder", name, value);
                });
        }

        public void AddFromFile(IVsProject project, string file)
        {
#if VS2017
            if(project.IsCppProject() || GetUnconfiguredProject(project) == null)
            {
                project.GetDTEProject().ProjectItems.AddFromFile(file);
            }
            else
            {
                project.UpdateProject((MSProject msproject) =>
                    {
                        if(Path.GetExtension(file).Equals(".cs", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var path = FileUtil.RelativePath(Path.GetDirectoryName(msproject.FullPath), file);
                            var items = msproject.GetItemsByEvaluatedInclude(path);
                            if(items.Count == 0)
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

        public void RemoveGeneratedItemDuplicates(IVsProject project)
        {
#if VS2017
            //
            // With .NET Core project system when default compile items is enabled we
            // can end up with duplicate generated items, as the call to AddItem doesn't
            // detect that the new create file is already part of a glob and adds a
            // second item with the given include.
            //
            if (!project.IsCppProject() && GetUnconfiguredProject(project) != null)
            {
                project.UpdateProject((MSProject msproject) =>
                    {
                        var all = msproject.Xml.Items.Where(item => item.ItemType.Equals("Compile"));

                        foreach(var item in all)
                        {
                            //
                            // If there is a glob item that already match the evaluated include path we
                            // can remove the non glob item as it is a duplicate
                            //
                            var globItem = msproject.AllEvaluatedItems.FirstOrDefault(i =>
                                {
                                    return i.HasMetadata("SliceCompileSource") &&
                                           !i.EvaluatedInclude.Equals(i.UnevaluatedInclude) &&
                                           i.EvaluatedInclude.Equals(item.Include);
                                });
                            if(globItem != null)
                            {
                                item.Parent.RemoveChild(item);
                            }
                        }
                    });
            }
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
                        //
                        // Only set SliceCompileSource if the item doesn't originate
                        // from a glob expression
                        //
                        if (item.EvaluatedInclude.Equals(item.UnevaluatedInclude))
                        {
                            item.SetMetadataValue("SliceCompileSource", slice);
                        }
#if VS2017
                        //
                        // With Visual Studio 2017 if the imte originate from a glob we
                        // add update the item medata using the Update attribute
                        //
                        else
                        {
                            var updateItem = msproject.Xml.Items.FirstOrDefault(i => generated.Equals(i.Update));
                            if(updateItem == null)
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
                            if(metadata != null)
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
#if VS2017
            var unconfiguredProject = GetUnconfiguredProject(project);
            if(unconfiguredProject != null)
            {
                var activeConfiguredProjectSubscription = unconfiguredProject.Services.ActiveConfiguredProjectSubscription;
                var projectSource = activeConfiguredProjectSubscription.ProjectSource;

                return projectSource.SourceBlock.LinkTo(
                    new ActionBlock<IProjectVersionedValue<IProjectSnapshot>>( update =>
                    {
                        onProjectUpdate();
                    }));
            }
#endif
            return null;
        }
    }
}
