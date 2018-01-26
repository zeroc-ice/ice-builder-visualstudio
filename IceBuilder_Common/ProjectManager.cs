// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Build.Evaluation;

#if VS2017
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
#endif

namespace IceBuilder
{
#if VS2017
    public class CPSProjectManagerI : ProjectManager
    {
        IProjectThreadingService ThreadingService
        {
            get;
            set;
        }

        IProjectLockService ProjectLockService
        {
            get;
            set;
        }

        private UnconfiguredProject UnconfiguredProject
        {
            get;
            set;
        }
        //
        // Disponse to unsuscribe
        //
        private IDisposable ProjectSubscription
        {
            get;
            set;
        }

        public CPSProjectManagerI(UnconfiguredProject unconfiguredProject)
        {
            UnconfiguredProject = unconfiguredProject;
            ProjectLockService = unconfiguredProject.ProjectService.Services.ProjectLockService;
            ThreadingService = unconfiguredProject.ProjectService.Services.ThreadingPolicy;
            var activeConfiguredProjectSubscription = unconfiguredProject.Services.ActiveConfiguredProjectSubscription;
            var projectSource = activeConfiguredProjectSubscription.ProjectSource;

            ProjectSubscription = projectSource.SourceBlock.LinkTo(
                new ActionBlock<IProjectVersionedValue<IProjectSnapshot>>(ProjectUpdateAsync));

            ThreadingService.ExecuteSynchronously(async () =>
                {
                    using (var projectWriteLock = await ProjectLockService.ReadLockAsync())
                    {
                        var configuredProject = await UnconfiguredProject.GetSuggestedConfiguredProjectAsync();
                        MSBuildProject = await projectWriteLock.GetProjectAsync(configuredProject);
                    }
                });
        }

        private async Task ProjectUpdateAsync(IProjectVersionedValue<IProjectSnapshot> update)
        {
            // Switch to the Main thread (if necessary):
            await ThreadingService.SwitchToUIThread();
            OnProjectChanged(new ProjectChangedEventArgs(update.Value.ProjectInstance));
        }

        public override async void UpdateProjectAsync(ProjectUpdateAction update)
        {
            using (var access = await ProjectLockService.WriteLockAsync())
            {
                var configuredProject = await UnconfiguredProject.GetSuggestedConfiguredProjectAsync();
                var project = await access.GetProjectAsync(configuredProject);

                //
                // Check out the project from SCC before aply any changes
                //
                await access.CheckoutAsync(configuredProject.UnconfiguredProject.FullPath);

                if(update(project))
                {
                    project.Save();
                }
            }
        }

        public bool HasCapability(string capability)
        {
            return UnconfiguredProject.Capabilities.Contains(capability);
        }
    }
#endif

    public class MSBuildProjectManagerI : ProjectManager
    {
        public EnvDTE.Project Project
        {
            get;
            private set;
        }

        public MSBuildProjectManagerI(EnvDTE.Project project)
        {
            Project = project;
            MSBuildProject = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(project.FullName).FirstOrDefault();
        }

        public override void UpdateProjectAsync(ProjectUpdateAction update)
        {
            var sc = Project.DTE.SourceControl;
            var fullPath = MSBuildProject.FullPath;
            if (sc != null)
            {
                if(sc.IsItemUnderSCC(fullPath) && !sc.IsItemCheckedOut(fullPath))
                {
                    sc.CheckOutItem(fullPath);
                }
            }
            if (update(MSBuildProject))
            {
                Project.Save();
            }
        }

        public bool HasCapability(string capability)
        {
            return false;
        }
    }

    public class ProjectManagerFactoryI : IVsProjectManagerFactory
    {
        public IVsProjectManager GetProjectManager(IVsProject project)
        {
#if VS2017
            IVsBrowseObjectContext context = project as IVsBrowseObjectContext;
            if(context == null)
            {
                var dteProject = project.GetDTEProject();
                if(dteProject != null)
                {
                    context = dteProject.Object as IVsBrowseObjectContext;
                }
            }

            if(context != null)
            {
                return new CPSProjectManagerI(context.UnconfiguredProject);
            }
            else
            {
                return new MSBuildProjectManagerI(project.GetDTEProject());
            }
#else
            return new MSBuildProjectManagerI(project.GetDTEProject());
#endif
        }
    }
}
