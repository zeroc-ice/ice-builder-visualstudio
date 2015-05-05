// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

using Microsoft.VisualStudio.Shell.Interop;

using System.Windows.Threading;
using System.Diagnostics;
using System.IO;

using Microsoft.VisualStudio;

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

        public override void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
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
            Stopwatch = Stopwatch.StartNew();
            WriteMessage(String.Format("Build started {0}.", System.DateTime.Now));
        }

        void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            Stopwatch.Stop();
            WriteMessage(String.Format("\nBuild {0}.", (e.Succeeded ? "succeeded" : "FAILED")));
            WriteMessage(String.Format("Time Elapsed {0:00}:{1:00}:{2:00}.{3:00}",
                Stopwatch.Elapsed.Hours,
                Stopwatch.Elapsed.Minutes,
                Stopwatch.Elapsed.Seconds,
                Stopwatch.Elapsed.Milliseconds));
            Stopwatch = null;
        }

        public void eventSource_TargetStarted(object sender, TargetStartedEventArgs e)
        {
            if(e.TargetName.Equals("IceBuilder_Compile") ||IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            {
                WriteMessage(String.Format("{0}:", e.TargetName));
            }
            Indent += IndentLevel;
        }

        public void eventSource_TargetFinished(object sender, TargetFinishedEventArgs e)
        {
            Indent -= IndentLevel; 
        }

        public void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            if((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal)) ||
               (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal)) ||
			   (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed)))
            {
                WriteMessage(e.Message);
            }
        }

        public void WriteMessage(String message)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < Indent; ++i)
            {
                s.Append(" ");
            }
            s.AppendLine(message);

            OutputPane.Activate();
            OutputPane.OutputString(s.ToString());
        }

        private void OutputTaskItem(String message, EnvDTE.vsTaskPriority priority, String subcategory,
                                    String file, int line, String description)
        {
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
            OutputTaskItem(
                    String.Format("{0}({1}): warning : {2}",
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
            OutputTaskItem(
                String.Format("{0}({1}): error : {2}",
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
