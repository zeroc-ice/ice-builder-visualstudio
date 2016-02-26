// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using System.Windows.Forms;

namespace IceBuilder
{
    class UIUtil
    {
        public static void ShowErrorDialog(String title, String message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                0);
        }

        //
        // Open the Visual Studio native dialog for selecting a directory
        //
        public static String BrowserFolderDialog(IntPtr owner, String title, String initialDirectory)
        {
            IVsUIShell shell = Package.Instance.UIShell;

            VSBROWSEINFOW[] browseInfo = new VSBROWSEINFOW[1];
            browseInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSBROWSEINFOW));
            browseInfo[0].pwzInitialDir = initialDirectory;
            browseInfo[0].pwzDlgTitle = title;
            browseInfo[0].hwndOwner = owner;
            browseInfo[0].nMaxDirName = 260;
            IntPtr pDirName = Marshal.AllocCoTaskMem(520);
            browseInfo[0].pwzDirName = pDirName;

            try
            {
                int hr = shell.GetDirectoryViaBrowseDlg(browseInfo);
                if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED)
                {
                    return String.Empty;
                }
                ErrorHandler.ThrowOnFailure(hr);
                return Marshal.PtrToStringAuto(browseInfo[0].pwzDirName);
            }
            finally
            {
                if (pDirName != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pDirName);
                }
            }
        }
    }
}
