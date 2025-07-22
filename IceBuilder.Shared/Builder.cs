// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.Build.Execution;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Threading;
using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder
{
    public class Builder
    {
        private ManualResetEvent BuildAvailableEvent { get; set; }
        private IVsBuildManagerAccessor2 BuildManagerAccessor { get; set; }

        public Builder(IVsBuildManagerAccessor2 accessor) => BuildManagerAccessor = accessor;

        public bool Build(
            IVsProject project,
            BuildCallback buildCallback,
            BuildLogger buildLogger,
            string platform,
            string configuration)
        {
            return project.WithProject((MSBuildProject msproject) =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    // We need to set this before we acquire the build resources otherwise MSBuild will not see the changes.
                    bool onlyLogCriticalEvents = msproject.ProjectCollection.OnlyLogCriticalEvents;
                    msproject.ProjectCollection.Loggers.Add(buildLogger);
                    msproject.ProjectCollection.OnlyLogCriticalEvents = false;

                    int err = BuildManagerAccessor.AcquireBuildResources(
                        VSBUILDMANAGERRESOURCE.VSBUILDMANAGERRESOURCE_DESIGNTIME |
                        VSBUILDMANAGERRESOURCE.VSBUILDMANAGERRESOURCE_UITHREAD,
                        out uint cookie);

                    if (err != VSConstants.E_PENDING && err != VSConstants.S_OK)
                    {
                        ErrorHandler.ThrowOnFailure(err);
                    }

                    if (err == VSConstants.E_PENDING)
                    {
                        msproject.ProjectCollection.Loggers.Remove(buildLogger);
                        msproject.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;

                        BuildAvailableEvent = new ManualResetEvent(false)
                        {
                            SafeWaitHandle = new SafeWaitHandle(BuildManagerAccessor.DesignTimeBuildAvailable, false)
                        };

                        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                        {
                            BuildAvailableEvent.WaitOne();
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            Package.Instance.BuildNextProject();
                        });
                        return false;
                    }
                    else
                    {
                        try
                        {
                            Dictionary<string, string> properties = new Dictionary<string, string>
                            {
                                ["Platform"] = platform,
                                ["Configuration"] = configuration
                            };

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
                            buildCallback.BeginBuild(platform, configuration);

                            submission.ExecuteAsync(s =>
                            {
                                ThreadHelper.JoinableTaskFactory.Run(async () =>
                                    {
                                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                                        msproject.ProjectCollection.Loggers.Remove(buildLogger);
                                        msproject.ProjectCollection.OnlyLogCriticalEvents = onlyLogCriticalEvents;
                                        BuildManagerAccessor.ReleaseBuildResources(cookie);
                                        s.BuildManager.ResetCaches();
                                        BuildManagerAccessor.UnregisterLoggers(s.SubmissionId);
                                        buildCallback.EndBuild(s.BuildResult.OverallResult == BuildResultCode.Success);
                                    });
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
                }, true);
        }
    }

    public class BuildCallback
    {
        private readonly IVsProject _project;
        private readonly EnvDTE.OutputWindowPane _outputPane;

        public BuildCallback(IVsProject project, EnvDTE.OutputWindowPane outputPane)
        {
            _project = project;
            _outputPane = outputPane;
        }

        public void BeginBuild(string platform, string configuration)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _outputPane.OutputString(
                string.Format("------ Ice Builder Build started: Project: {0}, Configuration: {1} {2} ------\n",
                    ProjectUtil.GetProjectName(_project), configuration, platform));
        }

        public void EndBuild(bool succeed)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _outputPane.OutputString(
                string.Format("------ Build {0} ------\n\n", succeed ? "succeeded" : "failed"));
            Package.Instance.BuildDone();
        }
    }
}
