// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.IO;
using System.Collections.Generic;
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
    [Guid(GuidList.IceBuilderPackageString)]

    [ProvideObject(typeof(IceCustomProject.PropertyPage),
        RegisterUsing = RegistrationMethod.CodeBase)]

    [ProvideProjectFactory(typeof(IceCustomProject.ProjectFactory),
        "Ice Builder",
        null,
        null,
        null,
        @"..\Templates\Projects")]

    public sealed class IceVisualStudioPackage : Package
    {
        public static readonly String IceBuilderProjectFlavorGUID = "{3C53C28F-DC44-46B0-8B85-0C96B85B2042}";

        public IceVisualStudioPackage()
        {
        }

        public EnvDTE.DTE DTE
        {
            get
            {
                return _dte2.DTE;
            }
        }

        public EnvDTE80.DTE2 DTE2
        {
            get
            {
                return _dte2;
            }
        }

        private DTEUtil _dteUtil;
        public DTEUtil DTEUtil
        {
            get
            {
                return _dteUtil;
            }
        
        }

        protected override void Initialize()
        {
            try
            {
                base.Initialize();
                IVsShell shell = GetService(typeof(IVsShell)) as IVsShell;
                _dte2 = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));

                // Add our command handlers for menu (commands must exist in the .vsct file)
                OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

                OleMenuCommand menuItem = null;
                if (null != mcs)
                {
                    // Create the command for the menu item.
                    CommandID menuCommandID = new CommandID(GuidList.IceBuilderCommandsGUI, (int)PkgCmdIDList.AddIceBuilder);
                    menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                    menuItem.Enabled = false;
                    mcs.AddCommand(menuItem);

                    menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                    menuCommandID = new CommandID(GuidList.IceBuilderCommandsGUI, (int)PkgCmdIDList.RemoveIceBuilder);
                    menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                    menuItem.Enabled = false;
                    mcs.AddCommand(menuItem);

                    menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;
                }

                String assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (_dte2.DTE.Version.StartsWith("11.0"))
                {
                    _assembly = Assembly.LoadFrom(Path.Combine(assemblyDir, "Resources\\IceVisualStudio_2012.dll"));
                }
                else
                {
                    _assembly = Assembly.LoadFrom(Path.Combine(assemblyDir, "Resources\\IceVisualStudio_2013.dll"));
                }

                Type builder = _assembly.GetType("ZeroC.IceVisualStudio.Builder");

                MethodInfo create = builder.GetMethod("create");

                create.Invoke(null, new object[] { shell, _dte2, menuItem });

                _menuCallback = builder.GetMethod("MenuItemCallback");

                Type util = _assembly.GetType("ZeroC.IceVisualStudio.Util");
                _setIceHome = util.GetMethod("setIceHome");
                _getIceHome = util.GetMethod("getIceHome");


                _dteUtil = new DTEUtil(this,
                    _assembly.GetType("IceVisualStudio.VCUtilI").GetConstructor(new Type[] { }).Invoke(new object[]{}) as IceBuilder_Common.VCUtil,
                    _assembly.GetType("IceVisualStudio.VSUtilI").GetConstructor(new Type[] { }).Invoke(new object[]{}) as IceBuilder_Common.VSUtil);

                this.RegisterProjectFactory(new IceCustomProject.ProjectFactory(this));
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

        void menuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand command = sender as OleMenuCommand;
            if (command != null)
            {
                command.Visible = false;
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            //_menuCallback.Invoke(null, null);

            
            EnvDTE.Project dteProject = DTEUtil.getSelectedProject();
            Microsoft.Build.Evaluation.Project msbuildProject = MSBuildUtils.LoadedProject(dteProject.FullName);
            //dteProject.Save();
            //DTEUtil.UnloadProject(_dte2.DTE);
            if(DTEUtil.VCUtil.IsCppProject(dteProject))
            {
                if(MSBuildUtils.AddImportIfNotExists(msbuildProject, MSBuildUtils.IceCppTargetsPath, "Ice Builder"))
                {
                    dteProject.Save();
                }
            }
            else if (DTEUtil.VSUtil.IsCSharpProject(dteProject))
            { 
                bool needSave = MSBuildUtils.AddProjectFlavorIfNotExists(msbuildProject, IceBuilderProjectFlavorGUID);
                needSave = MSBuildUtils.AddImportIfNotExists(msbuildProject, MSBuildUtils.IceCSharpTargetsPath, "Ice Builder") || needSave;
                if(needSave)
                {
                    dteProject.Save();
                    DTEUtil.UnloadProject();
                    DTEUtil.ReloadProject();
                }
            }

            //DTEUtil.ReloadProject(_dte2.DTE);
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

        private EnvDTE80.DTE2 _dte2;
        private MethodInfo _menuCallback;
        private static MethodInfo _setIceHome;
        private static MethodInfo _getIceHome;

        private static Assembly _assembly = null;
    }

    static class PkgCmdIDList
    {
        public const uint AddIceBuilder = 0x100;
        public const uint RemoveIceBuilder = 0x101;
    };

    static class GuidList
    {
        public const string IceBuilderPackageString = "ef9502be-dbc2-4568-a846-02b8e42d04c2";
        public const string IceBuilderCommands = "6a1127de-354d-414d-968e-f2d8f44147a4";

        public static readonly Guid IceBuilderCommandsGUI = new Guid(IceBuilderCommands);
    };
}
