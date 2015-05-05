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
    public partial class UpgradeDialog : Form
    {
        public UpgradeDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Clicked(object sender, EventArgs e)
        {
            List<EnvDTE.Project> selected = SelectedProjets;
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

        Dictionary<String, EnvDTE.Project> _projects;
        public Dictionary<String, EnvDTE.Project> Projects
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

        public List<EnvDTE.Project> SelectedProjets
        {
            get
            {
                List<EnvDTE.Project> values = new List<EnvDTE.Project>();
                foreach (object o in projectList.CheckedItems)
                {
                    values.Add(Projects[o.ToString()]);
                }
                return values;
            
            }
        }
    }
}
