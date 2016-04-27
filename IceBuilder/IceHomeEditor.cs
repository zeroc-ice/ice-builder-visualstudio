// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
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

        public bool SetIceHome(String path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                if (String.IsNullOrEmpty(path) ||
                    File.Exists(Path.Combine(path, "bin", "slice2cpp.exe")) ||
                    File.Exists(Path.Combine(path, "cpp", "bin", "slice2cpp.exe")) ||
                    File.Exists(Path.Combine(path, "cpp", "config", "Ice.props")) ||
                    File.Exists(Path.Combine(path, "config", "Ice.props")) ||
                    File.Exists(Path.Combine(path, "build", "native", "Ice.props")))
                {
                    txtIceHome.Text = path;
                    return true;
                }
                else
                {
                    String reason = String.Empty;
                    //
                    // With Ice >= 3.7 the binary distribution is a collection of nuget packages,
                    // look for the property sheet in the build directory
                    //
                    try
                    {
                        foreach (String d in Directory.EnumerateDirectories(path))
                        {
                            String props = Path.Combine(d, "build", String.Format("{0}.props", Path.GetFileName(d)));
                            if (File.Exists(props))
                            {
                                try
                                {
                                    Microsoft.Build.Evaluation.Project p = new Microsoft.Build.Evaluation.Project(props);
                                    try
                                    {
                                        if (!String.IsNullOrEmpty(p.GetPropertyValue(Package.IceHomeValue)))
                                        {
                                            txtIceHome.Text = path;
                                            return true;
                                        }
                                    }
                                    finally
                                    {
                                        ICollection<Microsoft.Build.Evaluation.Project> projects =
                                            Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(props);
                                        if (projects.Count > 0)
                                        {
                                            Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(p);
                                        }
                                    }                                    
                                }
                                catch(Exception)
                                {
                                }
                            }
                        }
                    }
                    catch(DirectoryNotFoundException)
                    {
                        reason = "path is invalid, such as referring to an unmapped drive.";
                    }
                    catch(System.Security.SecurityException)
                    {
                        reason = "The caller does not have the required permission.";
                    }
                    catch(UnauthorizedAccessException)
                    {
                        reason = "The caller does not have the required permission.";
                    }
                    catch(IOException)
                    {
                        reason = "path is a file name.";
                    }

                    lblInfo.Text =
                        String.Format("Invalid Ice home directory:\r\n\"{0}\"\r\n{1}", path, reason);
                    return false;
                }
            }
            return true;
        }

        private void btnIceHome_Click(object sender, EventArgs e)
        {
            String selectedPath = UIUtil.BrowserFolderDialog(Handle, "Select Folder",
                String.IsNullOrEmpty(txtIceHome.Text) ? 
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : txtIceHome.Text);

            lblInfo.Text = "";

            SetIceHome(selectedPath);
        }

        public bool AutoBuilding
        {
            set
            {
                autoBuild.Checked = value;
            }
            get
            {
                return autoBuild.Checked;
            }
        }
    }
}
