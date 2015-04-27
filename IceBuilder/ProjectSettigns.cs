using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IceBuilder
{
    public class ProjectSettigns
    {
        public readonly String OutputDirPropertyName = "OutputDir";
        public readonly String IcePropertyName = "Ice";
        public readonly String ChecksumPropertyName = "Checksum";
        public readonly String StreamingPropertyName = "Stream";
        public readonly String TiePropertyName = "Tie";
        public readonly String UnderscoresPropertyName = "Underscore";
        public readonly String AdditionalIncludeDirectoriesPropertyName = "AdditionalIncludeDirectories";
        public readonly String AdditionalOptionsPropertyName = "AdditionalOptions";

        public Boolean NeedSave
        {
            get
            {
                return !OutputDir.Equals(GetProperty(OutputDirPropertyName)) ||
                    Ice != GetPropertyAsBool(IcePropertyName) ||
                    Checksum != GetPropertyAsBool(ChecksumPropertyName) ||
                    Streaming != GetPropertyAsBool(StreamingPropertyName) ||
                    Tie != GetPropertyAsBool(TiePropertyName) ||
                    Underscores != GetPropertyAsBool(UnderscoresPropertyName) ||
                    !AdditionalIncludeDirectories.Equals(GetProperty(AdditionalIncludeDirectories)) ||
                    !AdditionalOptions.Equals(GetProperty(AdditionalOptionsPropertyName));
            }
        }

        public ProjectSettigns(EnvDTE.Project project)
        {
            Project = project;
        }

        public void Load()
        {
            OutputDir = GetProperty(OutputDirPropertyName);
            Ice = GetPropertyAsBool(IcePropertyName);
            Checksum = GetPropertyAsBool(ChecksumPropertyName);
            Streaming = GetPropertyAsBool(StreamingPropertyName);
            Tie = GetPropertyAsBool(TiePropertyName);
            Underscores = GetPropertyAsBool(UnderscoresPropertyName);
            AdditionalIncludeDirectories = GetProperty(AdditionalIncludeDirectoriesPropertyName);
            AdditionalOptions = GetProperty(AdditionalOptionsPropertyName);
        }

        public void Save()
        {
            SetPropertyIfChanged(OutputDirPropertyName, OutputDir);
            SetPropertyAsBoolIfChanged(IcePropertyName, Ice);
            SetPropertyAsBoolIfChanged(ChecksumPropertyName, Checksum);
            SetPropertyAsBoolIfChanged(StreamingPropertyName, Streaming);
            SetPropertyAsBoolIfChanged(TiePropertyName, Tie);
            SetPropertyAsBoolIfChanged(UnderscoresPropertyName, Underscores);
            SetPropertyIfChanged(AdditionalIncludeDirectoriesPropertyName, AdditionalIncludeDirectories);
            SetPropertyIfChanged(AdditionalOptionsPropertyName, AdditionalOptions);
        }

        public String OutputDir
        {
            get;
            set;
        }

        public Boolean Ice
        {
            get;
            set;
        }

        public Boolean Checksum
        {
            get;
            set;
        }

        public Boolean Streaming
        {
            get;
            set;
        }

        public Boolean Tie
        {
            get;
            set;
        }

        public Boolean Underscores
        {
            get;
            set;
        }

        public String AdditionalIncludeDirectories
        {
            get;
            set;
        }

        public String AdditionalOptions
        {
            get;
            set;
        }

        private EnvDTE.Project Project
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
