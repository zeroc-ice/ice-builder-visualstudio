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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ZeroC.IceVisualStudio
{
    public partial class IceHomeEditor : UserControl
    {
        public IceHomeEditor()
        {
            InitializeComponent();
            toolTip.SetToolTip(txtIceHome, "Ice SDK Location");
            toolTip.SetToolTip(btnSelectIceHome, "Ice SDK Location");
        }

        internal IceOptionsPage optionsPage;

        public void Initialize()
        {
            txtIceHome.Text = optionsPage.IceHome;
        }

        private void txtIceHome_LostFocus(object sender, EventArgs e)
        {
            validateIceHome();
        }

        private void btnSelectIceHome_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = optionsPage.IceHome;
            dialog.Description = "Select Ice SDK Location";
            DialogResult r = dialog.ShowDialog();
            if(r == DialogResult.OK)
            {
                txtIceHome.Text = dialog.SelectedPath;
                validateIceHome();
            }
        }

        private void updateIceHome()
        {
            optionsPage.IceHome = txtIceHome.Text;
            optionsPage.SaveSettingsToStorage();
        }

        private bool validateIceHome()
        {
            if(txtIceHome.Text == optionsPage.IceHome)
            {
                lblInfo.Text = "";
                lblInfo.Visible = false;
                return true;
            }
            else
            {
                try
                {
                    if(String.IsNullOrEmpty(txtIceHome.Text) ||
                       File.Exists(Path.Combine(txtIceHome.Text, "bin", "slice2cpp.exe")) ||
                       File.Exists(Path.Combine(txtIceHome.Text, "cpp", "bin", "slice2cpp.exe")))
                    {
                        updateIceHome();
                        lblInfo.Visible = true;
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                }

                lblInfo.Text = "Invalid Ice SDK Location";
                lblInfo.Visible = true;
                return false;
            }
        }

        private void txtIceHome_KeyPress(object sender, KeyPressEventArgs e)
        {
            validateIceHome();
        }
    }
}
