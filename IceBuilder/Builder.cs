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

        public bool Build(IVsProject project, BuildCallback buildCallback, BuildLogger buildLogger, string platform, string configuration)
        {
            return project.WithProject((MSBuildProject msproject) =>
                {
                    //
                    // We need to set this before we acquire the build resources otherwise Msbuild
                    // will not see the changes.
                    //
                    bool onlyLogCriticalEvents = msproject.ProjectCollection.OnlyLogCriticalEvents;
                    msproject.ProjectCollection.Loggers.Add(buildLogger);
                    msproject.ProjectCollection.OnlyLogCriticalEvents = false;

                    uint cookie;
                    int err = BuildManagerAccessor.AcquireBuildResources(VSBUILDMANAGERRESOURCE.VSBUILDMANAGERRESOURCE_DESIGNTIME |
                                                                         VSBUILDMANAGERRESOURCE.VSBUILDMANAGERRESOURCE_UITHREAD, out cookie);

                    if (err != VSConstants.E_PENDING && err != VSConstants.S_OK)
                    {
                        ErrorHandler.ThrowOnFailure(err);
                    }

                    if (err == VSConstants.E_PENDING)
                    {
                        msproject.ProjectCollection.Loggers.Remove(buildLogger);
                        msproject.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;

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
                            properties["Platform"] = platform;
                            properties["Configuration"] = configuration;

                            BuildRequestData buildRequest = new BuildRequestData(
                                    msproject.FullPath,
                                    properties,
                                    null,
                                    new string[] { "SliceCompile" },
                                    msproject.ProjectCollection.HostServices,
                                    BuildRequestDataFlags.ProvideProjectStateAfterBuild |
                                    BuildRequestDataFlags.IgnoreExistingProjectState |
                                    BuildRequestDataFlags.ReplaceExistingProjectInstance);

                            BuildSubmission submission = BuildManager.DefaultBuildManager.PendBuildRequest(buildRequest);
                            ErrorHandler.ThrowOnFailure(BuildManagerAccessor.RegisterLogger(submission.SubmissionId, buildLogger));
                            Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                                {
                                    buildCallback.BeginBuild(platform, configuration);
                                }));
                            submission.ExecuteAsync(s =>
                            {
                                Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                                {
                                    msproject.ProjectCollection.Loggers.Remove(buildLogger);
                                    msproject.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;
                                    BuildManagerAccessor.ReleaseBuildResources(cookie);
                                    s.BuildManager.ResetCaches();
                                    BuildManagerAccessor.UnregisterLoggers(s.SubmissionId);
                                    buildCallback.EndBuild(s.BuildResult.OverallResult == BuildResultCode.Success);
                                }));
                            }, null);

                            return true;
                        }
                        catch (Exception)
                        {
                            msproject.ProjectCollection.Loggers.Remove(buildLogger);
                            msproject.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;
                            BuildManagerAccessor.ReleaseBuildResources(cookie);
                            throw;
                        }
                    }
                });
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
        public BuildCallback(IVsProject project, EnvDTE.OutputWindowPane outputPane)
        {
            _project = project;
            _outputPane = outputPane;
        }

        public void BeginBuild(string platform, string configuration)
        {
            _outputPane.OutputString(
                string.Format("------ Ice Builder Build started: Project: {0}, Configuration: {1} {2} ------\n",
                    ProjectUtil.GetProjectName(_project), configuration, platform));
        }

        public void EndBuild(bool succeed)
        {
            _outputPane.OutputString(
                string.Format("------ Build {0} ------\n\n", (succeed ? "succeeded" : "failed")));
            Package.Instance.BuildDone();
        }

        private IVsProject _project;
        private EnvDTE.OutputWindowPane _outputPane;
    }
}
