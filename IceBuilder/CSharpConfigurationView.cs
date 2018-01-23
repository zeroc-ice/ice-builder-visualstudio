// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using System.IO;

namespace IceBuilder
{
    public partial class CSharpConfigurationView : UserControl
    {
        private PropertyPage Page
        {
            get;
            set;
        }

        public CSharpConfigurationView(PropertyPage page)
        {
            Page = page;
            InitializeComponent();
        }

        public void LoadSettigns(ProjectSettigns settings)
        {
            OutputDir = settings.OutputDir;
            IncludeDirectories = settings.IncludeDirectories;
            AdditionalOptions = settings.AdditionalOptions;
            Dirty = false;
        }

        public virtual void Initialize(Control parent, Rectangle rect)
        {
            SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
            Parent = parent;
        }

        public int ProcessAccelerator(ref Message keyboardMessage)
        {
            if(FromHandle(keyboardMessage.HWnd).PreProcessMessage(ref keyboardMessage))
            {
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        public readonly uint PageStatusDirty = 0x1;
        public readonly uint PageStatusClean = 0x4;
        public bool _dirty;
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                _dirty = value;
                if(Page.PageSite != null)
                {
                    Page.PageSite.OnStatusChange(value ? PageStatusDirty : PageStatusClean);
                }
            }
        }

        public string OutputDir
        {
            get
            {
                return txtOutputDir.Text;
            }
            set
            {
                txtOutputDir.Text = value;
            }
        }

        public string AdditionalOptions
        {
            get
            {
                return txtAdditionalOptions.Text;
            }
            set
            {
                txtAdditionalOptions.Text = value;
            }
        }

        public string IncludeDirectories
        {
            get
            {
                return txtIncludeDirectories.Text;
            }
            set
            {
                txtIncludeDirectories.Text = value;
            }
        }

        private void btnOutputDirectoryBrowse_Click(object sender, EventArgs e)
        {
            string projectDir = Path.GetFullPath(Path.GetDirectoryName(ProjectUtil.GetProjectFullPath(Page.Project)));
            string selectedPath = UIUtil.BrowserFolderDialog(Handle, "Output Directory", projectDir);
            if(!string.IsNullOrEmpty(selectedPath))
            {
                selectedPath = FileUtil.RelativePath(projectDir, selectedPath);
                OutputDir = string.IsNullOrEmpty(selectedPath) ? "." : selectedPath;
                if(!txtOutputDir.Text.Equals(Page.Settings.OutputDir))
                {
                    Dirty = isDirty();
                }
            }
        }

        private void OutputDirectory_Leave(object sender, EventArgs e)
        {
            if(!txtOutputDir.Text.Equals(Page.Settings.OutputDir))
            {
                Dirty = isDirty();
            }
        }

        private void AdditionalOptions_Leave(object sender, EventArgs e)
        {
            if(!txtAdditionalOptions.Text.Equals(Page.Settings.AdditionalOptions))
            {
                Dirty = isDirty();
            }
        }

        private void txtOutputDir_TextChanged(object sender, EventArgs e)
        {
            if(!txtOutputDir.Text.Equals(Page.Settings.OutputDir))
            {
                Dirty = isDirty();
            }
        }

        private void txtIncludeDirectories_TextChanged(object sender, EventArgs e)
        {
            if (!txtIncludeDirectories.Text.Equals(Page.Settings.IncludeDirectories))
            {
                Dirty = isDirty();
            }
        }

        private void txtAdditionalOptions_TextChanged(object sender, EventArgs e)
        {
            if (!txtAdditionalOptions.Text.Equals(Page.Settings.AdditionalOptions))
            {
                Dirty = isDirty();
            }
        }

        private bool isDirty()
        {
            return !txtOutputDir.Text.Equals(Page.Settings.OutputDir) ||
                !txtIncludeDirectories.Text.Equals(Page.Settings.IncludeDirectories) ||
                !txtAdditionalOptions.Text.Equals(Page.Settings.AdditionalOptions);
        }
    }
}
