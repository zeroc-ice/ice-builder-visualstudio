﻿// **********************************************************************
//
// Copyright (c) 20092015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using MSBuildProject = Microsoft.Build.Evaluation.Project;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace IceBuilder
{
    public class Builder
    {
        public Builder(IVsBuildManagerAccessor2 accessor)
        {
            BuildManagerAccessor = accessor;
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public bool Build(EnvDTE.Project p, BuildCallback buildCallback, BuildLogger buildLogger)
        {
            MSBuildProject project = MSBuildUtils.LoadedProject(p.FullName);
            //
            // We need to set this before we acquire the build resources otherwise Msbuild
            // will not see the changes.
            //
            bool onlyLogCriticalEvents = project.ProjectCollection.OnlyLogCriticalEvents;
            project.ProjectCollection.Loggers.Add(buildLogger);
            project.ProjectCollection.OnlyLogCriticalEvents = false;

            uint cookie;
            int err = BuildManagerAccessor.AcquireBuildResources(VSBUILDMANAGERRESOURCE.VSBUILDMANAGERRESOURCE_DESIGNTIME |
                                                                 VSBUILDMANAGERRESOURCE.VSBUILDMANAGERRESOURCE_UITHREAD, out cookie);

            if (err != VSConstants.E_PENDING && err != VSConstants.S_OK)
            {
                ErrorHandler.ThrowOnFailure(err);
            }

            if (err == VSConstants.E_PENDING)
            {
                project.ProjectCollection.Loggers.Remove(buildLogger);
                project.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;

                Dispatcher = Dispatcher.CurrentDispatcher;
                BuildAvailableEvent = new System.Threading.ManualResetEvent(false);
                BuildAvailableEvent.SafeWaitHandle = new SafeWaitHandle(BuildManagerAccessor.DesignTimeBuildAvailable, false);

                Thread t = new Thread(() =>
                    {
                        BuildAvailableEvent.WaitOne();
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Package.Instance.BuildNextProject();
                        }));
                    });
                t.Start();
                return false;
            }
            else
            {
                Package.Instance.FileTracker.Reap(p);
                ProjectUtil.SetupGenerated(p);
                
                try
                {
                    BuildRequestData buildRequest = new BuildRequestData(
                            project.CreateProjectInstance(),
                            new String[] { "IceBuilder_Compile" },
                            project.ProjectCollection.HostServices,
                            BuildRequestDataFlags.None);

                    BuildSubmission submission = BuildManager.DefaultBuildManager.PendBuildRequest(buildRequest);
                    ErrorHandler.ThrowOnFailure(BuildManagerAccessor.RegisterLogger(submission.SubmissionId, buildLogger));
                    buildCallback.BeginBuild();
                    submission.ExecuteAsync((s) =>
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                            {
                                project.ProjectCollection.Loggers.Remove(buildLogger);
                                project.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;
                                BuildManagerAccessor.ReleaseBuildResources(cookie);
                                BuildManagerAccessor.UnregisterLoggers(submission.SubmissionId);
                                buildCallback.EndBuild(submission.BuildResult.OverallResult == BuildResultCode.Success);
                            }));
                        }, null);
                    
                    return true;
                }
                catch (Exception)
                {
                    project.ProjectCollection.Loggers.Remove(buildLogger);
                    project.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;
                    BuildManagerAccessor.ReleaseBuildResources(cookie);
                    throw;
                }
            }
        }

        private ManualResetEvent BuildAvailableEvent
        {
            get;
            set;
        }

        private IVsBuildManagerAccessor2 BuildManagerAccessor
        {
            get;
            set;
        }

        private Dispatcher Dispatcher
        {
            get;
            set;
        }
    }

    public class BuildCallback
    {
        public BuildCallback(EnvDTE.Project project, EnvDTE.OutputWindowPane outputPane, 
                      EnvDTE80.SolutionConfiguration2 solutionConfiguration)
        {
            
            Project = project;
            OutputPane = outputPane;
            SolutionConfiguration = solutionConfiguration;
        }

        public void BeginBuild()
        {
            OutputPane.OutputString(
            String.Format("------ Ice Builder Build started: Project: {0}, Configuration: {1} {2} ------\n",
                ProjectUtil.GetProjectName(Project),
                SolutionConfiguration.Name,
                SolutionConfiguration.PlatformName));
        }

        public void EndBuild(bool succeed)
        {
            OutputPane.OutputString(
                String.Format("------ Build {0} ------\n\n", (succeed ? "succeeded" : "failed")));
            Package.Instance.BuildDone();
        }

        EnvDTE.Project Project
        {
            get;
            set;
        }

        EnvDTE.OutputWindowPane OutputPane
        {
            get;
            set;
        }

        EnvDTE80.SolutionConfiguration2 SolutionConfiguration
        {
            get;
            set;
        }
    }
}
