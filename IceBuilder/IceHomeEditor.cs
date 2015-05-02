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

        private void btnIceHome_Click(object sender, EventArgs e)
        {
            String selectedPath = UIUtil.BrowserFolderDialog(Handle, "Select Folder",
                String.IsNullOrEmpty(txtIceHome.Text) ? 
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : txtIceHome.Text);

            lblInfo.Text = "";

            if(!String.IsNullOrEmpty(selectedPath) && !selectedPath.Equals(txtIceHome.Text))
            {
                if (String.IsNullOrEmpty(selectedPath) ||
                    File.Exists(Path.Combine(selectedPath, "bin", "slice2cpp.exe")) ||
                    File.Exists(Path.Combine(selectedPath, "cpp", "bin", "slice2cpp.exe")))
                {                 
                    txtIceHome.Text = selectedPath;
                }
                else
                {
                    lblInfo.Text =
                        String.Format("Invalid Ice home directory:\r\n\"{0}\"", selectedPath);
                }
            }
        }
    }
}
