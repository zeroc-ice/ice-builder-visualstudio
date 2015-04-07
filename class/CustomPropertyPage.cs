using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace IceBuilder
{
    [Guid("5DE47AE6-13DC-44D5-938B-6DBC7F95699B")]
    class CustomPropertyPage : IPropertyPage2, IPropertyPage
    {

        /// <summary>
        /// Create the dialog box window for the property page.
        /// The dialog box is created without a frame, caption, or system menu/controls. 
        /// </summary>
        /// <param name="hWndParent">
        /// The window handle of the parent of the dialog box that is being created.
        /// </param>
        /// <param name="pRect">
        /// The RECT structure containing the positioning information for the dialog box. 
        /// This method must create its dialog box with the placement and dimensions
        /// described by this structure.
        /// </param>
        /// <param name="bModal">
        /// Indicates whether the dialog box frame is modal (TRUE) or modeless (FALSE).
        /// </param>
        public void Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            if ((pRect == null) || (pRect.Length == 0))
            {
                throw new ArgumentNullException("pRect");
            }
            Control parentControl = Control.FromHandle(hWndParent);
            RECT rect = pRect[0];
            this.MyPageView.Initialize(parentControl,
                Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom));
        }

        /// <summary>
        /// Destroy the window created in IPropertyPage::Activate.
        /// </summary>
        public void Deactivate()
        {
            if (this.myPageView != null)
            {
                this.myPageView.Dispose();
                this.myPageView = null;
            }
        }

        /// <summary>
        /// Retrieve information about the property page.
        /// </summary>
        /// <param name="pPageInfo"></param>
        public void GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            PROPPAGEINFO proppageinfo;
            if ((pPageInfo == null) || (pPageInfo.Length == 0))
            {
                throw new ArgumentNullException("pPageInfo");
            }
            proppageinfo.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            proppageinfo.dwHelpContext = 0;
            proppageinfo.pszDocString = null;
            proppageinfo.pszHelpFile = null;
            proppageinfo.pszTitle = "Ice Builder";
            proppageinfo.SIZE.cx = this.MyPageView.ViewSize.Width;
            proppageinfo.SIZE.cy = this.MyPageView.ViewSize.Height;
            pPageInfo[0] = proppageinfo;
        }
    }
}
