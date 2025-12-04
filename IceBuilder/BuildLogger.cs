// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace IceBuilder;

public class BuildLogger : Logger
{
    private int Indent { get; set; }
    private int IndentLevel { get; set; }
    private EnvDTE.OutputWindowPane OutputPane { get; set; }
    private Stopwatch Stopwatch { get; set; }

    public BuildLogger(EnvDTE.OutputWindowPane outputPane)
    {
        OutputPane = outputPane;
        IndentLevel = 2;
    }

    public override void Initialize(IEventSource eventSource)
    {
        eventSource.ProjectStarted += new ProjectStartedEventHandler(EventSource_ProjectStarted);
        eventSource.ProjectFinished += new ProjectFinishedEventHandler(EventSource_ProjectFinished);

        eventSource.TargetStarted += new TargetStartedEventHandler(EventSource_TargetStarted);
        eventSource.TargetFinished += new TargetFinishedEventHandler(EventSource_TargetFinished);

        eventSource.MessageRaised += new BuildMessageEventHandler(EventSource_MessageRaised);
        eventSource.WarningRaised += new BuildWarningEventHandler(EventSource_WarningRaised);
        eventSource.ErrorRaised += new BuildErrorEventHandler(EventSource_ErrorRaised);
    }

    void EventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
    {
        Stopwatch = Stopwatch.StartNew();
        WriteMessage(string.Format("Build started {0}.", DateTime.Now));
    }

    void EventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
    {
        Stopwatch.Stop();
        WriteMessage(string.Format("\nBuild {0}.", e.Succeeded ? "succeeded" : "FAILED"));
        WriteMessage(string.Format("Time Elapsed {0:00}:{1:00}:{2:00}.{3:00}",
            Stopwatch.Elapsed.Hours,
            Stopwatch.Elapsed.Minutes,
            Stopwatch.Elapsed.Seconds,
            Stopwatch.Elapsed.Milliseconds));
        Stopwatch = null;
    }

    public void EventSource_TargetStarted(object sender, TargetStartedEventArgs e)
    {
        if (e.TargetName.Equals("SliceCompile") || IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            WriteMessage(string.Format("{0}:", e.TargetName));
        }
        Indent += IndentLevel;
    }

    public void EventSource_TargetFinished(object sender, TargetFinishedEventArgs e) => Indent -= IndentLevel;

    public void EventSource_MessageRaised(object sender, BuildMessageEventArgs e)
    {
        if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal)) ||
            (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal)) ||
            (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed)))
        {
            WriteMessage(e.Message);
        }
    }

    public void WriteMessage(string message)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < Indent; ++i)
            {
                s.Append(" ");
            }
            s.AppendLine(message);

            OutputPane.Activate();
            OutputPane.OutputString(s.ToString());
        });
    }

    private void OutputTaskItem(
        string message,
        EnvDTE.vsTaskPriority priority,
        string file,
        int line,
        string description)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                OutputPane.Activate();
                OutputPane.OutputTaskItemString(
                    message,
                    priority,
                    EnvDTE.vsTaskCategories.vsTaskCategoryBuildCompile,
                    EnvDTE.vsTaskIcon.vsTaskIconCompile,
                    file,
                    line,
                    description,
                    true);
            });
    }

    void EventSource_WarningRaised(object sender, BuildWarningEventArgs e) =>
        OutputTaskItem(
            $"{Path.Combine(Path.GetDirectoryName(e.ProjectFile), e.File)}({e.LineNumber}: warning : {e.Message}",
            EnvDTE.vsTaskPriority.vsTaskPriorityMedium,
            e.File,
            e.LineNumber,
            e.Message);

    void EventSource_ErrorRaised(object sender, BuildErrorEventArgs e) =>
        OutputTaskItem(
            $"{Path.Combine(Path.GetDirectoryName(e.ProjectFile), e.File)}({e.LineNumber}): error : {e.Message}",
            EnvDTE.vsTaskPriority.vsTaskPriorityHigh,
            e.File,
            e.LineNumber,
            e.Message);

    public override void Shutdown()
    {
    }
}
