// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace IceBuilder;

public class SolutionEventHandler : IVsSolutionEvents3, IVsSolutionLoadEvents
{
    public void BeginTrack() =>
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Package.Instance.IVsSolution.AdviseSolutionEvents(this, out _cookie);
        });

    public void EndTrack() =>
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Package.Instance.IVsSolution.UnadviseSolutionEvents(_cookie);
        });

    public int OnAfterBackgroundSolutionLoadComplete()
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                Package.Instance.RunningDocumentTableEventHandler.BeginTrack();
                Package.Instance.InitializeProjects(DTEUtil.GetProjects());
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        });
        return 0;
    }

    public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch) => 0;

    public int OnBeforeBackgroundSolutionLoadBegins() => 0;

    public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch) => 0;

    public int OnBeforeOpenSolution(string pszSolutionFilename) => 0;

    public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
    {
        pfShouldDelayLoadToNextIdle = false;
        return 0;
    }

    public int OnAfterCloseSolution(object pUnkReserved)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                Package.Instance.RunningDocumentTableEventHandler.EndTrack();
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        });
        return 0;
    }

    public int OnAfterClosingChildren(IVsHierarchy child) => 0;

    public int OnAfterLoadProject(IVsHierarchy hierarchyOld, IVsHierarchy hierarchyNew)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                var project = hierarchyNew as IVsProject;
                if (project != null)
                {
                    Package.Instance.InitializeProjects(new List<IVsProject>(new IVsProject[] { project }));
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
            }
        });
        return 0;
    }

    public int OnAfterMergeSolution(object pUnkReserved) => 0;
    public int OnAfterOpeningChildren(IVsHierarchy hierarchy) => 0;
    public int OnAfterOpenProject(IVsHierarchy hierarchy, int fAdded) => 0;
    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => 0;
    public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => 0;
    public int OnBeforeCloseSolution(object pUnkReserved) => 0;
    public int OnBeforeClosingChildren(IVsHierarchy pHierarchy) => 0;
    public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy) => 0;
    public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => 0;

    public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
    {
        pfCancel = 0;
        return 0;
    }

    public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
    {
        pfCancel = 0;
        return 0;
    }

    public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
    {
        pfCancel = 0;
        return 0;
    }

    uint _cookie;
}
