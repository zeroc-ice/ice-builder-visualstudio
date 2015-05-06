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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

using System.Runtime.InteropServices;

namespace IceBuilder
{
    public partial class CSharpConfigurationView : UserControl
    {
        private PropertyPage Page
        {
            get;
            set;
        }

        private String AssembliesDir
        {
            get;
            set;
        }

        public CSharpConfigurationView(PropertyPage page)
        {
            Page = page;
            InitializeComponent();
            includeDirectories.PropertyPage = Page;
            AssembliesDir = Package.Instance.GetAssembliesDir(Package.Instance.GetIceHome());
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

        public readonly uint PageStatusDirty = 0x1;
        public readonly uint PageStatusClean = 0x4;
        public bool _dirty;
        public Boolean Dirty
        {
            get 
            {
                return _dirty;
            }
            set
            {
                _dirty = value;
                if (Page.PageSite != null)
                {
                    Page.PageSite.OnStatusChange(value ? PageStatusDirty : PageStatusClean);
                }
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

        public CheckState Underscores
        {
            get
            {
                return chkUnderscores.CheckState;
            }
            set
            {
                chkUnderscores.CheckState = value;
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
            }
        }

        public void LoadReferencedAssemblies()
        {
            if (!String.IsNullOrEmpty(AssembliesDir))
            {
                try
                {
                    String[] assemblies = Directory.GetFiles(AssembliesDir, "*.dll");
                    foreach (String assembly in assemblies)
                    {
                        String name = Path.GetFileNameWithoutExtension(assembly);
                        referencedAssemblies.Items.Add(name);
                        if (ProjectUtil.HasAssemblyReference(Page.Project, name))
                        {
                            referencedAssemblies.SetItemCheckState(referencedAssemblies.Items.Count - 1, CheckState.Checked);
                        }
                    }
                }
                catch (IOException)
                {
                }
            }
        }

        public List<String> Assemblies
        {
            get
            {
                List<String> assemblies = new List<String>();
                foreach (object o in referencedAssemblies.Items)
                {
                    assemblies.Add(o.ToString());
                }
                return assemblies;
            }
        }

        public List<String> ReferencedAssemblies
        {
            get
            {
                List<String> selected = new List<String>();
                foreach (object o in referencedAssemblies.CheckedItems)
                {
                    selected.Add(o.ToString());
                }
                return selected;
            }
        }

        public IncludeDirectories IncludeDirectories
        {
            get
            {
                return includeDirectories;
            }
        }

        private void btnOutputDirectoryBrowse_Click(object sender, EventArgs e)
        {
            String projectDir = Path.GetFullPath(Path.GetDirectoryName(Page.Project.FullName));
            String selectedPath = UIUtil.BrowserFolderDialog(Handle, "Output Directory", projectDir);
            if (!String.IsNullOrEmpty(selectedPath))
            {
                selectedPath = FileUtil.RelativePath(projectDir, selectedPath);
                OutputDir = String.IsNullOrEmpty(selectedPath) ? "." : selectedPath;
                if(!txtOutputDir.Text.Equals(Page.Settings.OutputDir))
                {
                    Dirty = true;
                }
            }
        }

        private void OutputDirectory_Leave(object sender, EventArgs e)
        {
            if (!txtOutputDir.Text.Equals(Page.Settings.OutputDir))
            {
                Dirty = true;
            }
        }

        private void AllowIcePrefix_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkIce.Checked != Page.Settings.AllowIcePrefix)
            {
                Dirty = true;
            }
        }

        private void Underscore_Changed(object sender, System.EventArgs e)
        {
            if (chkUnderscores.Checked != Page.Settings.Underscore)
            {
                Dirty = true;
            }
        }

        private void Stream_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkStreaming.Checked != Page.Settings.Stream)
            {
                Dirty = true;
            }
        }

        private void Checksum_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkChecksum.Checked != Page.Settings.Checksum)
            {
                Dirty = true;
            }
        }

        private void Tie_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkTie.Checked != Page.Settings.Tie)
            {
                Dirty = true;
            }
        }

        private void AdditionalOptions_Leave(object sender, System.EventArgs e)
        {
            if (!txtAdditionalOptions.Text.Equals(Page.Settings.AdditionalOptions))
            {
                Dirty = true;
            }
        }

        private void ReferencedAssemblies_ItemChecked(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            Dirty = true;
        }
    }
}
