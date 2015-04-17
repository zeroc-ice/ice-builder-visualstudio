// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.IO;
using System.Linq;
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
using Microsoft.Build.Evaluation;

namespace IceBuilder
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(IceOptionsPage), "Ice Builder", "General", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [Guid(GuidList.IceBuilderPackageString)]

    [ProvideObject(typeof(PropertyPage),
        RegisterUsing = RegistrationMethod.CodeBase)]

    [ProvideProjectFactory(typeof(ProjectFactory),
        "Ice Builder",
        null,
        null,
        null,
        @"..\Templates\Projects")]

    public sealed class Package : Microsoft.VisualStudio.Shell.Package
    {
        private static readonly string[] suportedVersions =
        {
            "3.6.0"
        };

        public Package()
        {
        }

        public EnvDTE.DTE DTE
        {
            get
            {
                return DTE2.DTE;
            }
        }

        public EnvDTE80.DTE2 DTE2
        {
            get;
            private set;
        }

        public DTEUtil DTEUtil
        {
            get;
            private set;
        }

        public IVsBuildManagerAccessor BuildManagerAccessor
        {
            get;
            private set;
        }

        public bool IsCommandLineMode
        {
            get;
            private set;
        }


        public Output Output
        {
            get;
            private set;
        }

        public ErrorProvider ErrorProvider
        {
            get;
            private set;
        }

        public static Package Instance
        {
            get;
            private set;
        }

        public IVsUIShell IVsUIShell
        {
            get;
            private set;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Instance = this;
            DTEUtil = new DTEUtil();
            
            String resourcesDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources");

            //
            // Copy required target, property and task files
            //
            String dataDir = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                                            "ZeroC", "IceBuilder");

            if(!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            foreach (String f in new String[] 
                { 
                    "IceBuilder.Common.props",
                    "IceBuilder.Cpp.props",
                    "IceBuilder.Cpp.targets",
                    "IceBuilder.Cpp.xml",
                    "IceBuilder.CSharp.props",
                    "IceBuilder.CSharp.targets",
                    "IceBuilder.Tasks.dll" 
                })
            {
                if (!File.Exists(Path.Combine(dataDir, f)))
                {
                    File.Copy(Path.Combine(resourcesDirectory, f),
                                Path.Combine(dataDir, f));
                }
                else
                {
                    byte[] data1 = File.ReadAllBytes(Path.Combine(resourcesDirectory, f));
                    byte[] data2 = File.ReadAllBytes(Path.Combine(dataDir, f));
                    if (!data1.SequenceEqual(data2))
                    {
                        File.Copy(Path.Combine(resourcesDirectory, f),
                                    Path.Combine(dataDir, f), true);
                    }
                }
            }

            IVsShell shell = GetService(typeof(IVsShell)) as IVsShell;
            DTE2 = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));

            {
                object value;
                shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out value);
                IsCommandLineMode = (bool)value;
            }

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if(null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandAddID = new CommandID(GuidList.IceBuilderCommandsGUI, (int)PkgCmdIDList.AddIceBuilder);
                OleMenuCommand menuItemAdd = new OleMenuCommand(AddIceBuilderToProject, menuCommandAddID);
                menuItemAdd.Enabled = false;
                mcs.AddCommand(menuItemAdd);
                menuItemAdd.BeforeQueryStatus += addIceBuilder_BeforeQueryStatus;

                CommandID menuCommanRemoveID = new CommandID(GuidList.IceBuilderCommandsGUI, (int)PkgCmdIDList.RemoveIceBuilder);
                OleMenuCommand menuItemRemove = new OleMenuCommand(RemoveIceBuilderFromProject, menuCommanRemoveID);
                menuItemRemove.Enabled = false;
                mcs.AddCommand(menuItemRemove);
                menuItemRemove.BeforeQueryStatus += removeIceBuilder_BeforeQueryStatus;
            }


            this.RegisterProjectFactory(new ProjectFactory(this));

            //
            // Subscribe to solution events.
            //
            if (!IsCommandLineMode)
            {
                _solutionEvents = DTE2.Events.SolutionEvents;
                _solutionEvents.Opened += new EnvDTE._dispSolutionEvents_OpenedEventHandler(SolutionOpened);
                _solutionEvents.AfterClosing += new EnvDTE._dispSolutionEvents_AfterClosingEventHandler(SolutionClosed);
            }

            //
            // If IceHome isn't set try to locate a supported Ice install.
            //
            if(String.IsNullOrEmpty(GetIceHome()))
            {
                foreach (String version in suportedVersions)
                {
                    String iceHome = (String)Microsoft.Win32.Registry.GetValue(
                        "HKEY_LOCAL_MACHINE\\Software\\ZeroC\\Ice " + version, "InstallDir", "");

                    if (!String.IsNullOrEmpty(iceHome))
                    {
                        SetIceHome(iceHome);
                        break;
                    }
                }
            }

            IVsUIShell = Package.Instance.GetService(typeof(SVsUIShell)) as IVsUIShell;
        }

        void addIceBuilder_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand command = sender as OleMenuCommand;
            if(command != null)
            {
                EnvDTE.Project p = DTEUtil.GetSelectedProject();
                if(p != null)
                {
                    Microsoft.Build.Evaluation.Project project = MSBuildUtils.LoadedProject(p.FullName);
                    if(MSBuildUtils.IsCppProject(project) || MSBuildUtils.IsCSharpProject(project))
                    {
                        command.Enabled = !MSBuildUtils.IsIceBuilderEnabeld(project);
                    }
                    else
                    {
                        command.Enabled = false;
                    }
                }
            }
        }

        void removeIceBuilder_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand command = sender as OleMenuCommand;
            if (command != null)
            {
                EnvDTE.Project p = DTEUtil.GetSelectedProject();
                if (p != null)
                {
                    Microsoft.Build.Evaluation.Project project = MSBuildUtils.LoadedProject(p.FullName);
                    if (MSBuildUtils.IsCppProject(project) || MSBuildUtils.IsCSharpProject(project))
                    {
                        command.Enabled = MSBuildUtils.IsIceBuilderEnabeld(project);
                    }
                    else
                    {
                        command.Enabled = false;
                    }
                }
            }
        }

        private void AddIceBuilderToProject(object sender, EventArgs e)
        {
            OleMenuCommand command = sender as OleMenuCommand;
            if (command != null)
            {
                EnvDTE.Project p = DTEUtil.GetSelectedProject();
                if(p != null)
                {
                    AddIceBuilderToProject(p);
                }
            }
        }

        private void AddIceBuilderToProject(EnvDTE.Project p)
        {
            Microsoft.Build.Evaluation.Project project = MSBuildUtils.LoadedProject(p.FullName);
            if (MSBuildUtils.IsCSharpProject(project))
            {
                //DTEUtil.UnloadProject();
                MSBuildUtils.AddIceBuilderToProject(project);
                p.Save();
                //project.Save();
                //DTEUtil.ReloadProject();
            }
            else
            {
                MSBuildUtils.AddIceBuilderToProject(project);
                p.Save();
            }
        }

        private void RemoveIceBuilderFromProject(object sender, EventArgs e)
        {
            OleMenuCommand command = sender as OleMenuCommand;
            if (command != null)
            {
                EnvDTE.Project p = DTEUtil.GetSelectedProject();
                if (p != null)
                {
                    RemoveIceBuilderFromProject(p);    
                }
            }
        }

        private void RemoveIceBuilderFromProject(EnvDTE.Project p)
        {
            Microsoft.Build.Evaluation.Project project = MSBuildUtils.LoadedProject(p.FullName);
            if (MSBuildUtils.IsCSharpProject(project))
            {
                DTEUtil.UnloadProject();
                MSBuildUtils.RemoveIceBuilderFromProject(project);
                project.Save();
                DTEUtil.ReloadProject();
            }
            else
            {
                if (MSBuildUtils.RemoveIceBuilderFromProject(project))
                {
                    p.Save();
                }
            }
        }

        public void SolutionOpened()
        {
            Output = new Output(DTE2);
            ErrorProvider = new ErrorProvider(DTE as System.IServiceProvider);

            List<EnvDTE.Project> projects = DTEUtil.GetProjects(DTE.Solution);
            foreach (EnvDTE.Project project in projects)
            {
                if(ProjectConverter.Update(MSBuildUtils.LoadedProject(project.FullName)))
                {
                    if(project.Globals != null)
                    {
                        foreach (String name in ProjectConverter.PropertyNames)
                        {
                            if (project.Globals.get_VariableExists(name))
                            {
                                project.Globals[name] = String.Empty;
                                project.Globals.set_VariablePersists(name, false);
                            }
                        }
                    }
                    AddIceBuilderToProject(project);
                }
            }
        }

        public void SolutionClosed()
        {
            //TODO disponse Output / ErrorProvider
        }

        private static String IceHomeKey = "HKEY_CURRENT_USER\\Software\\ZeroC\\IceBuilder";
        private static String IceHomeValue = "IceHome";
        private static String IceVersionValue = "IceVersion";
        private static String IceVersionMMValue = "IceVersionMM";

        private static String IceCSharpAssembleyKey =
            "HKEY_CURRENT_USER\\Software\\Microsoft\\.NETFramework\\v2.0.50727\\AssemblyFoldersEx\\Ice";


        public void SetIceHome(String value)
        {
            if (String.IsNullOrEmpty(value))
            {
                //
                // Remove all registry settings.
                //
                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceHomeValue, "",
                                                  Microsoft.Win32.RegistryValueKind.String);
                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceVersionValue, "",
                                                  Microsoft.Win32.RegistryValueKind.String);
                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceVersionMMValue, "",
                                                  Microsoft.Win32.RegistryValueKind.String);
                Microsoft.Win32.Registry.SetValue(IceCSharpAssembleyKey, "", "",
                                                  Microsoft.Win32.RegistryValueKind.String);
                return;

            }
            else
            {
                Version v = null;

                try
                {
                    v = Version.Parse(GetSliceCompilerVersion(value));
                }
                catch (System.Exception ex)
                {
                    string err = "Failed to run Slice compiler using Ice installation from `" + value + "'"
                        + "\n" + ex.ToString();

                    MessageBox.Show(err, "Ice Builder",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                    (MessageBoxOptions)0);
                    return;
                }
                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceHomeValue, value,
                                              Microsoft.Win32.RegistryValueKind.String);

                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceVersionValue, v.ToString(),
                                                  Microsoft.Win32.RegistryValueKind.String);

                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceVersionMMValue, String.Format("{0}.{1}", v.Major, v.Minor),
                                                  Microsoft.Win32.RegistryValueKind.String);

                if (File.Exists(Path.Combine(value, "bin", "slice2cpp.exe")))
                {
                    Microsoft.Win32.Registry.SetValue(IceCSharpAssembleyKey, "",
                                                      Path.Combine(value, "Assemblies"),
                                                      Microsoft.Win32.RegistryValueKind.String);
                }
                else if (File.Exists(Path.Combine(value, "cpp", "bin", "slice2cpp.exe")))
                {
                    Microsoft.Win32.Registry.SetValue(IceCSharpAssembleyKey, "",
                                                      Path.Combine(value, "csharp", "Assemblies"),
                                                      Microsoft.Win32.RegistryValueKind.String);
                }

            }
        }

        public string GetIceHome()
        {
            return (String)Microsoft.Win32.Registry.GetValue(IceHomeKey, IceHomeValue, "");
        }

        public System.Version GetIceVersion()
        {
            return System.Version.Parse((String)Microsoft.Win32.Registry.GetValue(IceHomeKey, IceVersionValue, ""));
        }

        public String GetSliceCompilerVersion(String iceHome)
        {
            String sliceCompiler = GetSliceCompilerPath(null, iceHome);
            if(!File.Exists(sliceCompiler))
            {
                Output.WriteLine("'" + sliceCompiler + "' not found, review your Ice installation");
                ErrorProvider.Add(null, sliceCompiler, TaskErrorCategory.Error, 0, 0,
                                  "'" + sliceCompiler + "' not found, review your Ice installation");
                return null;
            }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = sliceCompiler;
            process.StartInfo.Arguments = "-v";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(sliceCompiler);
            StreamReader reader = new StreamReader();
            process.OutputDataReceived += new DataReceivedEventHandler(reader.appendData);


            try
            {
                process.Start();

                //
                // When StandardError and StandardOutput are redirected, at least one
                // should use asynchronous reads to prevent deadlocks when calling
                // process.WaitForExit; the other can be read synchronously using ReadToEnd.
                //
                // See the Remarks section in the below link:
                //
                // http://msdn.microsoft.com/en-us/library/system.diagnostics.process.standarderror.aspx
                //

                // Start the asynchronous read of the standard output stream.
                process.BeginOutputReadLine();
                // Read Standard error.
                string version = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    ErrorProvider.Add(null, sliceCompiler, TaskErrorCategory.Error, 0, 0,
                                      "Slice compiler `" + sliceCompiler +
                                      "' failed to start(error code " + process.ExitCode.ToString() + ")");
                    return null;
                }
                return version;
            }
            catch(Exception ex)
            {
                Output.WriteLine("An exception was thrown when trying to start the Slice compiler\n" +
                                ex.ToString());
                ErrorProvider.Add(null, sliceCompiler, TaskErrorCategory.Error, 0, 0,
                                  "An exception was thrown when trying to start the Slice compiler\n" +
                                  ex.ToString());
                return null;
            }
            finally
            {
                process.Close();
            }
        }

        public string GetSliceCompilerPath(Microsoft.Build.Evaluation.Project project, String iceHome)
        {
            string compiler = MSBuildUtils.IsCSharpProject(project) ? "slice2cs.exe" : "slice2cpp.exe";
            if (!String.IsNullOrEmpty(iceHome))
            {
                if (File.Exists(Path.Combine(iceHome, "cpp", "bin", compiler)))
                {
                    return Path.Combine(iceHome, "cpp", "bin", compiler);
                }

                if (File.Exists(Path.Combine(iceHome, "bin", compiler)))
                {
                    return Path.Combine(iceHome, "bin", compiler);
                }
            }

            String message = "'" + compiler + "' not found";
            if (!String.IsNullOrEmpty(iceHome))
            {
                message += " in '" + iceHome + "'. You may need to update Ice Home in 'Tools > Options > Ice'";
            }
            else
            {
                message += ". You may need to set Ice Home in 'Tools > Options > Ice'";
            }
            Output.WriteLine(message);
            ErrorProvider.Add(null, "", TaskErrorCategory.Error, 0, 0, message);
            return null;
        }

        private EnvDTE.SolutionEvents _solutionEvents;
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
