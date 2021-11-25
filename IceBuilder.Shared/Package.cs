// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace IceBuilder
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "6.0.3", IconResourceID = 400)]
    [ProvideOptionPage(typeof(IceOptionsPage), "Projects", "Ice Builder", 113, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(IceBuilderPackageString)]

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

    public sealed class Package : AsyncPackage
    {
        public static readonly string[] AssemblyNames =
        {
            "Glacier2", "Ice", "IceBox", "IceDiscovery", "IceLocatorDiscovery",
            "IceGrid", "IcePatch2", "IceSSL", "IceStorm"
        };

        public static readonly string NuGetBuilderPackageId = "zeroc.icebuilder.msbuild";

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

        public static Package Instance
        {
            get;
            private set;
        }

        public INuGet NuGet
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
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Instance.OutputPane.Activate();
                Instance.OutputPane.OutputString(
                    string.Format("The Ice Builder has raised an unexpected exception:\n{0}", ex.ToString()));
            }
            catch (Exception)
            {
            }
        }

        public static void WriteMessage(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Instance.OutputPane.Activate();
            Instance.OutputPane.OutputString(message);
        }

        public bool AutoBuilding { get; set; }

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
            catch (NullReferenceException)
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

                if (key.GetSubKeyNames().Contains("Ice"))
                {
                    key.DeleteSubKey("Ice");
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
            }
        }

        public void SetIceHome(string value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (string.IsNullOrEmpty(value))
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

                if (!string.IsNullOrEmpty(props))
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
                    if (projects.Count > 0)
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
                        if (string.IsNullOrEmpty(compiler))
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
                    catch (Exception ex)
                    {
                        string err = "Failed to run Slice compiler using Ice installation from `" + value + "'"
                            + "\n" + ex.ToString();

                        MessageBox.Show(err, "Ice Builder",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                        return;
                    }
                    Registry.SetValue(IceBuilderKey, IceHomeValue, value, RegistryValueKind.String);

                    Registry.SetValue(IceBuilderKey, IceVersionValue, v.ToString(), RegistryValueKind.String);

                    var iceIntVersion = string.Format("{0}{1:00}{2:00}", v.Major, v.Minor, v.Build);
                    Registry.SetValue(IceBuilderKey, IceIntVersionValue, iceIntVersion, RegistryValueKind.String);

                    var iceVersionMM = string.Format("{0}.{1}", v.Major, v.Minor);
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
            if (project != null)
            {
                iceHome = project.GetEvaluatedProperty(IceHomeValue);
            }

            if (string.IsNullOrEmpty(iceHome))
            {
                object value = Registry.GetValue(IceBuilderKey, IceHomeValue, "");
                iceHome = value == null ? string.Empty : value.ToString();
            }

            return iceHome;
        }

        public void QueueProjectsForBuilding(ICollection<IVsProject> projects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            BuildContext(true);
            OutputPane.Clear();
            OutputPane.Activate();
            foreach (IVsProject p in projects)
            {
                _buildProjects.Add(p);
            }
            BuildNextProject();
        }

        public void BuildDone()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            BuildingProject = null;
            BuildNextProject();
        }

        public void BuildNextProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_buildProjects.Count == 0)
            {
                BuildContext(false);
            }
            else if (!Building && BuildingProject == null)
            {
                IVsProject project = _buildProjects.ElementAt(0);
                ProjectUtil.SaveProject(project);
                ProjectUtil.SetupGenerated(project);
                if (BuildProject(project))
                {
                    _buildProjects.Remove(project);
                }
            }
        }

        public void ReloadProject(IVsProject project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsHierarchy hier = project as IVsHierarchy;
            IVsSolution.GetGuidOfProject(hier, out Guid projectGUID);
            IVsSolution4.UnloadProject(projectGUID, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser);
            IVsSolution4.ReloadProject(ref projectGUID);
        }

        public void InitializeProjects(List<IVsProject> projects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project.IsMSBuildIceBuilderInstalled())
            {
                if (project.IsCppProject())
                {
                    VCUtil.SetupSliceFilter(project.GetDTEProject());
                }
                else
                {
                    if (project is IVsAggregatableProject && !project.HasProjectFlavor(IceBuilderNewFlavor))
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

        protected override async Task InitializeAsync(CancellationToken cancel, IProgress<ServiceProgressData> progress)
        {
            Instance = this;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            AutoBuilding = GetAutoBuilding();

            object value = null;
            {
                Shell = await GetServiceAsync(typeof(IVsShell)) as IVsShell;
                if (Shell == null)
                {
                    throw new PackageInitializationException("Error initializing Shell");
                }
                Shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out value);
                CommandLineMode = (bool)value;
            }

            if (!CommandLineMode)
            {
                InstallDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ResourcesDirectory = Path.Combine(InstallDirectory, "Resources");

                DTE2 = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
                if (DTE2 == null)
                {
                    throw new PackageInitializationException("Error initializing DTE2");
                }

                IVsSolution = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
                if (IVsSolution == null)
                {
                    throw new PackageInitializationException("Error initializing IVsSolution");
                }

                IVsSolution4 = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution4;
                if (IVsSolution4 == null)
                {
                    throw new PackageInitializationException("Error initializing IVsSolution4");
                }

                UIShell = await GetServiceAsync(typeof(SVsUIShell)) as IVsUIShell;
                if (UIShell == null)
                {
                    throw new PackageInitializationException("Error initializing UIShell");
                }

                MonitorSelection = await GetServiceAsync(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
                if (MonitorSelection == null)
                {
                    throw new PackageInitializationException("Error initializing MonitorSelection");
                }

                //
                // Update the InstallDir this was previously used in project imports, but is still usefull if you
                // need to detect the extension install dir.
                //
                string version = DTE2.Version;
                Registry.SetValue(IceBuilderKey, string.Format("InstallDir.{0}", version), InstallDirectory,
                                  RegistryValueKind.String);

                Assembly assembly = null;
                if (version.StartsWith("14.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2015.dll"));
                }
                else if (version.StartsWith("15.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2017.dll"));
                }
                else if (version.StartsWith("16.0"))
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2019.dll"));
                }
                else
                {
                    assembly = Assembly.LoadFrom(Path.Combine(ResourcesDirectory, "IceBuilder.VS2022.dll"));
                }

                var factory = assembly.GetType("IceBuilder.ProjectHelperFactoryI").GetConstructor(new Type[] { }).Invoke(
                    new object[] { }) as IVsProjectHelperFactory;

                VCUtil = factory.VCUtil;
                NuGet = factory.NuGet;
                ProjectHelper = factory.ProjectHelper;

                ProjectFactoryHelperInstance.Init(VCUtil, NuGet, ProjectHelper);

                NuGet.OnNugetBatchEnd(PackageInstalled);

                RunningDocumentTableEventHandler = new RunningDocumentTableEventHandler(
                    await GetServiceAsync(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable);

                Builder = new Builder(await GetServiceAsync(typeof(SVsBuildManagerAccessor)) as IVsBuildManagerAccessor2);

                //
                // Subscribe to solution events.
                //
                SolutionEventHandler = new SolutionEventHandler();
                SolutionEventHandler.BeginTrack();

                DocumentEventHandler = new DocumentEventHandler(
                    await GetServiceAsync(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2);
                DocumentEventHandler.BeginTrack();

                BuildEvents = DTE2.Events.BuildEvents;
                BuildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
                BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            RegisterProjectFactory(new ProjectFactory());
            RegisterProjectFactory(new ProjectFactoryOld());

            value = null;
            IVsSolution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out value);
            if ((bool)value)
            {
                RunningDocumentTableEventHandler.BeginTrack();
                InitializeProjects(DTEUtil.GetProjects());
            }
        }

        private void PackageInstalled()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projects = DTEUtil.GetProjects();
            foreach (IVsProject project in projects)
            {
                InitializeProject(project);
            }
        }

        private void BuildEvents_OnBuildBegin(EnvDTE.vsBuildScope scope, EnvDTE.vsBuildAction action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (action == EnvDTE.vsBuildAction.vsBuildActionBuild ||
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
            catch (Exception ex)
            {
                UnexpectedExceptionWarning(ex);
            }
        }

        private void BuildEvents_OnBuildDone(EnvDTE.vsBuildScope scope, EnvDTE.vsBuildAction action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
                BuildContext(false);
                UnexpectedExceptionWarning(ex);
            }
        }

        private void SetCmdUIContext(Guid context, bool enabled)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ErrorHandler.ThrowOnFailure(MonitorSelection.GetCmdUIContextCookie(ref context, out uint cookie));
            if (cookie != 0)
            {
                ErrorHandler.ThrowOnFailure(MonitorSelection.SetCmdUIContext(cookie, enabled ? 1 : 0));
            }
        }

        private bool IsCmdUIContextActive(Guid context)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ErrorHandler.ThrowOnFailure(MonitorSelection.GetCmdUIContextCookie(ref context, out uint cookie));
            int active = 0;
            if (cookie != 0)
            {
                ErrorHandler.ThrowOnFailure(MonitorSelection.IsCmdUIContextActive(cookie, out active));
            }
            return active != 0;
        }

        private void BuildContext(bool enabled)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SetCmdUIContext(VSConstants.UICONTEXT.SolutionBuilding_guid, enabled);
            if (enabled)
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
            ThreadHelper.ThrowIfNotOnUIThread();
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

                if (!Builder.Build(project, new BuildCallback(project, OutputPane), logger, platform, configuration))
                {
                    BuildingProject = null;
                }
                return BuildingProject != null;
            }
            catch (Exception)
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
            ThreadHelper.ThrowIfNotOnUIThread();
            string sliceCompiler = GetSliceCompilerPath(null, iceHome);
            if (!File.Exists(sliceCompiler))
            {
                string message = string.Format("'{0}' not found, review your Ice installation", sliceCompiler);
                OutputPane.OutputTaskItemString(
                    message,
                    EnvDTE.vsTaskPriority.vsTaskPriorityHigh,
                    "BuildCompile",
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
            process.OutputDataReceived += new DataReceivedEventHandler(reader.AppendData);

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
                    string message = string.Format("Slice compiler `{0}' failed to start(error code {1})",
                        sliceCompiler, process.ExitCode);
                    OutputPane.OutputTaskItemString(
                        message,
                        EnvDTE.vsTaskPriority.vsTaskPriorityHigh,
                        "BuildCompile",
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
                    version = string.Format("{0}.{1}", version.Substring(0, version.Length - 1), 51);
                }
                return version;
            }
            catch (Exception ex)
            {
                string message = string.Format("An exception was thrown when trying to start the Slice compiler\n{0}",
                        ex.ToString());
                OutputPane.OutputTaskItemString(
                    message,
                    EnvDTE.vsTaskPriority.vsTaskPriorityHigh,
                    "BuildCompile",
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
            ThreadHelper.ThrowIfNotOnUIThread();
            string compiler = MSBuildUtils.IsCSharpProject(project) ? "slice2cs.exe" : "slice2cpp.exe";
            if (!string.IsNullOrEmpty(iceHome))
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

            string message = "'" + compiler + "' not found";
            if (!string.IsNullOrEmpty(iceHome))
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
                "BuildCompile",
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
                ThreadHelper.ThrowIfNotOnUIThread();
                if (_outputPane == null)
                {
                    const string vsWindowKindOutput = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";
                    EnvDTE.Window window = DTE2.Windows.Item(vsWindowKindOutput);
                    EnvDTE.OutputWindow outputWindow = window.Object as EnvDTE.OutputWindow;
                    foreach (EnvDTE.OutputWindowPane pane in outputWindow.OutputWindowPanes)
                    {
                        if (pane.Guid.Equals(BuildOutputPaneGUID, StringComparison.CurrentCultureIgnoreCase))
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

        public IVCUtil VCUtil
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
                ThreadHelper.ThrowIfNotOnUIThread();
                object value = Registry.GetValue(
                    Path.Combine("HKEY_CURRENT_USER", DTE.RegistryRoot, "General"),
                    "MSBuildLoggerVerbosity", 2);

                if (!uint.TryParse(value == null ? "2" : value.ToString(), out uint verbosity))
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

        private readonly HashSet<IVsProject> _buildProjects = new HashSet<IVsProject>();

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
