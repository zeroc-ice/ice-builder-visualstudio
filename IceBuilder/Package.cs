// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace IceBuilder;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration("#110", "#112", "6.0.5", IconResourceID = 400)]
[ProvideOptionPage(typeof(IceOptionsPage), "Projects", "Ice Builder", 113, 0, true)]
[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
[Guid(IceBuilderPackageString)]
[ProvideObject(typeof(PropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
[ProvideProjectFactory(typeof(ProjectFactory), "Ice Builder", null, null, null, @"..\Templates\Projects")]
public sealed class Package : AsyncPackage
{
    public IVsShell Shell { get; private set; }
    public EnvDTE80.DTE2 DTE2 { get; private set; }
    public IVsUIShell UIShell { get; private set; }
    public IVsSolution IVsSolution { get; private set; }
    public IVsSolution4 IVsSolution4 { get; private set; }
    private SolutionEventHandler SolutionEventHandler { get; set; }
    public RunningDocumentTableEventHandler RunningDocumentTableEventHandler { get; private set; }
    public IVsMonitorSelection MonitorSelection { get; set; }
    private EnvDTE.BuildEvents BuildEvents { get; set; }

    public static Package Instance { get; private set; }

    public static void UnexpectedExceptionWarning(Exception ex)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            Instance.OutputPane.Activate();
            Instance.OutputPane.OutputString($"The Ice Builder has raised an unexpected exception:\n{ex}");
        }
        catch
        {
        }
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

    public Guid OutputPaneGUID = new("CE9BFDCD-5AFD-4A77-BD40-75E0E1E5162C");

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

    public void InitializeProjects(List<IVsProject> projects)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            _packageInstalled = null;
            projects = DTEUtil.GetProjects();
            foreach (IVsProject project in projects)
            {
                InitializeProject(project);
            }
        }
        finally
        {
            _packageInstalled = PackageInstalled;
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

    public bool CommandLineMode { get; private set; }

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

            // Update the InstallDir this was previously used in project imports, but is still usefull if you
            // need to detect the extension install dir.
            string version = DTE2.Version;
            Registry.SetValue(
                IceBuilderKey,
                string.Format("InstallDir.{0}", version),
                InstallDirectory,
                RegistryValueKind.String);

            var model = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            _installerEvents = model.GetService<IVsPackageInstallerEvents>();
            _installerEvents.PackageReferenceAdded += PackageInstallerEvents_PackageReferenceAdded;
            _packageInstalled = PackageInstalled;

            RunningDocumentTableEventHandler = new RunningDocumentTableEventHandler(
                await GetServiceAsync(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable);

            Builder = new Builder(await GetServiceAsync(typeof(SVsBuildManagerAccessor)) as IVsBuildManagerAccessor2);

            // Subscribe to solution events.
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

        value = null;
        IVsSolution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out value);
        if ((bool)value)
        {
            RunningDocumentTableEventHandler.BeginTrack();
            InitializeProjects(DTEUtil.GetProjects());
        }
    }

    private void PackageInstallerEvents_PackageReferenceAdded(IVsPackageMetadata metadata)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            if (_packageInstalled is not null)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _packageInstalled.Invoke();
            }
        });
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
                // Ensure this runs once for parallel builds.
                if (Building)
                {
                    return;
                }
                Building = true;

                var projects = new List<IVsProject>();
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

                foreach (IVsProject project in projects.Where(project => project.IsMSBuildIceBuilderInstalled()))
                {
                    ProjectUtil.SetupGenerated(project);
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

    private IVsProject BuildingProject { get; set; }

    private bool BuildProject(IVsProject project)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            var logger = new BuildLogger(OutputPane)
            {
                Verbosity = LoggerVerbosity
            };
            BuildingProject = project;
            var dteproject = project.GetDTEProject();
            var activeConfiguration = dteproject.ConfigurationManager.ActiveConfiguration;
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

    private DocumentEventHandler DocumentEventHandler { get; set; }

    private Builder Builder { get; set; }

    private Microsoft.Build.Framework.LoggerVerbosity LoggerVerbosity
    {
        get
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            object value = Registry.GetValue(
                Path.Combine("HKEY_CURRENT_USER", DTE2.DTE.RegistryRoot, "General"),
                "MSBuildLoggerVerbosity", 2);

            if (!uint.TryParse(value == null ? "2" : value.ToString(), out uint verbosity))
            {
                verbosity = 2;
            }

            return verbosity switch
            {
                0 => Microsoft.Build.Framework.LoggerVerbosity.Quiet,
                1 => Microsoft.Build.Framework.LoggerVerbosity.Minimal,
                3 => Microsoft.Build.Framework.LoggerVerbosity.Detailed,
                4 => Microsoft.Build.Framework.LoggerVerbosity.Diagnostic,
                _ => Microsoft.Build.Framework.LoggerVerbosity.Normal,
            };
        }
    }

    private string InstallDirectory { get; set; }
    public static string ResourcesDirectory { get; private set; }
    private bool Building { get; set; }

    private readonly HashSet<IVsProject> _buildProjects = [];
    private Action _packageInstalled;
    private IVsPackageInstallerEvents _installerEvents;

    public static readonly string IceBuilderKey = @"HKEY_CURRENT_USER\Software\ZeroC\IceBuilder";
    public static readonly string IceAutoBuilding = "IceAutoBuilding";
    public const string IceBuilderPackageString = "0CEF9F9D-FA1F-45D0-9D1E-BBD2A86D5F62";
    public const string IceBuilderNewFlavorGuid = "28993779-3132-408A-BCB0-1D78225F4824";
    public const string IceBuilderNewFlavor = "{" + IceBuilderNewFlavorGuid + "}";
}
