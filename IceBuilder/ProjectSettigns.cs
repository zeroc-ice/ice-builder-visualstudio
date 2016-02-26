// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
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
            AllowIcePrefix = GetPropertyAsBool(PropertyNames.AllowIcePrefix);
            Checksum = GetPropertyAsBool(PropertyNames.Checksum);
            Stream = GetPropertyAsBool(PropertyNames.Stream);
            Tie = GetPropertyAsBool(PropertyNames.Tie);
            Underscore = GetPropertyAsBool(PropertyNames.Underscore);
            IncludeDirectories = GetProperty(PropertyNames.IncludeDirectories);
            AdditionalOptions = GetProperty(PropertyNames.AdditionalOptions);
        }

        public void Save()
        {
            SetPropertyIfChanged(PropertyNames.OutputDir, OutputDir);
            SetPropertyAsBoolIfChanged(PropertyNames.AllowIcePrefix, AllowIcePrefix);
            SetPropertyAsBoolIfChanged(PropertyNames.Checksum, Checksum);
            SetPropertyAsBoolIfChanged(PropertyNames.Stream, Stream);
            SetPropertyAsBoolIfChanged(PropertyNames.Tie, Tie);
            SetPropertyAsBoolIfChanged(PropertyNames.Underscore, Underscore);
            SetPropertyIfChanged(PropertyNames.IncludeDirectories, IncludeDirectories);
            SetPropertyIfChanged(PropertyNames.AdditionalOptions, AdditionalOptions);
        }

        public String OutputDir
        {
            get;
            set;
        }

        public Boolean AllowIcePrefix
        {
            get;
            set;
        }

        public Boolean Checksum
        {
            get;
            set;
        }

        public Boolean Stream
        {
            get;
            set;
        }

        public Boolean Tie
        {
            get;
            set;
        }

        public Boolean Underscore
        {
            get;
            set;
        }

        public String IncludeDirectories
        {
            get;
            set;
        }

        public String AdditionalOptions
        {
            get;
            set;
        }

        private IVsProject Project
        {
            get;
            set;
        }

        private String GetProperty(String name)
        {
            return ProjectUtil.GetProperty(Project, name);
        }

        private bool GetPropertyAsBool(String name)
        {
            return GetProperty(name).Equals("yes", StringComparison.CurrentCultureIgnoreCase);
        }

        private void SetPropertyAsBool(String name, bool value)
        {
            SetProperty(name, value ? "yes" : "");
        }

        private void SetProperty(String name, String value)
        {
            ProjectUtil.SetProperty(Project, name, value);
        }

        private void SetPropertyIfChanged(String name, String value)
        {
            if (!GetProperty(name).Equals(value))
            {
                SetProperty(name, value);
            }
        }

        private void SetPropertyAsBoolIfChanged(String name, bool value)
        {
            if (GetPropertyAsBool(name) != value)
            {
                SetPropertyAsBool(name, value);
            }
        }
    }
}
