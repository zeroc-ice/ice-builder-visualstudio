// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using System.ComponentModel;

namespace IceBuilder
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")]
    [Category("Projects and Solutions")]
    public class IceOptionsPage : DialogPage
    {
        public String IceHome
        {
            get;
            set;
        }

        IceHomeEditor Editor
        {
            get;
            set;
        }

        public IceOptionsPage()
        {
            Editor = new IceHomeEditor();
        }

        protected override IWin32Window Window
        {
            get
            {
                Editor.optionsPage = this;
                Editor.Initialize();
                return Editor; 
            }
        }

        public override void SaveSettingsToStorage()
        {
            if (!Package.Instance.GetIceHome().Equals(IceHome))
            {
                Package.Instance.SetIceHome(IceHome);
            }
        }

        public override void LoadSettingsFromStorage()
        {
            IceHome = Package.Instance.GetIceHome();
            Editor.ClearErrors();
        }
    }
}
