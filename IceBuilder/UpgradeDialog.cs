using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public partial class UpgradeDialog : Form
    {
        public UpgradeDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Clicked(object sender, EventArgs e)
        {
            List<IVsProject> selected = SelectedProjets;
            UpgradeDialogProgress proggressDialog = new UpgradeDialogProgress(selected.Count);
            ProjectConverter.Upgrade(selected, proggressDialog);
            proggressDialog.StartPosition = FormStartPosition.CenterParent;
            proggressDialog.ShowDialog(this);
            Close();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            Close();
        }

        Dictionary<String, IVsProject> _projects;
        public Dictionary<String, IVsProject> Projects
        {
            get
            {
                return _projects;
            }
            set
            {
                _projects = value;
                Values = value.Keys.ToList();
            }
        }

        public List<String> Values
        {
            set
            {
                projectList.Items.Clear();
                foreach(String v in value)
                {
                    projectList.Items.Add(v);
                    projectList.SetItemCheckState(projectList.Items.Count - 1, CheckState.Checked);
                }
            }
        }

        public List<IVsProject> SelectedProjets
        {
            get
            {
                List<IVsProject> values = new List<IVsProject>();
                foreach (object o in projectList.CheckedItems)
                {
                    values.Add(Projects[o.ToString()]);
                }
                return values;
            
            }
        }
    }
}
