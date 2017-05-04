// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace IceBuilder
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}")]
    [Guid("D381D516-A7DA-4BFD-BD04-A649F2DB947F")]
    public class Package : Microsoft.VisualStudio.Shell.Package
    {
        protected override void Initialize()
        {
            base.Initialize();
            IVsShell shell = GetService(typeof(IVsShell)) as IVsShell;
            object value;
            shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out value);
            if (!(bool)value)
            {
                string installDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\ZeroC\IceBuilder", "InstallDir.10.0", installDirectory,
                                  RegistryValueKind.String);
            }
        }
    }
}
