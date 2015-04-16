using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceCustomProject
{
    [Guid("1E2800FE-37C5-4FD3-BC2E-969342EE08AF")]
    public class PropertyPage : IPropertyPage2, IPropertyPage
    {
        private CSharpConfigurationView _view;
        public CSharpConfigurationView ConfigurationView
        {
            get
            {
                if (_view == null)
                {
                    _view = new CSharpConfigurationView(this);
                }
                return _view;
            }
            set
            {
                _view = value;
            }
        }

        private List<ProjectConfiguration> _configs = null;

        public String GetProperty(String name)
        {
            if (_configs != null && _configs.Count > 0)
            {
                return _configs[0].GetProperty(name);
            }
            return "";
        }

        #region IPropertyPage2 methods

        public void Activate(IntPtr parentHandle, RECT[] pRect, int modal)
        {
            RECT rect = pRect[0];
            this.ConfigurationView.Initialize(Control.FromHandle(parentHandle),
                                              Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom));
        }

        public void Apply()
        {
            if(_configs != null)
            {
                foreach (ProjectConfiguration config in _configs)
                {
                    if(!ConfigurationView.OutputDirMultipleValues)
                    {
                        config.Settings.OutputDir = ConfigurationView.OutputDir;
                    }

                    if (ConfigurationView.Ice != CheckState.Indeterminate)
                    {
                        config.Settings.Ice = ConfigurationView.Ice == CheckState.Checked ? true : false;
                    }

                    if (ConfigurationView.Checksum != CheckState.Indeterminate)
                    {
                        config.Settings.Checksum = ConfigurationView.Checksum == CheckState.Checked ? true : false;
                    }

                    if (ConfigurationView.Streaming != CheckState.Indeterminate)
                    {
                        config.Settings.Streaming = ConfigurationView.Streaming == CheckState.Checked ? true : false;
                    }

                    if (ConfigurationView.Tie != CheckState.Indeterminate)
                    {
                        config.Settings.Tie = ConfigurationView.Tie == CheckState.Checked ? true : false;
                    }

                    if (ConfigurationView.Underscores != CheckState.Indeterminate)
                    {
                        config.Settings.Underscores = ConfigurationView.Underscores == CheckState.Checked ? true : false;
                    }

                    if (!ConfigurationView.AdditionalIncludeDirectories.MultipleValues)
                    {
                        config.Settings.AdditionalIncludeDirectories = 
                            String.Join(";", ConfigurationView.AdditionalIncludeDirectories.Values);
                    }

                    if(!ConfigurationView.AdditionalOptionsMultipleValues)
                    {
                        config.Settings.AdditionalOptions = ConfigurationView.AdditionalOptions;
                    }

                    config.Settings.Save();
                }
                ConfigurationView.NeedSave = false;
            }
        }

        public void Deactivate()
        {
            if(_view != null)
            {
                _view.Dispose();
                _view = null;
            }
        }

        public void EditProperty(int DISPID)
        {
        }

        public void GetPageInfo(PROPPAGEINFO[] pageInfo)
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

        public void Help(String pszHelpDir)
        { 
        }

        public int IsPageDirty()
        {
            return ConfigurationView.NeedSave ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        public void Move(RECT[] pRect)
        {
            Rectangle rect = Rectangle.FromLTRB(pRect[0].left, pRect[0].top, pRect[0].right, pRect[0].bottom);
            ConfigurationView.Location = new Point(rect.X, rect.Y);
            ConfigurationView.Size = new Size(rect.Width, rect.Height);
        }

        public void SetObjects(uint cObjects, Object[] objects)
        {
            if (objects != null)
            {
                _configs = new List<ProjectConfiguration>();
                foreach(object o in objects)
                {
                    IVsCfg config = o as IVsCfg;
                    if(config != null)
                    {
                        _configs.Add(ProjectConfiguration.getProjectConfiguration(config));
                    }
                }

                for(int i = 0; i < _configs.Count; ++i)
                {
                    ProjectConfiguration config = _configs[i];

                    if (i == 0)
                    {
                        ConfigurationView.OutputDir = config.Settings.OutputDir;
                        ConfigurationView.Ice = config.Settings.Ice ? CheckState.Checked : CheckState.Unchecked;
                        ConfigurationView.Checksum = config.Settings.Checksum ? CheckState.Checked : CheckState.Unchecked;
                        ConfigurationView.Streaming = config.Settings.Streaming ? CheckState.Checked : CheckState.Unchecked;
                        ConfigurationView.Tie = config.Settings.Tie ? CheckState.Checked : CheckState.Unchecked;
                        ConfigurationView.Underscores = config.Settings.Underscores ? CheckState.Checked : CheckState.Unchecked;
                        ConfigurationView.AdditionalIncludeDirectories.Values = new List<String>(
                            config.Settings.AdditionalIncludeDirectories.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries));
                        ConfigurationView.AdditionalOptions = config.Settings.AdditionalOptions;
                    }
                    else
                    {
                        if(!ConfigurationView.OutputDir.Equals(config.Settings.OutputDir))
                        {
                            ConfigurationView.OutputDir = "";
                            ConfigurationView.OutputDirMultipleValues = true;
                        }

                        if (ConfigurationView.Ice != CheckState.Indeterminate &&
                           ConfigurationView.Ice != (config.Settings.Ice ? CheckState.Checked : CheckState.Unchecked))
                        {
                            ConfigurationView.Ice = CheckState.Indeterminate;
                        }

                        if (ConfigurationView.Checksum != CheckState.Indeterminate &&
                           ConfigurationView.Checksum != (config.Settings.Checksum ? CheckState.Checked : CheckState.Unchecked))
                        {
                            ConfigurationView.Checksum = CheckState.Indeterminate;
                        }

                        if (ConfigurationView.Streaming != CheckState.Indeterminate &&
                           ConfigurationView.Streaming != (config.Settings.Streaming ? CheckState.Checked : CheckState.Unchecked))
                        {
                            ConfigurationView.Streaming = CheckState.Indeterminate;
                        }

                        if (ConfigurationView.Tie != CheckState.Indeterminate &&
                           ConfigurationView.Tie != (config.Settings.Tie ? CheckState.Checked : CheckState.Unchecked))
                        {
                            ConfigurationView.Tie = CheckState.Indeterminate;
                        }

                        if (ConfigurationView.Underscores != CheckState.Indeterminate &&
                           ConfigurationView.Underscores != (config.Settings.Underscores ? CheckState.Checked : CheckState.Unchecked))
                        {
                            ConfigurationView.Underscores = CheckState.Indeterminate;
                        }

                        if (!String.Join(";", ConfigurationView.AdditionalIncludeDirectories.Values).Equals(
                            config.Settings.AdditionalIncludeDirectories))
                        {
                            ConfigurationView.AdditionalIncludeDirectories.Values = new List<String>();
                            ConfigurationView.AdditionalIncludeDirectories.MultipleValues = true;
                        }

                        if (!ConfigurationView.AdditionalOptions.Equals(config.Settings.AdditionalOptions))
                        {
                            ConfigurationView.AdditionalOptions = "";
                            ConfigurationView.AdditionalOptionsMultipleValues = true;
                        }
                    }
                }
                ConfigurationView.NeedSave = false;
            }
        }

        public void SetPageSite(IPropertyPageSite pageSite)
        {
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
            Message message = Message.Create(pMsg[0].hwnd, (int)pMsg[0].message, pMsg[0].wParam, pMsg[0].lParam);
            int hr = ConfigurationView.ProcessAccelerator(ref message);
            pMsg[0].lParam = message.LParam;
            pMsg[0].wParam = message.WParam;
            return hr;
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
