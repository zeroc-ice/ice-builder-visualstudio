// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Windows.Forms;

namespace IceBuilder
{
    public partial class IceHomeEditor : UserControl
    {
        public IceHomeEditor() => InitializeComponent();

        internal IceOptionsPage optionsPage;

        public string IceHome
        {
            get
            {
                return txtIceHome.Text;
            }
            set
            {
                txtIceHome.Text = value;
                lblInfo.Text = "";
            }
        }

        public bool SetIceHome(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(Path.Combine(path, "config", "ice.props")) ||
                    File.Exists(Path.Combine(path, "config", "icebuilder.props")))
                {
                    txtIceHome.Text = path;
                }
                else
                {
                    lblInfo.Text =
                        string.Format("Invalid Ice home directory:\r\n\"{0}\"\r\n", path);
                    return false;
                }
            }
            return true;
        }

        private void BtnIceHome_Click(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string selectedPath = UIUtil.BrowserFolderDialog(Handle, "Select Folder",
                string.IsNullOrEmpty(txtIceHome.Text) ?
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : txtIceHome.Text);

            lblInfo.Text = "";

            SetIceHome(selectedPath);
        }

        public bool AutoBuilding
        {
            set => autoBuild.Checked = value;
            get => autoBuild.Checked;
        }
    }
}
