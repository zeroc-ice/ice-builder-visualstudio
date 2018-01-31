// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

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
            OutputDir = GetProperty(ItemMetadataNames.OutputDir);
            IncludeDirectories = GetProperty(ItemMetadataNames.IncludeDirectories);
            AdditionalOptions = GetProperty(ItemMetadataNames.AdditionalOptions);
        }

        public void Save()
        {
            SetPropertyIfChanged(ItemMetadataNames.OutputDir, OutputDir);
            SetPropertyIfChanged(ItemMetadataNames.IncludeDirectories, IncludeDirectories);
            SetPropertyIfChanged(ItemMetadataNames.AdditionalOptions, AdditionalOptions);
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

        public IVsProject Project
        {
            get;
            private set;
        }

        private string GetProperty(string name)
        {
            return Project.GetDefaultItemMetadata(name, false, string.Empty);
        }

        private void SetProperty(string name, string value)
        {
            Project.SetItemMetadata(name, value);
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
