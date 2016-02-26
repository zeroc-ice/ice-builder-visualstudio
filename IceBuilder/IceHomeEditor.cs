// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
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

using System.IO;

namespace IceBuilder
{
    public partial class IceHomeEditor : UserControl
    {
        public IceHomeEditor()
        {
            InitializeComponent();
        }

        internal IceOptionsPage optionsPage;

        public String IceHome
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

        public bool SetIceHome(String path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                if (String.IsNullOrEmpty(path) ||
                    File.Exists(Path.Combine(path, "bin", "slice2cpp.exe")) ||
                    File.Exists(Path.Combine(path, "cpp", "bin", "slice2cpp.exe")) ||
                    File.Exists(Path.Combine(path, "cpp", "config", "Ice.props")) ||
                    File.Exists(Path.Combine(path, "config", "Ice.props")) ||
                    File.Exists(Path.Combine(path, "build", "native", "Ice.props")))
                {
                    txtIceHome.Text = path;
                    return true;
                }
                else
                {
                    lblInfo.Text =
                        String.Format("Invalid Ice home directory:\r\n\"{0}\"", path);
                    return false;
                }
            }
            return true;
        }

        private void btnIceHome_Click(object sender, EventArgs e)
        {
            String selectedPath = UIUtil.BrowserFolderDialog(Handle, "Select Folder",
                String.IsNullOrEmpty(txtIceHome.Text) ? 
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : txtIceHome.Text);

            lblInfo.Text = "";

            SetIceHome(selectedPath);
        }

        public bool AutoBuilding
        {
            set
            {
                autoBuild.Checked = value;
            }
            get
            {
                return autoBuild.Checked;
            }
        }
    }
}
