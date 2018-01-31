// **********************************************************************
//
// Copyright (c) 20092015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using Microsoft.Build.Execution;
using MSBuildProject = Microsoft.Build.Evaluation.Project;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;

namespace IceBuilder
{
    public class Builder
    {
        public Builder(IVsBuildManagerAccessor2 accessor)
        {
            BuildManagerAccessor = accessor;
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public bool Build(IVsProject p, BuildCallback buildCallback, BuildLogger buildLogger)
        {
            MSBuildProject project = p.GetMSBuildProject();

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

            if(err != VSConstants.E_PENDING && err != VSConstants.S_OK)
            {
                ErrorHandler.ThrowOnFailure(err);
            }

            if(err == VSConstants.E_PENDING)
            {
                project.ProjectCollection.Loggers.Remove(buildLogger);
                project.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;

                Dispatcher = Dispatcher.CurrentDispatcher;
                BuildAvailableEvent = new ManualResetEvent(false);
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
                try
                {
                    Dictionary<string, string> properties = new Dictionary<string, string>();
                    string platform = buildCallback.ProjectConfiguration.PlatformName;
                    properties["Platform"] = platform.Equals("Any CPU") ? "AnyCPU" : platform;
                    properties["Configuration"] = buildCallback.ProjectConfiguration.ConfigurationName;

                    BuildRequestData buildRequest = new BuildRequestData(
                            p.GetProjectFullPath(),
                            properties,
                            null,
                            new string[] { "SliceCompile" },
                            project.ProjectCollection.HostServices,
                            BuildRequestDataFlags.ProvideProjectStateAfterBuild |
                            BuildRequestDataFlags.IgnoreExistingProjectState |
                            BuildRequestDataFlags.ReplaceExistingProjectInstance);

                    BuildSubmission submission = BuildManager.DefaultBuildManager.PendBuildRequest(buildRequest);
                    ErrorHandler.ThrowOnFailure(BuildManagerAccessor.RegisterLogger(submission.SubmissionId, buildLogger));
                    buildCallback.BeginBuild();
                    submission.ExecuteAsync(s =>
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                        {
                            project.ProjectCollection.Loggers.Remove(buildLogger);
                            project.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;
                            BuildManagerAccessor.ReleaseBuildResources(cookie);
                            s.BuildManager.ResetCaches();
                            BuildManagerAccessor.UnregisterLoggers(s.SubmissionId);
                            buildCallback.EndBuild(s.BuildResult.OverallResult == BuildResultCode.Success);
                        }));
                    }, null);

                    return true;
                }
                catch(Exception)
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
        public BuildCallback(IVsProject project, EnvDTE.OutputWindowPane outputPane,
                      EnvDTE.Configuration projectConfiguration)
        {

            Project = project;
            OutputPane = outputPane;
            ProjectConfiguration = projectConfiguration;

        }

        public void BeginBuild()
        {
            OutputPane.OutputString(
                string.Format("------ Ice Builder Build started: Project: {0}, Configuration: {1} {2} ------\n",
                    ProjectUtil.GetProjectName(Project),
                    ProjectConfiguration.ConfigurationName,
                    ProjectConfiguration.PlatformName));
        }

        public void EndBuild(bool succeed)
        {
            OutputPane.OutputString(
                string.Format("------ Build {0} ------\n\n", (succeed ? "succeeded" : "failed")));
            Package.Instance.BuildDone();
        }

        IVsProject Project
        {
            get;
            set;
        }

        EnvDTE.OutputWindowPane OutputPane
        {
            get;
            set;
        }

        public EnvDTE.Configuration ProjectConfiguration
        {
            get;
            set;
        }
    }
}
