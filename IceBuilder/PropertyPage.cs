// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    [Guid("1E2800FE-37C5-4FD3-BC2E-969342EE08AF")]
    public class PropertyPage : IPropertyPage2, IPropertyPage, IDisposable
    {
        private CSharpConfigurationView _view;
        public CSharpConfigurationView ConfigurationView
        {
            get;
            set;
        }

        public PropertyPage()
        {
            ConfigurationView = new CSharpConfigurationView(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(ConfigurationView != null)
            {
                ConfigurationView.Dispose();
                ConfigurationView = null;
            }
        }

        #region IPropertyPage2 methods

        public void Activate(IntPtr parentHandle, RECT[] pRect, int modal)
        {
            try
            {
                RECT rect = pRect[0];
                this.ConfigurationView.Initialize(Control.FromHandle(parentHandle),
                                                  Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom));
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        public void Apply()
        {
            try
            {
                Settings.OutputDir = ConfigurationView.OutputDir;
                Settings.AllowIcePrefix = ConfigurationView.Ice == CheckState.Checked ? true : false;
                Settings.Checksum = ConfigurationView.Checksum == CheckState.Checked ? true : false;
                Settings.Stream = ConfigurationView.Streaming == CheckState.Checked ? true : false;
                Settings.Tie = ConfigurationView.Tie == CheckState.Checked ? true : false;
                Settings.Underscore = ConfigurationView.Underscores == CheckState.Checked ? true : false;
                Settings.IncludeDirectories = String.Join(";", ConfigurationView.IncludeDirectories.Values);
                Settings.AdditionalOptions = ConfigurationView.AdditionalOptions;

                List<String> referencedAssemblies = ConfigurationView.ReferencedAssemblies;
                foreach (String assembly in ConfigurationView.Assemblies)
                {
                    if (DTEUtil.HasAssemblyReference(Project, assembly))
                    {
                        if (!referencedAssemblies.Contains(assembly))
                        {
                            DTEUtil.RemoveAssemblyReference(Project, assembly);
                        }
                    }
                    else
                    {
                        if (referencedAssemblies.Contains(assembly))
                        {
                            DTEUtil.AddAssemblyReference(Project, assembly);
                        }
                    }
                }
                Settings.Save();
                ConfigurationView.Dirty = false;
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        public void Deactivate()
        {
            try
            {
                if(_view != null)
                {
                    _view.Dispose();
                    _view = null;
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        public void EditProperty(int DISPID)
        {
        }

        public void GetPageInfo(PROPPAGEINFO[] pageInfo)
        {
            try
            {
                PROPPAGEINFO proppageinfo;
                proppageinfo.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
                proppageinfo.dwHelpContext = 0;
                proppageinfo.pszDocString = null;
                proppageinfo.pszHelpFile = null;
                proppageinfo.pszTitle = "Ice Builder";
                proppageinfo.SIZE.cx = this.ConfigurationView.Size.Width;
                proppageinfo.SIZE.cy = this.ConfigurationView.Size.Height;
                pageInfo[0] = proppageinfo;
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        public void Help(String pszHelpDir)
        { 
        }

        public int IsPageDirty()
        {
            try
            {
                return ConfigurationView.Dirty ? VSConstants.S_OK : VSConstants.S_FALSE;
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        public void Move(RECT[] pRect)
        {
            try
            {
                Rectangle rect = Rectangle.FromLTRB(pRect[0].left, pRect[0].top, pRect[0].right, pRect[0].bottom);
                ConfigurationView.Location = new Point(rect.X, rect.Y);
                ConfigurationView.Size = new Size(rect.Width, rect.Height);
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        public ProjectSettigns Settings
        {
            get;
            private set;
        }

        public EnvDTE.Project Project
        {
            get;
            private set;
        }

        public void SetObjects(uint cObjects, Object[] objects)
        {
            try
            {
                if (objects != null && cObjects > 0)
                {
                    IVsBrowseObject browse = objects[0] as IVsBrowseObject;
                    if (browse != null)
                    {
                        IVsHierarchy hier;
                        uint id;
                        browse.GetProjectItem(out hier, out id);
                        Project = DTEUtil.GetProject(hier);
                        if (Project != null)
                        {
                            Settings = new ProjectSettigns(Project);
                            Settings.Load();
                            ConfigurationView.OutputDir = Settings.OutputDir;
                            ConfigurationView.Ice = Settings.AllowIcePrefix ? CheckState.Checked : CheckState.Unchecked;
                            ConfigurationView.Checksum = Settings.Checksum ? CheckState.Checked : CheckState.Unchecked;
                            ConfigurationView.Streaming = Settings.Stream ? CheckState.Checked : CheckState.Unchecked;
                            ConfigurationView.Tie = Settings.Tie ? CheckState.Checked : CheckState.Unchecked;
                            ConfigurationView.Underscores = Settings.Underscore ? CheckState.Checked : CheckState.Unchecked;
                            ConfigurationView.IncludeDirectories.Values = new List<String>(
                                Settings.IncludeDirectories.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                            ConfigurationView.AdditionalOptions = Settings.AdditionalOptions;
                            ConfigurationView.LoadReferencedAssemblies();
                            ConfigurationView.Dirty = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        public IPropertyPageSite PageSite
        {
            get;
            private set;
        }

        public void SetPageSite(IPropertyPageSite site)
        {
            PageSite = site;
        }

        public const int SW_SHOW = 5;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_HIDE = 0;

        public void Show(uint show)
        {
            switch(show)
            {
                case SW_HIDE:
                {
                    ConfigurationView.Hide();
                    break;
                }
                case SW_SHOW:
                case SW_SHOWNORMAL:
                {
                    ConfigurationView.Show();
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        public int TranslateAccelerator(MSG[] pMsg)
        {
            try
            {
                Message message = Message.Create(pMsg[0].hwnd, (int)pMsg[0].message, pMsg[0].wParam, pMsg[0].lParam);
                int hr = ConfigurationView.ProcessAccelerator(ref message);
                pMsg[0].lParam = message.LParam;
                pMsg[0].wParam = message.WParam;
                return hr;
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        #endregion

        #region IPropertyPage methods
        int IPropertyPage.Apply()
        {
            this.Apply();
            return VSConstants.S_OK;
        }
        #endregion
    }
}
