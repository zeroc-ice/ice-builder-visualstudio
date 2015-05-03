using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IceBuilder
{

    public interface UpgradeProgressCallback
    {
        bool Canceled { get; set; }
        void Finished();
        void ReportProgress(String project, int index);
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
                lock (_lock)
                {
                    return _canceled;
                }
            }
            set
            {
                lock (_lock)
                {
                    _canceled = value;
                }
            }
        }

        public void ReportProgress(String project, int index)
        {
            InfoLabel.Text = String.Format("Upgrading project: {0}", project);
            ProgressBar.Value = index;
        }

        public void Finished()
        {
            Close();
        }

        private Object _lock = new Object();
    }
}
