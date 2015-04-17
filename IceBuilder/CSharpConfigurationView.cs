// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using System.IO;

namespace IceBuilder
{
    public partial class CSharpConfigurationView : UserControl
    {
        private PropertyPage _page;
        public CSharpConfigurationView(PropertyPage page)
        {
            _page = page;
            InitializeComponent();
            includeDirectories.PropertyPage = page;
        }

        public virtual void Initialize(Control parent, Rectangle rect)
        {
            base.SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
            base.Parent = parent;
        }

        public int ProcessAccelerator(ref Message keyboardMessage)
        {
            if (Control.FromHandle(keyboardMessage.HWnd).PreProcessMessage(ref keyboardMessage))
            {
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        public bool _needSave;
        public Boolean NeedSave
        {
            get 
            {
                return _needSave || includeDirectories.NeedSave;
            }
            set
            {
                _needSave = value;
            }
        }

        private bool _outputDirMultipleValues;
        public Boolean OutputDirMultipleValues
        {
            get
            {
                return _outputDirMultipleValues;
            }
            set
            {
                _outputDirMultipleValues = value;
            }
        }

        public String OutputDir
        {
            get
            {
                return txtOutputDir.Text;
            }
            set
            {
                txtOutputDir.Text = value;
                OutputDirMultipleValues = false;
            }
        }

        public CheckState Ice
        {
            get
            {
                return chkIce.CheckState;
            }
            set
            {
                chkIce.CheckState = value;
            }
        }

        public CheckState Checksum
        {
            get
            {
                return chkChecksum.CheckState;
            }
            set
            {
                chkChecksum.CheckState = value;
            }
        }

        public CheckState Streaming
        {
            get
            {
                return chkStreaming.CheckState;
            }
            set
            {
                chkStreaming.CheckState = value;
            }
        }

        public CheckState Tie
        {
            get
            {
                return chkTie.CheckState;
            }
            set
            {
                chkTie.CheckState = value;
            }
        }

        public CheckState Underscores
        {
            get
            {
                return chkUnderscores.CheckState;
            }
            set
            {
                chkUnderscores.CheckState = value;
            }
        }

        public String AdditionalOptions
        {
            get
            {
                return txtAdditionalOptions.Text;
            }
            set
            {
                txtAdditionalOptions.Text = value;
                AdditionalOptionsMultipleValues = false;
            }
        }

        private bool _additionalOptionsMultipleValues;
        public Boolean AdditionalOptionsMultipleValues
        {
            get
            {
                return _additionalOptionsMultipleValues;
            }
            set
            {
                _additionalOptionsMultipleValues = value;
            }
        }

        private bool _traceLevelMultipleValues;
        public Boolean TraceLevelMultipleValues
        {
            get
            {
                return _traceLevelMultipleValues;
            }
            set
            {
                _traceLevelMultipleValues = value;
            }
        }

        public IncludeDirectories AdditionalIncludeDirectories
        {
            get
            {
                return includeDirectories;
            }
        }

        private void txtOutputDir_TextChanged(object sender, EventArgs e)
        {
            NeedSave = true;
        }

        void option_CheckedChanged(object sender, System.EventArgs e)
        {
            NeedSave = true;
        }

        private void btnOutputDirectoryBrowse_Click(object sender, EventArgs e)
        {
            String projectDir = _page.GetProperty("MSBuildProjectDirectory");
            String selectedPath = UIUtil.BrowserFolderDialog(Handle, "Select Output Directory", projectDir);
            if (!String.IsNullOrEmpty(selectedPath))
            {
                OutputDir = FileUtil.RelativePath(projectDir, selectedPath);
            }
        }

        private void txtAdditionalOptions_TextChanged(object sender, EventArgs e)
        {
            NeedSave = true;
            AdditionalOptionsMultipleValues = false;
        }

        private void cmbTraceLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            NeedSave = true;
            TraceLevelMultipleValues = false;
        }
    }
}
