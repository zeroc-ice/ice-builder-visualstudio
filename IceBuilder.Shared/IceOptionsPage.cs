// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IceBuilder
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")]
    [Category("Projects and Solutions")]
    public class IceOptionsPage : DialogPage
    {
        IceHomeEditor Editor { get; set; }

        public IceOptionsPage() => Editor = new IceHomeEditor();

        protected override IWin32Window Window
        {
            get
            {
                Editor.optionsPage = this;
                return Editor;
            }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Package.Instance.SetAutoBuilding(Editor.AutoBuilding);
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Editor.AutoBuilding = Package.Instance.AutoBuilding;
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        }
    }
}
