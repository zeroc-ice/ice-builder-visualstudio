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
    [ProvideOptionPage(typeof(IceOptionsPage), "Projects", "Ice Builder", 113, 0, true)]
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
        #region Visual Studio Services

        public IVsShell Shell
        {
            get;
            private set;
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

        public IVsUIShell UIShell
        {
            get;
            private set;
        }

        public IVsSolution IVsSolution
        {
            get;
            private set;
        }

        public IVsSolution4 IVsSolution4
        {
            get;
            private set;
        }

        private SolutionEventHandler SolutionEventHandler
        {
            get;
            set;
        }

        public RunningDocumentTableEventHandler RunningDocumentTableEventHandler
        {
            get;
            private set;
        }

        public IVsMonitorSelection MonitorSelection
        {
            get;
            set;
        }

        private EnvDTE.BuildEvents BuildEvents
        {
            get;
            set;
        }
        #endregion

        public static Package Instance
        {
            get;
            private set;
        }

        public GeneratedFileTracker FileTracker
        {
            get;
            private set;
        }

        public EnvDTE.DTEEvents DTEEvents
        {
            get;
            private set;
        }

        public static void UnexpectedExceptionWarning(Exception ex)
        {
            try
            {
                if(!Package.Instance.CommandLineMode)
                {
                    MessageBox.Show("The Ice Builder has raised an unexpected exception:\n" +
                                    ex.ToString(),
                                    "Ice Builder", MessageBoxButtons.OK,
                                    MessageBoxIcon.Error,
                                    MessageBoxDefaultButton.Button1,
                                    (MessageBoxOptions)0);
                }
            }
            catch (Exception)
            {
            }

            try
            {
                Package.Instance.OutputPane.OutputString(ex.ToString());
            }
            catch (Exception)
            {
            }
        }

        public Guid OutputPaneGUID = new Guid("CE9BFDCD-5AFD-4A77-BD40-75E0E1E5162C");

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
                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceIntVersionValue, "",
                                                  Microsoft.Win32.RegistryValueKind.String);
                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceVersionMMValue, "",
                                                  Microsoft.Win32.RegistryValueKind.String);
                Microsoft.Win32.Registry.SetValue(IceCSharpAssembleyKey, "", "",
                                                  Microsoft.Win32.RegistryValueKind.String);

                if(DTE2.Solution != null)
                {
                    MSBuildUtils.SetIceHome(DTEUtil.GetProjects(DTE2.Solution), "", "", "", "");
                }
                return;
            }
            else
            {
                Version v = null;

                try
                {
                    String compiler = GetSliceCompilerVersion(value);
                    if(String.IsNullOrEmpty(compiler))
                    {
                        string err = "Unable to find a valid Ice installation in `" + value + "'";

                        MessageBox.Show(err, 
                                        "Ice Builder",
                                        MessageBoxButtons.OK, 
                                        MessageBoxIcon.Error, 
                                        MessageBoxDefaultButton.Button1,
                                        (MessageBoxOptions)0);
                        return;
                    }
                    else
                    {
                        v = Version.Parse(compiler);
                    }
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

                String iceIntVersion = String.Format("{0}{1:00}{2:00}", v.Major, v.Minor, v.Build);
                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceIntVersionValue, 
                                                  iceIntVersion,
                                                  Microsoft.Win32.RegistryValueKind.String);

                String iceVersionMM = String.Format("{0}.{1}", v.Major, v.Minor);
                Microsoft.Win32.Registry.SetValue(IceHomeKey, IceVersionMMValue, iceVersionMM,
                                                  Microsoft.Win32.RegistryValueKind.String);

                if (DTE2.Solution != null)
                {
                    MSBuildUtils.SetIceHome(DTEUtil.GetProjects(DTE2.Solution), value, v.ToString(), iceIntVersion, iceVersionMM);
                }

                Microsoft.Win32.Registry.SetValue(IceCSharpAssembleyKey, "", GetAssembliesDir(value),
                                                  Microsoft.Win32.RegistryValueKind.String);
            }
        }

        public String GetAssembliesDir(String iceHome)
        {
            if (File.Exists(Path.Combine(iceHome, "bin", "slice2cpp.exe")))
            {
                return Path.Combine(iceHome, "Assemblies");
            }
            else if (File.Exists(Path.Combine(iceHome, "cpp", "bin", "slice2cpp.exe")))
            {
                return Path.Combine(iceHome, "csharp", "Assemblies");
            }
            return String.Empty;
        }

        public String GetIceHome()
        {
            Object value = Microsoft.Win32.Registry.GetValue(IceHomeKey, IceHomeValue, "");
            return value == null ? String.Empty : value.ToString();
        }

        public void QueueProjectsForBuilding(ICollection<EnvDTE.Project> projects)
        {
            BuildContext(true);
            OutputPane.Clear();
            OutputPane.Activate();
            foreach (EnvDTE.Project p in projects)
            {
                _buildProjects.Add(p);
            }
            BuildNextProject();
        }

        public void BuildDone()
        {
            BuildingProject = null;
            BuildNextProject();
        }

        public void BuildNextProject()
        {
            if (_buildProjects.Count == 0)
            {
                BuildContext(false);
            }
            else if (!Building && BuildingProject == null)
            {
                EnvDTE.Project p = _buildProjects.ElementAt(0);
                if (BuildProject(p))
                {
                    _buildProjects.Remove(p);
                }
            }
        }

        public void InitializeProjects()
        {
            //
            // Postpone project initialization until the AddIn has been
            // removed.
            //
            if(!File.Exists(AddinPath))
            {
                OutputPane.Clear();
                InitializeProjects(DTEUtil.GetProjects(DTE.Solution));
            }
        }

        public void InitializeProjects(List<EnvDTE.Project> projects)
        {
            ProjectConverter.TryUpgrade(projects);
            projects = DTEUtil.GetProjects(DTE.Solution);
            foreach (EnvDTE.Project project in projects)
            {
                List<EnvDTE.Project> build = new List<EnvDTE.Project>();
                if (DTEUtil.IsIceBuilderEnabled(project))
                {
                    if (DTEUtil.IsCppProject(project))
                    {
                        VCUtil.SetupSliceFilter(project);
                    }
                    FileTracker.Add(project);
                    build.Add(project);
                }
                QueueProjectsForBuilding(build);
            }
        }

        public bool CommandLineMode
        {
            get;
            private set;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Instance = this;

            {
                Shell = GetService(typeof(IVsShell)) as IVsShell;
                object value;
                Shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out value);
                CommandLineMode = (bool)value;
            }

            this.RegisterProjectFactory(new ProjectFactory());
            
            if (!CommandLineMode)
            {
                ResourcesDirectory = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources");

                //
                // Copy required target, property and task files
                //
                String dataDir = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "ZeroC", "IceBuilder");
                if (!Directory.Exists(dataDir))
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
                        File.Copy(Path.Combine(ResourcesDirectory, f), Path.Combine(dataDir, f));
                    }
                    else
                    {
                        byte[] data1 = File.ReadAllBytes(Path.Combine(ResourcesDirectory, f));
                        byte[] data2 = File.ReadAllBytes(Path.Combine(dataDir, f));
                        if (!data1.SequenceEqual(data2))
                        {
                            File.Copy(Path.Combine(ResourcesDirectory, f), Path.Combine(dataDir, f), true);
                        }
                    }
                }

                DTE2 = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));
                DTEEvents = DTE.Events.DTEEvents;
                IVsSolution = GetService(typeof(SVsSolution)) as IVsSolution;
                IVsSolution4 = GetService(typeof(SVsSolution)) as IVsSolution4;
                UIShell = Package.Instance.GetService(typeof(SVsUIShell)) as IVsUIShell;
                MonitorSelection = GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

                // Add our command handlers for menu (commands must exist in the .vsct file)
                OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

                if (null != mcs)
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

                //
                // If IceHome isn't set or is set to an invlid location, try to 
                // locate the latest version installed and use that.
                //
                Version version = null;
                Version latest = null;
                String iceHome = null;
                bool updateIceHome = true;
                try
                {
                    if (File.Exists(Path.Combine(GetIceHome(), "bin", "slice2cpp.exe")) ||
                        File.Exists(Path.Combine(GetIceHome(), "cpp", "bin", "slice2cpp.exe")))
                    {
                        updateIceHome = false;
                    }
                }
                catch (ArgumentException)
                { 
                }

                if(updateIceHome)
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\ZeroC"))
                    {
                        if (key != null)
                        {
                            foreach (String name in key.GetSubKeyNames())
                            {
                                using (RegistryKey subKey = key.OpenSubKey(name))
                                {
                                    if(subKey == null)
                                    {
                                        continue;
                                    }

                                    object value = subKey.GetValue("InstallDir");
                                    if (value == null)
                                    {
                                        continue;
                                    }

                                    String installDir = value.ToString();

                                    try
                                    {
                                        if (!File.Exists(Path.Combine(installDir, "bin", "slice2cpp.exe")))
                                        {
                                            continue;
                                        }
                                    }
                                    catch (ArgumentException)
                                    {
                                        // Could happen if install dir is null or has invalid characters
                                        continue;
                                    }

                                    try
                                    {
                                        version = Version.Parse(GetSliceCompilerVersion(installDir));
                                    }
                                    catch (System.Exception)
                                    {
                                        continue;
                                    }

                                    if (version.Build == 51)
                                    {
                                        // Ignore beta version
                                        continue;
                                    }

                                    if (version < new Version(3, 6, 0))
                                    { 
                                        //We require 3.6.0 or greatest
                                        continue;
                                    }

                                    if (!name.Equals(
                                        String.Format("Ice {0}.{1}.{2}",
                                            version.Major, version.Minor, version.Build)))
                                    {
                                        continue;
                                    }

                                    latest = (latest == null) ? version : (latest > version) ? latest : version;
                                    if (latest.Equals(version))
                                    {
                                        iceHome = installDir;
                                    }
                                }
                            }
                        }
                    }

                    if (iceHome != null)
                    {
                        SetIceHome(iceHome);
                    }
                    else
                    {
                        //
                        // Postpone the setting of Ice home until the add-in has been removed.
                        //
                        if(!File.Exists(AddinPath))
                        {
                            if (MessageBox.Show(
                                    "The selected Ice home directory does not exist or is not " +
                                    "an Ice installation. Please provide a correct Ice home directory.", 
                                    "Ice Builder - Incorrect Ice home", 
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Information) == DialogResult.OK)
                            {
                                DTEEvents.OnStartupComplete += ShowIceOptionsPage;
                            }
                        }
                    }
                }

                Assembly assembly = null;
                if (DTE.Version.StartsWith("11.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2012.dll"));
                }
                else if (DTE.Version.StartsWith("12.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2013.dll"));
                }
                else
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2015.dll"));
                }
                VCUtil = assembly.GetType("IceBuilder.VCUtilI").GetConstructor(new Type[] { }).Invoke(
                    new object[] { }) as VCUtil;

                RunningDocumentTableEventHandler = new RunningDocumentTableEventHandler(
                    GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable);

                Builder = new Builder(GetService(typeof(SVsBuildManagerAccessor)) as IVsBuildManagerAccessor2);

                //
                // Subscribe to solution events.
                //
                SolutionEventHandler = new SolutionEventHandler();
                SolutionEventHandler.BeginTrack();

                DocumentEventHandler = new DocumentEventHandler(
                    GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2);
                DocumentEventHandler.BeginTrack();

                FileTracker = new GeneratedFileTracker();

                BuildEvents = DTE2.Events.BuildEvents;
                BuildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
                BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;

                if(File.Exists(AddIn.Path))
                {
                    DTEEvents.OnStartupComplete += AddinRemoval;
                }
            }
        }

        void ShowIceOptionsPage()
        {
            ShowOptionPage(typeof(IceOptionsPage));
        }

        private void BuildEvents_OnBuildBegin(EnvDTE.vsBuildScope scope, EnvDTE.vsBuildAction action)
        {
            try
            {
                if(action == EnvDTE.vsBuildAction.vsBuildActionBuild ||
                   action == EnvDTE.vsBuildAction.vsBuildActionRebuildAll)
                {
                    //
                    // Ensure this runs once for parallel builds.
                    //
                    if (Building)
                    {
                        return;
                    }
                    Building = true;
                }

                List<EnvDTE.Project> projects = new List<EnvDTE.Project>();
                if (scope.Equals(EnvDTE.vsBuildScope.vsBuildScopeSolution))
                {
                    projects = DTEUtil.GetProjects(DTE2.Solution);
                }
                else
                {
                    EnvDTE.Project selected = DTEUtil.GetSelectedProject();
                    if(selected != null)
                    {
                        DTEUtil.GetProjects(selected, ref projects);
                    }
                }

                foreach (EnvDTE.Project project in projects)
                {
                    if (DTEUtil.IsIceBuilderEnabled(project))
                    {
                        FileTracker.Reap(project);
                        ProjectUtil.SetupGenerated(project);
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        private void BuildEvents_OnBuildDone(EnvDTE.vsBuildScope scope, EnvDTE.vsBuildAction action)
        {
            try
            {
                Building = false;
                if (_buildProjects.Count > 0)
                {
                    BuildContext(true);
                    BuildNextProject();
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        private void SetCmdUIContext(Guid context, bool enabled)
        {
            uint cookie;
            ErrorHandler.ThrowOnFailure(MonitorSelection.GetCmdUIContextCookie(ref context, out cookie));
            if(cookie != 0)
            {
                ErrorHandler.ThrowOnFailure(MonitorSelection.SetCmdUIContext(cookie, (enabled ? 1 : 0)));
            }
        }

        private bool IsCmdUIContextActive(Guid context)
        {
            uint cookie;
            ErrorHandler.ThrowOnFailure(MonitorSelection.GetCmdUIContextCookie(ref context, out cookie));
            int active = 0;
            if(cookie != 0)
            {
                ErrorHandler.ThrowOnFailure(MonitorSelection.IsCmdUIContextActive(cookie, out active));
            }
            return active != 0; ;
        }

        private void BuildContext(bool enabled)
        {
            SetCmdUIContext(VSConstants.UICONTEXT.SolutionBuilding_guid, enabled);
            if(enabled)
            {
                SetCmdUIContext(VSConstants.UICONTEXT.NotBuildingAndNotDebugging_guid, false);
                SetCmdUIContext(VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_guid, false);
            }
            else
            {
                bool debugging = IsCmdUIContextActive(VSConstants.UICONTEXT.Debugging_guid);
                SetCmdUIContext(VSConstants.UICONTEXT.NotBuildingAndNotDebugging_guid, !debugging);
                SetCmdUIContext(VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_guid, !debugging);
            }
        }

        private EnvDTE80.SolutionConfiguration2 ActiveConfiguration
        {
            get
            {
                return (DTE.Solution.SolutionBuild as EnvDTE80.SolutionBuild2).ActiveConfiguration as EnvDTE80.SolutionConfiguration2;
            }
        }

        private EnvDTE.Project BuildingProject
        {
            get;
            set;
        }

        private bool BuildProject(EnvDTE.Project project)
        {
            try
            {
                BuildLogger logger = new BuildLogger(OutputPane);
                logger.Verbosity = LoggerVerbosity;
                BuildingProject = project;
                if (!Builder.Build(project, new BuildCallback(project, OutputPane, ActiveConfiguration),
                                   logger))
                {
                    BuildingProject = null;
                }
                return BuildingProject != null;
            }
            catch(Exception)
            {
                BuildingProject = null;
                return false;
            }
        }

        private void addIceBuilder_BeforeQueryStatus(object sender, EventArgs e)
        {
            try
            {
                OleMenuCommand command = sender as OleMenuCommand;
                if(command != null)
                {
                    EnvDTE.Project p = DTEUtil.GetSelectedProject();
                    if(p != null)
                    {
                        if(DTEUtil.IsCppProject(p) || DTEUtil.IsCSharpProject(p))
                        {
                            command.Enabled = !MSBuildUtils.IsIceBuilderEnabeld(MSBuildUtils.LoadedProject(p.FullName));
                        }
                        else
                        {
                            command.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        private void removeIceBuilder_BeforeQueryStatus(object sender, EventArgs e)
        {
            try
            {
                OleMenuCommand command = sender as OleMenuCommand;
                if (command != null)
                {
                    EnvDTE.Project p = DTEUtil.GetSelectedProject();
                    if (p != null)
                    {
                        if (DTEUtil.IsCppProject(p) || DTEUtil.IsCSharpProject(p))
                        {
                            command.Enabled = MSBuildUtils.IsIceBuilderEnabeld(MSBuildUtils.LoadedProject(p.FullName));
                        }
                        else
                        {
                            command.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        private void AddIceBuilderToProject(object sender, EventArgs e)
        {
            try
            {
                OleMenuCommand command = sender as OleMenuCommand;
                if (command != null)
                {
                    EnvDTE.Project p = DTEUtil.GetSelectedProject();
                    if (p != null)
                    {
                        AddIceBuilderToProject(p);
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        public void AddIceBuilderToProject(EnvDTE.Project p)
        {
            Microsoft.Build.Evaluation.Project project = MSBuildUtils.LoadedProject(p.FullName);
            if (MSBuildUtils.AddIceBuilderToProject(project))
            {
                if (DTEUtil.IsCppProject(p))
                {
                    VCUtil.SetupSliceFilter(p);
                }
                else
                {
                    String includeDirectories = ProjectUtil.GetProperty(p, PropertyNames.IncludeDirectories);
                    if (String.IsNullOrEmpty(includeDirectories))
                    {
                        ProjectUtil.SetProperty(p, PropertyNames.IncludeDirectories, @"$(IceHome)\slice");
                    }
                    else if(includeDirectories.IndexOf(@"$(IceHome)\slice") == -1)
                    {
                        ProjectUtil.SetProperty(p, PropertyNames.IncludeDirectories, 
                            String.Format(@"$(IceHome)\slice;{0}", includeDirectories));
                    }
                    ProjectUtil.AddAssemblyReference(p, "Ice");
                }
                p.Save();
                IVsHierarchy hier = DTEUtil.GetIVsHierarchy(p);
                Guid projectGUID = Guid.Empty;
                IVsSolution.GetGuidOfProject(hier, out projectGUID);
                IVsSolution.CloseSolutionElement((uint)__VSSLNCLOSEOPTIONS.SLNCLOSEOPT_UnloadProject, hier, 0);
                project.Save();
                IVsSolution4.ReloadProject(ref projectGUID);
            }
        }

        private void RemoveIceBuilderFromProject(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
        }

        private void RemoveIceBuilderFromProject(EnvDTE.Project p)
        {
            Microsoft.Build.Evaluation.Project project = MSBuildUtils.LoadedProject(p.FullName);
            MSBuildUtils.RemoveIceBuilderFromProject(project);
            foreach (EnvDTE.Project p1 in _buildProjects)
            { 
                if(p.FullName.Equals(p.FullName))
                {
                    _buildProjects.Remove(p1);
                    break;
                }
            }


            ProjectUtil.DeleteItems(p,
                ProjectUtil.GetGeneratedFiles(p).Aggregate(
                    new List<String>(),
                    (items, kv) => 
                        {
                            items.AddRange(kv.Value);
                            return items;
                        }));

            Directory.GetFiles(GetAssembliesDir(GetIceHome()), "*.dll")
                .ToList()
                .ForEach(item =>
                    {
                        String name = Path.GetFileNameWithoutExtension(item);
                        if(ProjectUtil.HasAssemblyReference(p, name))
                        {
                            ProjectUtil.RemoveAssemblyReference(p, name);
                        }
                    });
            p.Save();

            Guid projectGUID = Guid.Empty;
            IVsHierarchy hier = DTEUtil.GetIVsHierarchy(p);
            IVsSolution.GetGuidOfProject(hier, out projectGUID);
            IVsSolution.CloseSolutionElement((uint)__VSSLNCLOSEOPTIONS.SLNCLOSEOPT_UnloadProject, hier, 0);
            project.Save();
            IVsSolution4.ReloadProject(ref projectGUID);
        }

        private String GetSliceCompilerVersion(String iceHome)
        {
            String sliceCompiler = GetSliceCompilerPath(null, iceHome);
            if(!File.Exists(sliceCompiler))
            {
                String message = String.Format("'{0}' not found, review your Ice installation", sliceCompiler);
                OutputPane.OutputTaskItemString(
                    message, 
                    EnvDTE.vsTaskPriority.vsTaskPriorityHigh,
                    EnvDTE.vsTaskCategories.vsTaskCategoryBuildCompile, 
                    EnvDTE.vsTaskIcon.vsTaskIconCompile, 
                    sliceCompiler, 
                    0, 
                    message);
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
                    String message = String.Format("Slice compiler `{0}' failed to start(error code {1})", 
                        sliceCompiler, process.ExitCode);
                    OutputPane.OutputTaskItemString(
                        message, 
                        EnvDTE.vsTaskPriority.vsTaskPriorityHigh,
                        EnvDTE.vsTaskCategories.vsTaskCategoryBuildCompile, 
                        EnvDTE.vsTaskIcon.vsTaskIconCompile, 
                        sliceCompiler, 
                        0, 
                        message);
                    return null;
                }

                //
                // Convert beta version to is numeric value
                //
                if (version.EndsWith("b"))
                {
                    version = String.Format("{0}.{1}",
                        version.Substring(0, version.Length - 1), 51);
                }
                return version;
            }
            catch(Exception ex)
            {
                String message = String.Format("An exception was thrown when trying to start the Slice compiler\n{0}",
                        ex.ToString());
                OutputPane.OutputTaskItemString(
                    message, 
                    EnvDTE.vsTaskPriority.vsTaskPriorityHigh,
                    EnvDTE.vsTaskCategories.vsTaskCategoryBuildCompile,
                    EnvDTE.vsTaskIcon.vsTaskIconCompile,
                    sliceCompiler,
                    0,
                    message);
                return null;
            }
            finally
            {
                process.Close();
            }
        }

        private string GetSliceCompilerPath(Microsoft.Build.Evaluation.Project project, String iceHome)
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
            OutputPane.OutputTaskItemString(
                message, 
                EnvDTE.vsTaskPriority.vsTaskPriorityHigh,
                EnvDTE.vsTaskCategories.vsTaskCategoryBuildCompile, 
                EnvDTE.vsTaskIcon.vsTaskIconCompile, 
                compiler, 
                0,
                message);
            return null;
        }

        private readonly String BuildOutputPaneGUID = "{1BD8A850-02D1-11d1-BEE7-00A0C913D1F8}";
        private EnvDTE.OutputWindowPane _outputPane = null;
        private EnvDTE.OutputWindowPane OutputPane
        {
            get
            {
                if (_outputPane == null)
                {
                    EnvDTE.Window window = DTE2.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                    EnvDTE.OutputWindow outputWindow = window.Object as EnvDTE.OutputWindow;
                    foreach (EnvDTE.OutputWindowPane pane in outputWindow.OutputWindowPanes)
                    {
                        if(pane.Guid.Equals(BuildOutputPaneGUID, StringComparison.CurrentCultureIgnoreCase))
                        {
                            _outputPane = pane;
                            break;
                        }
                    }
                }
                return _outputPane;
            }
        }

        private DocumentEventHandler DocumentEventHandler
        {
            get;
            set;
        }

        public VCUtil VCUtil
        {
            get;
            private set;
        }

        private Builder Builder
        {
            get;
            set;
        }

        private Microsoft.Build.Framework.LoggerVerbosity LoggerVerbosity
        {
            get
            {
                object value = Microsoft.Win32.Registry.GetValue(
                    Path.Combine("HKEY_CURRENT_USER", DTE.RegistryRoot, "General"),
                    "MSBuildLoggerVerbosity", 2);

                uint verbosity;
                if (!UInt32.TryParse(value == null ? "2" : value.ToString(), out verbosity))
                {
                    verbosity = 2;
                }

                switch (verbosity)
                {
                    case 0:
                        return Microsoft.Build.Framework.LoggerVerbosity.Quiet;
                    case 1:
                        return Microsoft.Build.Framework.LoggerVerbosity.Minimal;
                    case 3:
                        return Microsoft.Build.Framework.LoggerVerbosity.Detailed;
                    case 4:
                        return Microsoft.Build.Framework.LoggerVerbosity.Diagnostic;
                    default:
                        return Microsoft.Build.Framework.LoggerVerbosity.Normal;
                }
            }
        }

        private void AddinRemoval()
        {
            String path = Path.Combine(
                System.Environment.GetEnvironmentVariable("ALLUSERSPROFILE"),
                    (DTE.Version.StartsWith("11.0") ?
                        "Microsoft\\VisualStudio\\11.0\\Addins\\Ice-VS2012.AddIn" :
                        "Microsoft\\VisualStudio\\12.0\\Addins\\Ice-VS2013.AddIn"));
            if (File.Exists(path))
            {
                if(MessageBox.Show("Detected Ice Add-in for Visual Studio; this Add-in is not compatible with " +
                                   "the Ice Builder for Visual Studio. Do you want to remove this Add-in?",
                                   "Ice Builder", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = Path.Combine(ResourcesDirectory, "AddinRemoval.exe");
                    process.StartInfo.Arguments = String.Format("\"{0}\" \"{1}\"", DTE.FullName, path);
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = true;
                    process.Start();
                    process.WaitForExit();

                    ((IVsShell4)Shell).Restart((uint)__VSRESTARTTYPE.RESTART_Normal);
                }
            }
        }

        private String ResourcesDirectory
        {
            get;
            set;
        }

        private bool Building
        {
            get;
            set;
        }
       
        private HashSet<EnvDTE.Project> _buildProjects = new HashSet<EnvDTE.Project>();

        public static readonly String IceHomeKey = @"HKEY_CURRENT_USER\Software\ZeroC\IceBuilder";
        public static readonly String IceHomeValue = "IceHome";
        public static readonly String IceVersionValue = "IceVersion";
        public static readonly String IceVersionMMValue = "IceVersionMM";
        public static readonly String IceIntVersionValue = "IceIntVersion";
        public static readonly String IceCSharpAssembleyKey =
            @"HKEY_CURRENT_USER\Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\Ice";
        private static readonly string[] suportedVersions =
        {
            "3.6.0"
        };

        public static readonly String AddinPath = Path.Combine(
            System.Environment.GetEnvironmentVariable("ALLUSERSPROFILE"),
                (DTE.Version.StartsWith("11.0") ?
                    "Microsoft\\VisualStudio\\11.0\\Addins\\Ice-VS2012.AddIn" :
                    "Microsoft\\VisualStudio\\12.0\\Addins\\Ice-VS2013.AddIn"));
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
