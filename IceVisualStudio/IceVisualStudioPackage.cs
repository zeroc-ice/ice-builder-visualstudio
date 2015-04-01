// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.IO;

using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace ZeroC.IceVisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(IceOptionsPage), "Ice Builder", "General", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [Guid(GuidList.guidIceVisualStudioPkgString)]
    public sealed class IceVisualStudioPackage : Package
    {
        public IceVisualStudioPackage()
        {
        }
        
        protected override void Initialize()
        {
            try
            {
                base.Initialize();
                IVsShell shell = GetService(typeof(IVsShell)) as IVsShell;
                EnvDTE80.DTE2 dte2 = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));

                // Add our command handlers for menu (commands must exist in the .vsct file)
                OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

                MenuCommand menuItem = null;
                if (null != mcs)
                {
                    // Create the command for the menu item.
                    CommandID menuCommandID = new CommandID(GuidList.guidIceVisualStudioCmdSet, (int)PkgCmdIDList.IceConfiguration);
                    menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                    menuItem.Enabled = false;
                    mcs.AddCommand(menuItem);
                }

                Assembly assembly = null;
                String assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (dte2.DTE.Version.StartsWith("11.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(assemblyDir, "Resources\\IceVisualStudio_2012.dll"));
                }
                else
                {
                    assembly = Assembly.LoadFrom(Path.Combine(assemblyDir, "Resources\\IceVisualStudio_2013.dll"));
                }

                Type builder = assembly.GetType("ZeroC.IceVisualStudio.Builder");

                MethodInfo create = builder.GetMethod("create");

                create.Invoke(null, new object[] { shell, dte2, menuItem });

                _menuCallback = builder.GetMethod("MenuItemCallback");

                Type util = assembly.GetType("ZeroC.IceVisualStudio.Util");
                _setIceHome = util.GetMethod("setIceHome");
                _getIceHome = util.GetMethod("getIceHome");
            }
            catch(System.Exception ex)
            {
                MessageBox.Show("The Ice Builder has raised an unexpected exception:\n" +
                                ex.ToString(),
                                "Ice Builder", MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1,
                                (MessageBoxOptions)0);
                throw;
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            _menuCallback.Invoke(null, null);
        }

        public static void setIceHome(String value)
        { 
            _setIceHome.Invoke(null, new object[]{ value });
        }

        public static String getIceHome()
        {
            Object result = _getIceHome.Invoke(null, null);
            return result == null ? "" : result.ToString();
        }

        private MethodInfo _menuCallback;
        private static MethodInfo _setIceHome;
        private static MethodInfo _getIceHome;
    }

    static class PkgCmdIDList
    {
        public const uint IceConfiguration = 0x100;
    };

    static class GuidList
    {
        public const string guidIceVisualStudioPkgString = "ef9502be-dbc2-4568-a846-02b8e42d04c2";
        public const string guidIceVisualStudioCmdSetString = "6a1127de-354d-414d-968e-f2d8f44147a4";

        public static readonly Guid guidIceVisualStudioCmdSet = new Guid(guidIceVisualStudioCmdSetString);
    };
}
