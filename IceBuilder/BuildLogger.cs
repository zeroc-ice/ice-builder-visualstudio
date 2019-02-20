// **********************************************************************
//
// Copyright (c) ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Text;

using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell;

using System.Diagnostics;
using System.IO;

namespace IceBuilder
{
    public class BuildLogger : Logger
    {
        EnvDTE.OutputWindowPane OutputPane
        {
            get;
            set;
        }

        public BuildLogger(EnvDTE.OutputWindowPane outputPane)
        {
            OutputPane = outputPane;
            IndentLevel = 2;
        }

        public override void Initialize(IEventSource eventSource)
        {
            eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
            eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);

            eventSource.TargetStarted += new TargetStartedEventHandler(eventSource_TargetStarted);
            eventSource.TargetFinished += new TargetFinishedEventHandler(eventSource_TargetFinished);

            eventSource.MessageRaised += new BuildMessageEventHandler(eventSource_MessageRaised);
            eventSource.WarningRaised += new BuildWarningEventHandler(eventSource_WarningRaised);
            eventSource.ErrorRaised += new BuildErrorEventHandler(eventSource_ErrorRaised);
        }

        void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Stopwatch = Stopwatch.StartNew();
            WriteMessage(string.Format("Build started {0}.", DateTime.Now));
        }

        void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Stopwatch.Stop();
            WriteMessage(string.Format("\nBuild {0}.", (e.Succeeded ? "succeeded" : "FAILED")));
            WriteMessage(string.Format("Time Elapsed {0:00}:{1:00}:{2:00}.{3:00}",
                Stopwatch.Elapsed.Hours,
                Stopwatch.Elapsed.Minutes,
                Stopwatch.Elapsed.Seconds,
                Stopwatch.Elapsed.Milliseconds));
            Stopwatch = null;
        }

        public void eventSource_TargetStarted(object sender, TargetStartedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (e.TargetName.Equals("SliceCompile") || IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            {
                WriteMessage(string.Format("{0}:", e.TargetName));
            }
            Indent += IndentLevel;
        }

        public void eventSource_TargetFinished(object sender, TargetFinishedEventArgs e)
        {
            Indent -= IndentLevel;
        }

        public void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal)) ||
               (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal)) ||
               (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed)))
            {
                WriteMessage(e.Message);
            }
        }

        public void WriteMessage(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            StringBuilder s = new StringBuilder();
            for(int i = 0; i < Indent; ++i)
            {
                s.Append(" ");
            }
            s.AppendLine(message);

            OutputPane.Activate();
            OutputPane.OutputString(s.ToString());
        }

        private void OutputTaskItem(string message, EnvDTE.vsTaskPriority priority, string subcategory,
                                    string file, int line, string description)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OutputTaskItem(
                    string.Format("{0}({1}): warning : {2}",
                        Path.Combine(Path.GetDirectoryName(e.ProjectFile), e.File),
                        e.LineNumber,
                        e.Message),
                    EnvDTE.vsTaskPriority.vsTaskPriorityMedium, "",
                    e.File,
                    e.LineNumber,
                    e.Message);
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OutputTaskItem(
                string.Format("{0}({1}): error : {2}",
                    Path.Combine(Path.GetDirectoryName(e.ProjectFile), e.File),
                    e.LineNumber,
                    e.Message),
                EnvDTE.vsTaskPriority.vsTaskPriorityHigh, "",
                e.File,
                e.LineNumber,
                e.Message);
        }

        public override void Shutdown()
        {
        }

        private Stopwatch Stopwatch
        {
            get;
            set;
        }

        private int Indent
        {
            get;
            set;
        }

        private int IndentLevel
        {
            get;
            set;
        }
    }
}
