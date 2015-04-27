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

namespace IceBuilder
{
    public class BuildLogger : Logger
    {
        public BuildLogger(IVsOutputWindowPane outputPane, LoggerVerbosity verbosity)
        {
            OutputPane = outputPane;
            Indent = 0;
            IndentLevel = 2;
            Verbosity = verbosity;
        }

        public override void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
        {
            eventSource.ProjectStarted += eventSource_ProjectStarted;
            eventSource.ProjectFinished += eventSource_ProjectFinished;
            eventSource.TargetStarted += eventSource_TargetStarted;
            eventSource.TargetFinished += eventSource_TargetFinished;

            eventSource.MessageRaised += eventSource_MessageRaised;
            eventSource.WarningRaised += eventSource_WarningRaised;
            eventSource.ErrorRaised += eventSource_ErrorRaised;
            OutputPane.Activate();
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
            if(e.TargetName.Equals("IceBuilder_Compile") ||
               IsVerbosityAtLeast(LoggerVerbosity.Detailed))
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

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            throw new NotImplementedException();
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            OutputPane.OutputTaskItemString(
                String.Format("{0}({1}): error : {2}", 
                    Path.Combine(Path.GetDirectoryName(e.ProjectFile), e.File),
                    e.LineNumber,
                    e.Message),
                VSTASKPRIORITY.TP_HIGH, VSTASKCATEGORY.CAT_BUILDCOMPILE, "error", 0, 
                e.File,   
                (uint)e.LineNumber -1, 
                e.Message);
            OutputPane.FlushToTaskList();
        }

        private void WriteMessage(String message)
        {
            StringBuilder s = new StringBuilder();
            for(int i = 0; i < Indent; ++i)
            {
                s.Append(" ");
            }
            s.AppendLine(message);
            OutputPane.OutputString(s.ToString());
        }

        public override void Shutdown()
        {
        }

        private int IndentLevel
        {
            get;
            set;
        }
        private int Indent
        {
            get;
            set;
        }

        private IVsOutputWindowPane OutputPane
        {
            get;
            set;
        }

        private Stopwatch Stopwatch
        {
            get;
            set;
        }
    }
}
