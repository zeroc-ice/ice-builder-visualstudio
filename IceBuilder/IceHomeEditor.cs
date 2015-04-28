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

        public void Initialize()
        {
            txtIceHome.Text = optionsPage.IceHome;
        }

        public void ClearErrors()
        {
            lblInfo.Text = "";
        }

        private void btnIceHome_Click(object sender, EventArgs e)
        {
            String selectedPath = UIUtil.BrowserFolderDialog(Handle, "Ice Home Location",
                String.IsNullOrEmpty(txtIceHome.Text) ? 
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : txtIceHome.Text);

            lblInfo.Text = "";
            selectedPath = String.IsNullOrEmpty(selectedPath) ? String.Empty : selectedPath;

            if (!selectedPath.Equals(optionsPage.IceHome))
            {
                if (String.IsNullOrEmpty(selectedPath) ||
                    File.Exists(Path.Combine(selectedPath, "bin", "slice2cpp.exe")) ||
                    File.Exists(Path.Combine(selectedPath, "cpp", "bin", "slice2cpp.exe")))
                {                 
                    txtIceHome.Text = selectedPath;
                    optionsPage.IceHome = txtIceHome.Text;
                    optionsPage.SaveSettingsToStorage();
                }
                else
                {
                    lblInfo.Text =
                        String.Format("Invalid Ice Home Location:\r\n\"{0}\"", selectedPath);
                }
            }
        }
    }
}
