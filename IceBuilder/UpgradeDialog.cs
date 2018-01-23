// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public partial class UpgradeDialog : Form
    {
        public UpgradeDialog()
        {
            InitializeComponent();
            description.Rtf = File.ReadAllText(Path.Combine(Package.ResourcesDirectory, "upgrade.rtf"));
        }

        private void OKButton_Clicked(object sender, EventArgs e)
        {
            Hide();
            UpgradeDialogProgress proggressDialog = new UpgradeDialogProgress(Projects.Count);
            ProjectConverter.Upgrade(Projects, proggressDialog);
            proggressDialog.StartPosition = FormStartPosition.CenterParent;
            proggressDialog.ShowDialog(this);
            Close();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            Close();
        }

        public Dictionary<string, IVsProject> Projects
        {
            get;
            set;
        }

        private void description_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
