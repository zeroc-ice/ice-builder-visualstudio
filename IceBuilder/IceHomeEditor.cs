// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
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

        public string IceHome
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

        public bool SetIceHome(string path)
        {
            if(!string.IsNullOrEmpty(path))
            {
                if(string.IsNullOrEmpty(path) ||
                    File.Exists(Path.Combine(path, "cpp", "config", "ice.props")) ||
                    File.Exists(Path.Combine(path, "config", "ice.props")) ||
                    File.Exists(Path.Combine(path, "config", "icebuilder.props")))
                {
                    txtIceHome.Text = path;
                    return true;
                }
                else
                {
                    string reason = string.Empty;
                    //
                    // With Ice >= 3.7 the binary distribution is a collection of nuget packages,
                    // look for the property sheet in the build directory
                    //
                    try
                    {
                        foreach(string d in Directory.EnumerateDirectories(path))
                        {
                            string props = Path.Combine(d, "build", string.Format("{0}.props", Path.GetFileName(d)));
                            if(File.Exists(props))
                            {
                                try
                                {
                                    Microsoft.Build.Evaluation.Project p = new Microsoft.Build.Evaluation.Project(props);
                                    try
                                    {
                                        if(!string.IsNullOrEmpty(p.GetPropertyValue(Package.IceHomeValue)))
                                        {
                                            txtIceHome.Text = path;
                                            return true;
                                        }
                                    }
                                    finally
                                    {
                                        ICollection<Microsoft.Build.Evaluation.Project> projects =
                                            Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(props);
                                        if(projects.Count > 0)
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
                        string.Format("Invalid Ice home directory:\r\n\"{0}\"\r\n{1}", path, reason);
                    return false;
                }
            }
            return true;
        }

        private void btnIceHome_Click(object sender, EventArgs e)
        {
            string selectedPath = UIUtil.BrowserFolderDialog(Handle, "Select Folder",
                string.IsNullOrEmpty(txtIceHome.Text) ?
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
