// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace IceBuilder
{
    public partial class CSharpConfigurationView : UserControl
    {
        private PropertyPage Page { get; set; }

        public CSharpConfigurationView(PropertyPage page)
        {
            Page = page;
            InitializeComponent();
        }

        public void LoadSettigns(ProjectSettigns settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var enabled = settings.IsMSBuildIceBuilderInstalled();
            Enable(enabled);
            if (enabled)
            {
                OutputDir = settings.OutputDir;
                IncludeDirectories = settings.IncludeDirectories;
                AdditionalOptions = settings.AdditionalOptions;
                Dirty = false;
            }
        }

        private void Enable(bool enabled)
        {
            txtOutputDir.Enabled = enabled;
            txtIncludeDirectories.Enabled = enabled;
            txtAdditionalOptions.Enabled = enabled;
            btnOutputDirectoryBrowse.Enabled = enabled;
        }

        public virtual void Initialize(Control parent, Rectangle rect)
        {
            SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
            Parent = parent;
        }

        public int ProcessAccelerator(ref Message keyboardMessage)
        {
            if (FromHandle(keyboardMessage.HWnd).PreProcessMessage(ref keyboardMessage))
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
            get => _dirty;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _dirty = value;
                if (Page.PageSite != null)
                {
                    Page.PageSite.OnStatusChange(value ? PageStatusDirty : PageStatusClean);
                }
            }
        }

        public string OutputDir
        {
            get => txtOutputDir.Text;
            set => txtOutputDir.Text = value;
        }

        public string AdditionalOptions
        {
            get => txtAdditionalOptions.Text;
            set => txtAdditionalOptions.Text = value;
        }

        public string IncludeDirectories
        {
            get => txtIncludeDirectories.Text;
            set => txtIncludeDirectories.Text = value;
        }

        private void BtnOutputDirectoryBrowse_Click(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string projectDir = Page.Project.GetProjectBaseDirectory();
            string selectedPath = UIUtil.BrowserFolderDialog(Handle, "Output Directory", projectDir);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                selectedPath = FileUtil.RelativePath(projectDir, selectedPath);
                OutputDir = string.IsNullOrEmpty(selectedPath) ? "." : selectedPath;
                if (!txtOutputDir.Text.Equals(Page.Settings.OutputDir))
                {
                    Dirty = IsDirty();
                }
            }
        }

        private void OutputDirectory_Leave(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!txtOutputDir.Text.Equals(Page.Settings.OutputDir))
            {
                Dirty = IsDirty();
            }
        }

        private void AdditionalOptions_Leave(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!txtAdditionalOptions.Text.Equals(Page.Settings.AdditionalOptions))
            {
                Dirty = IsDirty();
            }
        }

        private void TxtOutputDir_TextChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!txtOutputDir.Text.Equals(Page.Settings.OutputDir))
            {
                Dirty = IsDirty();
            }
        }

        private void TxtIncludeDirectories_TextChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!txtIncludeDirectories.Text.Equals(Page.Settings.IncludeDirectories))
            {
                Dirty = IsDirty();
            }
        }

        private void TxtAdditionalOptions_TextChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!txtAdditionalOptions.Text.Equals(Page.Settings.AdditionalOptions))
            {
                Dirty = IsDirty();
            }
        }

        private bool IsDirty() =>
            !txtOutputDir.Text.Equals(Page.Settings.OutputDir) ||
            !txtIncludeDirectories.Text.Equals(Page.Settings.IncludeDirectories) ||
            !txtAdditionalOptions.Text.Equals(Page.Settings.AdditionalOptions);
    }
}
