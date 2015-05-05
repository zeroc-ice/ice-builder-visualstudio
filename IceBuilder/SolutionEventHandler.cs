// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class SolutionEventHandler : IVsSolutionEvents3, IVsSolutionLoadEvents
    {
        public void BeginTrack()
        {
            Package.Instance.IVsSolution.AdviseSolutionEvents(this, out _cookie);
        }

        public void EndTrack()
        {
            Package.Instance.IVsSolution.UnadviseSolutionEvents(_cookie);
        }

        #region IVsSolutionLoadEvents
        public int OnAfterBackgroundSolutionLoadComplete()
        {
            Package.Instance.RunningDocumentTableEventHandler.BeginTrack();
            Package.Instance.InitializeProjects();
            return 0;
        }

        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return 0;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            return 0;
        }

        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return 0;
        }

        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            return 0;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return 0;
        }
        #endregion

        #region IVsSolutionEvents3
        public int OnAfterCloseSolution(Object pUnkReserved)
        {
            try
            {
                Package.Instance.RunningDocumentTableEventHandler.EndTrack();
                Package.Instance.FileTracker.Clear();
            }
            catch(Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnAfterClosingChildren(IVsHierarchy child)
        {
            return 0;
        }

        public int OnAfterLoadProject(IVsHierarchy hierarchyOld, IVsHierarchy hierarchyNew)
        {
            try
            {
                EnvDTE.Project project = DTEUtil.GetProject(hierarchyNew);
                if (project != null)
                {
                    Package.Instance.InitializeProjects(new List<EnvDTE.Project>(new EnvDTE.Project[] { project }));
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnAfterMergeSolution(Object pUnkReserved)
        {
            return 0;
        }

        public int OnAfterOpeningChildren(IVsHierarchy hierarchy)
        {
            return 0;
        }

        public int OnAfterOpenProject(IVsHierarchy hierarchy, int fAdded)
        {
            return 0;
        }

        public int OnAfterOpenSolution(Object pUnkReserved, int fNewSolution)
        {
            return 0;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            try
            {
                EnvDTE.Project project = DTEUtil.GetProject(pHierarchy);
                if (project != null)
                {
                    Package.Instance.FileTracker.Remove(project);
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnBeforeCloseSolution(Object pUnkReserved)
        {
            return 0;
        }

        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy)
        {
            return 0;
        }

        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy)
        {
            return 0;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            try
            {
                EnvDTE.Project project = DTEUtil.GetProject(pRealHierarchy);
                if (project != null)
                {
                    Package.Instance.FileTracker.Remove(project);
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            pfCancel = 0;
            return 0;
        }

        public int OnQueryCloseSolution(Object pUnkReserved, ref int pfCancel)
        {
            pfCancel = 0;
            return 0;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            pfCancel = 0;
            return 0;
        }
        #endregion IVsSolutionEvents3

        uint _cookie;
    }
}
