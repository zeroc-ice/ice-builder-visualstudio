// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class ProjectSettigns
    {
        public ProjectSettigns(IVsProject project)
        {
            Project = project;
        }

        public void Load()
        {
            OutputDir = GetProperty(PropertyNames.OutputDir);
            Checksum = GetPropertyAsBool(PropertyNames.Checksum);
            Tie = GetPropertyAsBool(PropertyNames.Tie);
            IncludeDirectories = GetProperty(PropertyNames.IncludeDirectories);
            AdditionalOptions = GetProperty(PropertyNames.AdditionalOptions);
        }

        public void Save()
        {
            SetPropertyIfChanged(PropertyNames.OutputDir, OutputDir);
            SetPropertyAsBoolIfChanged(PropertyNames.Checksum, Checksum);
            SetPropertyAsBoolIfChanged(PropertyNames.Tie, Tie);
            SetPropertyIfChanged(PropertyNames.IncludeDirectories, IncludeDirectories);
            SetPropertyIfChanged(PropertyNames.AdditionalOptions, AdditionalOptions);
        }

        public string OutputDir
        {
            get;
            set;
        }

        public bool Checksum
        {
            get;
            set;
        }

        public bool Tie
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

        private IVsProject Project
        {
            get;
            set;
        }

        private string GetProperty(string name)
        {
            return ProjectUtil.GetProperty(Project, name);
        }

        private bool GetPropertyAsBool(string name)
        {
            return GetProperty(name).Equals("yes", StringComparison.CurrentCultureIgnoreCase);
        }

        private void SetPropertyAsBool(string name, bool value)
        {
            SetProperty(name, value ? "yes" : "");
        }

        private void SetProperty(string name, string value)
        {
            ProjectUtil.SetProperty(Project, name, value);
        }

        private void SetPropertyIfChanged(string name, string value)
        {
            if(!GetProperty(name).Equals(value))
            {
                SetProperty(name, value);
            }
        }

        private void SetPropertyAsBoolIfChanged(string name, bool value)
        {
            if(GetPropertyAsBool(name) != value)
            {
                SetPropertyAsBool(name, value);
            }
        }
    }
}
