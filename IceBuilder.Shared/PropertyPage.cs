// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IceBuilder
{

    [Guid(PropertyPageGUID)]
    public class PropertyPage : IPropertyPage2, IPropertyPage, IDisposable
    {
        public const string PropertyPageGUID = "1E2800FE-37C5-4FD3-BC2E-969342EE08AF";

        public CSharpConfigurationView ConfigurationView { get; private set; }

        public IDisposable ProjectSubscription { get; private set; }

        public PropertyPage() =>
            ConfigurationView = new CSharpConfigurationView(this);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (ConfigurationView != null)
            {
                ConfigurationView.Dispose();
                ConfigurationView = null;
            }
        }

        public void Activate(IntPtr parentHandle, RECT[] pRect, int modal)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                RECT rect = pRect[0];
                ConfigurationView.Initialize(Control.FromHandle(parentHandle),
                                             Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom));
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        }

        public void Apply()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Settings.OutputDir = ConfigurationView.OutputDir;
                Settings.IncludeDirectories = ConfigurationView.IncludeDirectories;
                Settings.AdditionalOptions = ConfigurationView.AdditionalOptions;
                Settings.Save();
                ConfigurationView.Dirty = false;
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        }

        public void Deactivate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (ConfigurationView != null)
                {
                    ConfigurationView.Dispose();
                    ConfigurationView = null;
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        }

        public void EditProperty(int DISPID)
        {
        }

        public void GetPageInfo(PROPPAGEINFO[] pageInfo)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                PROPPAGEINFO proppageinfo;
                proppageinfo.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
                proppageinfo.dwHelpContext = 0;
                proppageinfo.pszDocString = null;
                proppageinfo.pszHelpFile = null;
                proppageinfo.pszTitle = "Ice Builder";
                proppageinfo.SIZE.cx = ConfigurationView.Size.Width;
                proppageinfo.SIZE.cy = ConfigurationView.Size.Height;
                pageInfo[0] = proppageinfo;
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        }

        public void Help(string pszHelpDir)
        {
        }

        public int IsPageDirty() =>
            ConfigurationView.Dirty ? VSConstants.S_OK : VSConstants.S_FALSE;

        public void Move(RECT[] pRect)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Rectangle rect = Rectangle.FromLTRB(pRect[0].left, pRect[0].top, pRect[0].right, pRect[0].bottom);
                ConfigurationView.Location = new Point(rect.X, rect.Y);
                ConfigurationView.Size = new Size(rect.Width, rect.Height);
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        }

        public ProjectSettigns Settings { get; private set; }

        public IVsProject Project { get; private set; }

        public void SetObjects(uint cObjects, object[] objects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (objects != null && cObjects > 0)
                {
                    if (objects[0] is IVsBrowseObject browse)
                    {
                        browse.GetProjectItem(out IVsHierarchy hier, out uint id);
                        Project = hier as IVsProject;
                        if (Project != null)
                        {
                            Settings = new ProjectSettigns(Project);
                            Settings.Load();
                            ConfigurationView.LoadSettigns(Settings);
                            ProjectSubscription = Project.OnProjectUpdate(() =>
                                {
                                    if (!ConfigurationView.Dirty)
                                    {
                                        Settings.Load();
                                        ConfigurationView.LoadSettigns(Settings);
                                    }
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        }

        public IPropertyPageSite PageSite { get; private set; }

        public void SetPageSite(IPropertyPageSite site) => PageSite = site;

        public const int SW_SHOW = 5;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_HIDE = 0;

        public void Show(uint show)
        {
            switch (show)
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
            Message message = Message.Create(pMsg[0].hwnd, (int)pMsg[0].message, pMsg[0].wParam, pMsg[0].lParam);
            int hr = ConfigurationView.ProcessAccelerator(ref message);
            pMsg[0].lParam = message.LParam;
            pMsg[0].wParam = message.WParam;
            return hr;
        }

        int IPropertyPage.Apply()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Apply();
            return VSConstants.S_OK;
        }
    }
}
