// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class BuildCallback
    {
        public BuildCallback(EnvDTE.Project project, IVsOutputWindowPane outputPane, 
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
                String.Format("------ Build {0} ------\n", (succeed ? "succeeded" : "failed")));
            Package.Instance.BuildDone(succeed);
        }

        EnvDTE.Project Project
        {
            get;
            set;
        }

        IVsOutputWindowPane OutputPane
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
