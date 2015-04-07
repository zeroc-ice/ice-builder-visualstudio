using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio;

namespace IceCustomProject
{
    public partial class CSharpConfigurationView : UserControl
    {
        public CSharpConfigurationView()
        {
            InitializeComponent();
        }


        public virtual void Initialize(Control parent, Rectangle rect)
        {
            base.SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
            base.Parent = parent;
        }

        public int ProcessAccelerator(ref Message keyboardMessage)
        {
            if (Control.FromHandle(keyboardMessage.HWnd).PreProcessMessage(ref keyboardMessage))
            {
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        public bool _neeSave;
        public Boolean NeedSave
        {
            get 
            {
                return _neeSave;
            }
            set
            {
                _neeSave = value;
            }
        }

        private bool _outputDirMultipleValues;
        public Boolean OutputDirMultipleValues
        {
            get
            {
                return _outputDirMultipleValues;
            }
            set
            {
                _outputDirMultipleValues = value;
            }
        }

        public String OutputDir
        {
            get
            {
                return txtOutputDir.Text;
            }
            set
            {
                txtOutputDir.Text = value;
                OutputDirMultipleValues = false;
            }
        }

        public CheckState Ice
        {
            get
            {
                return chkIce.CheckState;
            }
            set
            {
                chkIce.CheckState = value;
            }
        }

        public CheckState Checksum
        {
            get
            {
                return chkChecksum.CheckState;
            }
            set
            {
                chkChecksum.CheckState = value;
            }
        }

        public CheckState Streaming
        {
            get
            {
                return chkStreaming.CheckState;
            }
            set
            {
                chkStreaming.CheckState = value;
            }
        }

        public CheckState Tie
        {
            get
            {
                return chkTie.CheckState;
            }
            set
            {
                chkTie.CheckState = value;
            }
        }

        public String AdditionalOptions
        {
            get
            {
                return txtAdditionalOptions.Text;
            }
            set
            {
                txtAdditionalOptions.Text = value;
                AdditionalOptionsMultipleValues = false;
            }
        }

        private bool _additionalOptionsMultipleValues;
        public Boolean AdditionalOptionsMultipleValues
        {
            get
            {
                return _additionalOptionsMultipleValues;
            }
            set
            {
                _additionalOptionsMultipleValues = value;
            }
        }

        private void txtOutputDir_TextChanged(object sender, EventArgs e)
        {
            _neeSave = true;
        }

        void chkTie_CheckedChanged(object sender, System.EventArgs e)
        {
            _neeSave = true;
        }

        void chkChecksum_CheckedChanged(object sender, System.EventArgs e)
        {
            _neeSave = true;
        }

        void chkStreaming_CheckedChanged(object sender, System.EventArgs e)
        {
            _neeSave = true;
        }

        void chkIce_CheckedChanged(object sender, System.EventArgs e)
        {
            _neeSave = true;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void lblTracingLevel_Click(object sender, EventArgs e)
        {

        }

        private void btnOutputDirectoryBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select Output Path";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtOutputDir.Text = dialog.SelectedPath;
            }
        }

        private void CSharpConfigurationView_Load(object sender, EventArgs e)
        {

        }

        private void groupBoxGeneral_Enter(object sender, EventArgs e)
        {

        }
    }
}
