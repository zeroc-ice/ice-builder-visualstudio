// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class ProjectSettigns
    {
        public ProjectSettigns(IVsProjectManager projectManager)
        {
            ProjectManager = projectManager;
        }

        public void Load()
        {
            OutputDir = GetProperty(PropertyNames.New.OutputDir);
            IncludeDirectories = GetProperty(PropertyNames.New.IncludeDirectories);
            AdditionalOptions = GetProperty(PropertyNames.New.AdditionalOptions);
        }

        public void Save()
        {
            SetPropertyIfChanged(PropertyNames.New.OutputDir, OutputDir);
            SetPropertyIfChanged(PropertyNames.New.IncludeDirectories, IncludeDirectories);
            SetPropertyIfChanged(PropertyNames.New.AdditionalOptions, AdditionalOptions);
        }

        public string OutputDir
        {
            get;
            set;
        }

        public string IncludeDirectories
        {
            get;
            set;
        }

        public string AdditionalOptions
        {
            get;
            set;
        }

        public IVsProjectManager ProjectManager
        {
            get;
            private set;
        }

        private string GetProperty(string name)
        {
            return ProjectManager.GetProjectProperty(name);
        }

        private void SetProperty(string name, string value)
        {
            ProjectManager.SetProjectProperty(name, value);
        }

        private void SetPropertyIfChanged(string name, string value)
        {
            if(!GetProperty(name).Equals(value))
            {
                SetProperty(name, value);
            }
        }
    }
}
