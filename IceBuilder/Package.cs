// **********************************************************************
//
// Copyright (c) ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.Build.Evaluation;

namespace IceBuilder
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "5.0.3", IconResourceID = 400)]
    [ProvideOptionPage(typeof(IceOptionsPage), "Projects", "Ice Builder", 113, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [Guid(Package.IceBuilderPackageString)]

    [ProvideObject(typeof(PropertyPage),
        RegisterUsing = RegistrationMethod.CodeBase)]

    [ProvideProjectFactory(typeof(ProjectFactory),
        "Ice Builder",
        null,
        null,
        null,
        @"..\Templates\Projects")]

    [ProvideProjectFactory(typeof(ProjectFactoryOld),
        "Ice Builder Old",
        null,
        null,
        null,
        @"..\Templates\Projects")]

    public sealed class Package : Microsoft.VisualStudio.Shell.Package
    {
        public static readonly string[] AssemblyNames =
        {
            "Glacier2", "Ice", "IceBox", "IceDiscovery", "IceLocatorDiscovery",
            "IceGrid", "IcePatch2", "IceSSL", "IceStorm"
        };

        public static readonly string NuGetBuilderPackageId = "zeroc.icebuilder.msbuild";

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

        public EnvDTE.DTEEvents DTEEvents
        {
            get;
            private set;
        }

        public NuGet NuGet
        {
            get;
            private set;
        }

        public IVsProjectHelper ProjectHelper
        {
            get;
            private set;
        }

        public static void UnexpectedExceptionWarning(Exception ex)
        {
            try
            {
                Instance.OutputPane.Activate();
                Instance.OutputPane.OutputString(
                    String.Format("The Ice Builder has raised an unexpected exception:\n{0}", ex.ToString()));
            }
            catch(Exception)
            {
            }
        }

        public static void WriteMessage(string message)
        {
            Instance.OutputPane.Activate();
            Instance.OutputPane.OutputString(message);
        }

        bool autoBuilding;
        public bool AutoBuilding
        {
            get
            {
                return autoBuilding;
            }
            set
            {
                autoBuilding = value;
            }
        }

        public void SetAutoBuilding(bool value)
        {
            Registry.SetValue(IceBuilderKey, IceAutoBuilding, value ? 1 : 0, RegistryValueKind.DWord);
            AutoBuilding = value;
        }

        private bool GetAutoBuilding()
        {
            try
            {
                return 1 == (int)Registry.GetValue(IceBuilderKey, IceAutoBuilding, 0);
            }
            catch(NullReferenceException)
            {
                // Key doesn't exists use the default value
                return false;
            }
        }
        public Guid OutputPaneGUID = new Guid("CE9BFDCD-5AFD-4A77-BD40-75E0E1E5162C");

        private static void TryRemoveAssemblyFoldersExKey()
        {
            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey(
                   @"Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx", true);

                if(key.GetSubKeyNames().Contains("Ice"))
                {
                    key.DeleteSubKey("Ice");
                }
            }
            catch(Exception)
            {
            }
            finally
            {
                if(key != null)
                {
                    key.Close();
                }
            }
        }

        public void SetIceHome(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                //
                // Remove all registry settings.
                //
                Registry.SetValue(IceBuilderKey, IceHomeValue, "", RegistryValueKind.String);
                Registry.SetValue(IceBuilderKey, IceVersionValue, "", RegistryValueKind.String);
                Registry.SetValue(IceBuilderKey, IceIntVersionValue, "", RegistryValueKind.String);
                Registry.SetValue(IceBuilderKey, IceVersionMMValue, "", RegistryValueKind.String);

                TryRemoveAssemblyFoldersExKey();
                return;
            }
            else
            {
                string props = new string[]
                    {
                        Path.Combine(value, "config", "ice.props"),
                        Path.Combine(value,"cpp", "config", "ice.props"),
                        Path.Combine(value, "config", "icebuilder.props")
                    }.FirstOrDefault(path => File.Exists(path));

                if(!string.IsNullOrEmpty(props))
                {
                    Microsoft.Build.Evaluation.Project p = new Microsoft.Build.Evaluation.Project(
                        props,
                        new Dictionary<string, string>()
                            {
                                { "ICE_HOME", value }
                            },
                        null);
                    Registry.SetValue(IceBuilderKey, IceHomeValue, value, RegistryValueKind.String);

                    string version = p.GetPropertyValue(IceVersionValue);
                    Registry.SetValue(IceBuilderKey, IceVersionValue, version, RegistryValueKind.String);

                    string intVersion = p.GetPropertyValue(IceIntVersionValue);
                    Registry.SetValue(IceBuilderKey, IceIntVersionValue, intVersion, RegistryValueKind.String);

                    string mmVersion = p.GetPropertyValue(IceVersionMMValue);
                    Registry.SetValue(IceBuilderKey, IceVersionMMValue, mmVersion, RegistryValueKind.String);

                    Registry.SetValue(IceCSharpAssembleyKey, "", GetAssembliesDir(), RegistryValueKind.String);
                    ICollection<Microsoft.Build.Evaluation.Project> projects =
                        ProjectCollection.GlobalProjectCollection.GetLoadedProjects(props);
                    if(projects.Count > 0)
                    {
                        ProjectCollection.GlobalProjectCollection.UnloadProject(p);
                    }
                }
                else
                {
                    Version v = null;
                    try
                    {
                        string compiler = GetSliceCompilerVersion(value);
                        if(string.IsNullOrEmpty(compiler))
                        {
                            string err = "Unable to find a valid Ice installation in `" + value + "'";

                            MessageBox.Show(err,
                                            "Ice Builder",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error,
                                            MessageBoxDefaultButton.Button1,
                                            0);
                            return;
                        }
                        else
                        {
                            v = Version.Parse(compiler);
                        }
                    }
                    catch(Exception ex)
                    {
                        string err = "Failed to run Slice compiler using Ice installation from `" + value + "'"
                            + "\n" + ex.ToString();

                        MessageBox.Show(err, "Ice Builder",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                        return;
                    }
                    Registry.SetValue(IceBuilderKey, IceHomeValue, value, RegistryValueKind.String);

                    Registry.SetValue(IceBuilderKey, IceVersionValue, v.ToString(), RegistryValueKind.String);

                    string iceIntVersion = String.Format("{0}{1:00}{2:00}", v.Major, v.Minor, v.Build);
                    Registry.SetValue(IceBuilderKey, IceIntVersionValue, iceIntVersion, RegistryValueKind.String);

                    string iceVersionMM = string.Format("{0}.{1}", v.Major, v.Minor);
                    Registry.SetValue(IceBuilderKey, IceVersionMMValue, iceVersionMM, RegistryValueKind.String);

                    Registry.SetValue(IceCSharpAssembleyKey, "", GetAssembliesDir(), RegistryValueKind.String);
                }
            }
        }

        public string GetAssembliesDir(IVsProject project = null)
        {
            if (project != null)
            {
                string assembliesDir = project.GetEvaluatedProperty(IceAssembliesDir);
                if (Directory.Exists(assembliesDir))
                {
                    return assembliesDir;
                }
            }
            string iceHome = GetIceHome(project);
            if (Directory.Exists(Path.Combine(iceHome, "Assemblies")))
            {
                return Path.Combine(iceHome, "Assemblies");
            }
            else if (Directory.Exists(Path.Combine(iceHome, "csharp", "Assemblies")))
            {
                return Path.Combine(iceHome, "csharp", "Assemblies");
            }
            else if (Directory.Exists(Path.Combine(iceHome, "lib")))
            {
                return Path.Combine(iceHome, "lib");
            }

            return string.Empty;
        }

        public string GetIceHome(IVsProject project = null)
        {
            string iceHome = string.Empty;
            if(project != null)
            {
                iceHome = project.GetEvaluatedProperty(IceHomeValue);
            }

            if(string.IsNullOrEmpty(iceHome))
            {
                object value = Registry.GetValue(IceBuilderKey, IceHomeValue, "");
                iceHome = value == null ? string.Empty : value.ToString();
            }

            return iceHome;
        }

        public void QueueProjectsForBuilding(ICollection<IVsProject> projects)
        {
            BuildContext(true);
            OutputPane.Clear();
            OutputPane.Activate();
            foreach(IVsProject p in projects)
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
            if(_buildProjects.Count == 0)
            {
                BuildContext(false);
            }
            else if(!Building && BuildingProject == null)
            {
                IVsProject project = _buildProjects.ElementAt(0);
                ProjectUtil.SaveProject(project);
                ProjectUtil.SetupGenerated(project);
                if(BuildProject(project))
                {
                    _buildProjects.Remove(project);
                }
            }
        }

        public void ReloadProject(IVsProject project)
        {
            IVsHierarchy hier = project as IVsHierarchy;
            Guid projectGUID = Guid.Empty;
            IVsSolution.GetGuidOfProject(hier, out projectGUID);
            IVsSolution4.UnloadProject(projectGUID, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser);
            IVsSolution4.ReloadProject(ref projectGUID);
        }

        public void InitializeProjects(List<IVsProject> projects)
        {
            try
            {
                NuGet.OnNugetBatchEnd(null);
                ProjectConverter.TryUpgrade(projects);
                projects = DTEUtil.GetProjects();
                foreach (IVsProject project in projects)
                {
                    InitializeProject(project);
                }
            }
            finally
            {
                NuGet.OnNugetBatchEnd(PackageInstalled);
            }
        }

        public void InitializeProject(IVsProject project)
        {
            if (project.IsMSBuildIceBuilderInstalled())
            {
                if (project.IsCppProject())
                {
                    VCUtil.SetupSliceFilter(project.GetDTEProject());
                }
                else
                {
                    if (project is IVsAggregatableProject)
                    {
                        project.AddProjectFlavorIfNotExists(IceBuilderNewFlavor);
                    }
                }
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

            AutoBuilding = GetAutoBuilding();

            {
                Shell = GetService(typeof(IVsShell)) as IVsShell;
                object value;
                Shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out value);
                CommandLineMode = (bool)value;
            }

            RegisterProjectFactory(new ProjectFactory());
            RegisterProjectFactory(new ProjectFactoryOld());

            if(!CommandLineMode)
            {
                InstallDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ResourcesDirectory = Path.Combine(InstallDirectory, "Resources");

                DTE2 = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));
                DTEEvents = DTE.Events.DTEEvents;
                IVsSolution = GetService(typeof(SVsSolution)) as IVsSolution;
                IVsSolution4 = GetService(typeof(SVsSolution)) as IVsSolution4;
                UIShell = Instance.GetService(typeof(SVsUIShell)) as IVsUIShell;
                MonitorSelection = GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

                //
                // Update the InstallDir this was previously used in project imports, but is still usefull if you
                // need to detect the extension install dir.
                //
                Registry.SetValue(IceBuilderKey, string.Format("InstallDir.{0}", DTE.Version), InstallDirectory,
                                  RegistryValueKind.String);

                Assembly assembly = null;
                if(DTE.Version.StartsWith("11.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2012.dll"));
                }
                else if (DTE.Version.StartsWith("12.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2013.dll"));
                }
                else if (DTE.Version.StartsWith("14.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2015.dll"));
                }
                else
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2017.dll"));
                }
                var factory = assembly.GetType("IceBuilder.ProjectHelperFactoryI").GetConstructor(new Type[] { }).Invoke(
                    new object[] { }) as IVsProjectHelperFactory;

                VCUtil = factory.VCUtil;
                NuGet = factory.NuGet;
                ProjectHelper = factory.ProjectHelper;

                ProjectFactoryHelperInstance.Init(factory.VCUtil, factory.NuGet, factory.ProjectHelper);

                NuGet.OnNugetBatchEnd(PackageInstalled);

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

                BuildEvents = DTE2.Events.BuildEvents;
                BuildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
                BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            }
        }

        private void PackageInstalled()
        {
            var projects = DTEUtil.GetProjects();
            foreach (IVsProject project in projects)
            {
                // Projects that are not being track has not been previous initialized
                // initialize will do nothing if zeroc.icebuilder.msbuild package is
                // not installed
                if(project.IsMSBuildIceBuilderInstalled())
                {
                    InitializeProject(project);
                }
            }
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
                    if(Building)
                    {
                        return;
                    }
                    Building = true;

                    List<IVsProject> projects = new List<IVsProject>();
                    if (scope.Equals(EnvDTE.vsBuildScope.vsBuildScopeSolution))
                    {
                        projects = DTEUtil.GetProjects();
                    }
                    else
                    {
                        IVsProject selected = DTEUtil.GetSelectedProject();
                        if (selected != null)
                        {
                            projects.Add(selected);
                            DTEUtil.GetSubProjects(selected, ref projects);
                        }
                    }

                    foreach (IVsProject project in projects)
                    {
                        if (project.IsMSBuildIceBuilderInstalled())
                        {
                            ProjectUtil.SetupGenerated(project);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                UnexpectedExceptionWarning(ex);
            }
        }

        private void BuildEvents_OnBuildDone(EnvDTE.vsBuildScope scope, EnvDTE.vsBuildAction action)
        {
            try
            {
                Building = false;
                if(_buildProjects.Count > 0)
                {
                    BuildContext(true);
                    BuildNextProject();
                }
            }
            catch(Exception ex)
            {
                BuildContext(false);
                UnexpectedExceptionWarning(ex);
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
            return active != 0;
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

        private IVsProject BuildingProject
        {
            get;
            set;
        }

        private bool BuildProject(IVsProject project)
        {
            try
            {
                BuildLogger logger = new BuildLogger(OutputPane)
                {
                    Verbosity = LoggerVerbosity
                };
                BuildingProject = project;
                var dteproject = project.GetDTEProject();
                var activeConfiguration = dteproject.ConfigurationManager.ActiveConfiguration;
                Dictionary<string, string> properties = new Dictionary<string, string>();
                string platform = activeConfiguration.PlatformName.Equals("Any CPU") ? "AnyCPU" : activeConfiguration.PlatformName;
                string configuration = activeConfiguration.ConfigurationName;

                if(!Builder.Build(project, new BuildCallback(project, OutputPane), logger, platform, configuration))
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

        //
        // With Ice >= 3.7.0 we get the compiler version from Ice.props
        //
        private string GetSliceCompilerVersion(string iceHome)
        {
            string sliceCompiler = GetSliceCompilerPath(null, iceHome);
            if(!File.Exists(sliceCompiler))
            {
                string message = string.Format("'{0}' not found, review your Ice installation", sliceCompiler);
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

            Process process = new Process();
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

                if(process.ExitCode != 0)
                {
                    string message = string.Format("Slice compiler `{0}' failed to start(error code {1})",
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
                if(version.EndsWith("b"))
                {
                    version = string.Format("{0}.{1}", version.Substring(0, version.Length - 1), 51);
                }
                return version;
            }
            catch(Exception ex)
            {
                string message = string.Format("An exception was thrown when trying to start the Slice compiler\n{0}",
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

        private string GetSliceCompilerPath(Microsoft.Build.Evaluation.Project project, string iceHome)
        {
            string compiler = MSBuildUtils.IsCSharpProject(project) ? "slice2cs.exe" : "slice2cpp.exe";
            if(!string.IsNullOrEmpty(iceHome))
            {
                if(File.Exists(Path.Combine(iceHome, "cpp", "bin", compiler)))
                {
                    return Path.Combine(iceHome, "cpp", "bin", compiler);
                }

                if(File.Exists(Path.Combine(iceHome, "bin", compiler)))
                {
                    return Path.Combine(iceHome, "bin", compiler);
                }
            }

            string message = "'" + compiler + "' not found";
            if(!string.IsNullOrEmpty(iceHome))
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

        private readonly string BuildOutputPaneGUID = "{1BD8A850-02D1-11d1-BEE7-00A0C913D1F8}";
        private EnvDTE.OutputWindowPane _outputPane = null;
        public EnvDTE.OutputWindowPane OutputPane
        {
            get
            {
                if(_outputPane == null)
                {
                    EnvDTE.Window window = DTE2.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                    EnvDTE.OutputWindow outputWindow = window.Object as EnvDTE.OutputWindow;
                    foreach(EnvDTE.OutputWindowPane pane in outputWindow.OutputWindowPanes)
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
                object value = Registry.GetValue(
                    Path.Combine("HKEY_CURRENT_USER", DTE.RegistryRoot, "General"),
                    "MSBuildLoggerVerbosity", 2);

                uint verbosity;
                if(!UInt32.TryParse(value == null ? "2" : value.ToString(), out verbosity))
                {
                    verbosity = 2;
                }

                switch(verbosity)
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

        private string InstallDirectory
        {
            get;
            set;
        }
        public static string ResourcesDirectory
        {
            get;
            private set;
        }

        private bool Building
        {
            get;
            set;
        }

        private HashSet<IVsProject> _buildProjects = new HashSet<IVsProject>();

        public static readonly string IceBuilderKey = @"HKEY_CURRENT_USER\Software\ZeroC\IceBuilder";
        public static readonly string IceHomeValue = "IceHome";
        public static readonly string IceVersionValue = "IceVersion";
        public static readonly string IceVersionMMValue = "IceVersionMM";
        public static readonly string IceIntVersionValue = "IceIntVersion";
        public static readonly string IceAssembliesDir = "IceAssembliesDir";
        public static readonly string IceCSharpAssembleyKey =
            @"HKEY_CURRENT_USER\Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\Ice";
        public static readonly string IceAutoBuilding = "IceAutoBuilding";

        public const string IceBuilderPackageString = "ef9502be-dbc2-4568-a846-02b8e42d04c2";

        public const string IceBuilderOldFlavorGuid = "3C53C28F-DC44-46B0-8B85-0C96B85B2042";
        public const string IceBuilderOldFlavor = "{" + IceBuilderOldFlavorGuid + "}";
        public const string IceBuilderNewFlavorGuid = "28993779-3132-408A-BCB0-1D78225F4824";
        public const string IceBuilderNewFlavor = "{" + IceBuilderNewFlavorGuid + "}";
    }
}
