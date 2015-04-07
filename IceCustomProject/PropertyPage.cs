using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

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
                    _view = new CSharpConfigurationView();
                }
                return _view;
            }
            set
            {
                _view = value;
            }
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
            return VSConstants.S_OK;
        }

        public void Move(RECT[] pRect)
        {
            Rectangle rect = Rectangle.FromLTRB(pRect[0].left, pRect[0].top, pRect[0].right, pRect[0].bottom);
            ConfigurationView.Location = new Point(rect.X, rect.Y);
            ConfigurationView.Size = new Size(rect.Width, rect.Height);
        }

        public void SetObjects(uint cObjects, Object[] ppunk)
        { 
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

        /// <summary>
        /// Use a site object with this interface to set up communications between the
        /// property frame and the property page object.
        /// http://msdn.microsoft.com/en-us/library/ms690583(VS.85).aspx
        /// </summary>
        public IPropertyPageSite PropertyPageSite
        {
            get
            {
                return _propertyPageSite;
            }
            set
            {
                _propertyPageSite = value;
            }
        }

        private IPropertyPageSite _propertyPageSite;
    }
}
