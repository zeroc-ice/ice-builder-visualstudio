// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Windows.Forms;

namespace IceBuilder
{
    public interface UpgradeProgressCallback
    {
        bool Canceled
        {
            get; set;
        }
        void Finished();
        void ReportProgress(string project, int index);
    }

    public partial class UpgradeDialogProgress : Form, UpgradeProgressCallback
    {
        public UpgradeDialogProgress(int total)
        {
            InitializeComponent();
            _canceled = false;
            ProgressBar.Maximum = total;
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            Canceled = true;
        }

        private bool _canceled;
        public bool Canceled
        {
            get
            {
                lock(_lock)
                {
                    return _canceled;
                }
            }
            set
            {
                lock(_lock)
                {
                    _canceled = value;
                }
            }
        }

        public void ReportProgress(string project, int index)
        {
            InfoLabel.Text = string.Format("Upgrading project: {0}", project);
            ProgressBar.Value = index;
        }

        public void Finished()
        {
            Close();
        }

        private object _lock = new object();

        private void cancelButton_Click(object sender, EventArgs e)
        {

        }
    }
}
