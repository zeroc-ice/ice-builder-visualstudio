// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

using System.Windows.Forms;
using System.Windows.Threading;
using System.IO;

namespace IceBuilder
{
    public class RunningDocumentTableEventHandler : IVsRunningDocTableEvents2
    {
        public RunningDocumentTableEventHandler(IVsRunningDocumentTable runningDocumentTable)
        {
            RunningDocumentTable = runningDocumentTable;
        }

        public void BeginTrack()
        {
            RunningDocumentTable.AdviseRunningDocTableEvents(this, out _cookie);
        }

        public void EndTrack()
        {
            RunningDocumentTable.UnadviseRunningDocTableEvents(_cookie);
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return 0;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld,
                                            string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew,
                                            string pszMkDocumentNew)
        {
            return 0;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return 0;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
                                            uint dwEditLocksRemaining)
        {
            return 0;
        }

        public int OnAfterSave(uint docCookie)
        {
            try
            {
                if (Package.Instance.AutoBuilding)
                {
                    IVsProject project = null;
                    uint item = 0;
                    String path = null;
                    GetDocumentInfo(docCookie, ref project, ref item, ref path);
                    if (DTEUtil.IsIceBuilderEnabled(project) != IceBuilderProjectType.None &&
                        ProjectUtil.IsSliceFileName(path))
                    {
                        Package.Instance.QueueProjectsForBuilding(new List<IVsProject>(new IVsProject[] { project }));
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            try
            {
                if(fFirstShow != 0)
                {
                    IVsProject project = null;
                    uint item = 0;
                    String path = null;
                    GetDocumentInfo(docCookie, ref project, ref item, ref path);
                    if(project != null && !String.IsNullOrEmpty(path))
                    {
                        if(Package.Instance.FileTracker.Contains(ProjectUtil.GetProjectFullPath(project), path))
                        {
                            EnvDTE.ProjectItem i = ProjectUtil.GetProjectItem(project, item);
                            if (i != null)
                            {
                                i.Document.ReadOnly = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
                                              uint dwEditLocksRemaining)
        {
            return 0;
        }

        private void GetDocumentInfo(uint cookie, ref IVsProject project, ref uint item, ref string path)
        {
            uint pgrfRDTFlags;
            uint pdwReadLocks;
            uint pdwEditLocks;
            string pbstrMkDocument;
            IVsHierarchy ppHier;
            uint pitemid;
            IntPtr ppunkDocData;

            ErrorHandler.ThrowOnFailure(RunningDocumentTable.GetDocumentInfo(
                    cookie,
                    out pgrfRDTFlags,
                    out pdwReadLocks,
                    out pdwEditLocks,
                    out pbstrMkDocument,
                    out ppHier,
                    out pitemid,
                    out ppunkDocData));

            project = ppHier as IVsProject;
            path = pbstrMkDocument;
            item = pitemid;
        }

        IVsRunningDocumentTable RunningDocumentTable
        {
            get;
            set;
        }

        private HashSet<EnvDTE.Project> _buildProjects = new HashSet<EnvDTE.Project>();
        private uint _cookie;
    }

    public class DocumentEventHandler : IVsTrackProjectDocumentsEvents2
    {
        public DocumentEventHandler(IVsTrackProjectDocuments2 trackProjectDocuments2)
        {
            TrackProjectDocuments2 = trackProjectDocuments2;
        }

        public void BeginTrack()
        {
            TrackProjectDocuments2.AdviseTrackProjectDocumentsEvents(this, out _cookie);
        }

        public void EndTrack()
        {
            TrackProjectDocuments2.UnadviseTrackProjectDocumentsEvents(_cookie);
        }

        #region IVsTrackProjectDocumentsEvents2
        public int OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects,
                                           int[] rgFirstIndices, string[] rgpszMkDocuments,
                                           VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return 0;
        }

        public int OnAfterAddFilesEx(int projectsLength, int filesLength, IVsProject[] projects, int[] indices,
                                     string[] names, VSADDFILEFLAGS[] rgFlags)
        {
            try
            {
                for (int i = 0; i < projectsLength; ++i)
                {
                    IVsProject project = projects[i];
                    IceBuilderProjectType projectType = DTEUtil.IsIceBuilderEnabled(project);
                    if (projectType != IceBuilderProjectType.None)
                    {
                        int j = indices[i];
                        int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;
                        for (; j < k; ++j)
                        {
                            String path = names[i];
                            if (ProjectUtil.IsSliceFileName(path))
                            {
                                ProjectUtil.SetupGenerated(project, projectType);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects,
                                            int[] rgFirstIndices, string[] rgpszMkDocuments,
                                            VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return 0;
        }

        public int OnAfterRemoveFiles(int projectsLength, int filesLength, IVsProject[] projects, int[] indices,
                                      string[] names, VSREMOVEFILEFLAGS[] rgFlags)
        {
            try
            {
                for (int i = 0; i < projectsLength; ++i)
                {
                    IVsProject project = projects[i];
                    IceBuilderProjectType projectType = DTEUtil.IsIceBuilderEnabled(project);
                    if (projectType != IceBuilderProjectType.None)
                    {
                        int j = indices[i];
                        int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;
                        for (; j < k; ++j)
                        {
                            String path = names[i];
                            if (ProjectUtil.IsSliceFileName(path))
                            {
                                ProjectUtil.SetupGenerated(project, projectType);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices,
                                            string[] rgszMkOldNames, string[] rgszMkNewNames,
                                            VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return 0;
        }

        public int OnAfterRenameFiles(int projectsLength, int filesLength, IVsProject[] projects, int[] indices,
                                      string[] oldNames, string[] newNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            try
            {
                for (int i = 0; i < projectsLength; ++i)
                {
                    IVsProject project = projects[i];
                    IceBuilderProjectType projectType = DTEUtil.IsIceBuilderEnabled(project);
                    if (projectType != IceBuilderProjectType.None)
                    {
                        int j = indices[i];
                        int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;
                        for (; j < k; ++j)
                        {
                            String oldPath = oldNames[i];
                            String newPath = newNames[j];
                            if (ProjectUtil.IsSliceFileName(oldPath) || ProjectUtil.IsSliceFileName(newPath))
                            {
                                ProjectUtil.SetupGenerated(project, projectType);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices,
                                           string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return 0;
        }

        public int OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments,
                                         VSQUERYADDDIRECTORYFLAGS[] rgFlags,
                                         VSQUERYADDDIRECTORYRESULTS[] pSummaryResult,
                                         VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return 0;
        }

        public int OnQueryAddFiles(IVsProject project, int length, string[] files,
                                   VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult,
                                   VSQUERYADDFILERESULTS[] rgResults)
        {
            try
            {
                if(files.Any(f => ProjectUtil.IsSliceFileName(f)))
                {
                    IceBuilderProjectType projectType = DTEUtil.IsIceBuilderEnabled(project);
                    if (projectType != IceBuilderProjectType.None)
                    {
                        for (int i = 0; i < length; ++i)
                        {
                            if (ProjectUtil.IsSliceFileName(files[i]))
                            {
                                if (!ProjectUtil.CheckGenerateFileIsValid(project, projectType, files[i]))
                                {
                                    pSummaryResult[i] = VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddNotOK;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }

        public int OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments,
                                            VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags,
                                            VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult,
                                            VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return 0;
        }

        public int OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments,
                                      VSQUERYREMOVEFILEFLAGS[] rgFlags,
                                      VSQUERYREMOVEFILERESULTS[] pSummaryResult,
                                      VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return 0;
        }

        public int OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames,
                                            string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags,
                                            VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult,
                                            VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return 0;
        }

        public int OnQueryRenameFiles(IVsProject project, int filesLength, string[] oldNames, string[] newNames,
                                      VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult,
                                      VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            try
            {
                IceBuilderProjectType projectType = DTEUtil.IsIceBuilderEnabled(project);
                if (projectType != IceBuilderProjectType.None)
                {
                    for(int i = 0; i < filesLength; ++i)
                    {
                        if(ProjectUtil.IsSliceFileName(newNames[i]))
                        {

                            if(!ProjectUtil.CheckGenerateFileIsValid(project, projectType, newNames[i]))
                            {
                                rgResults[i] = VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameNotOK;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Package.UnexpectedExceptionWarning(ex);
                throw;
            }
            return 0;
        }
        #endregion IVsTrackProjectDocumentsEvents2

        IVsTrackProjectDocuments2 TrackProjectDocuments2
        {
            get;
            set;
        }
        
        private uint _cookie = 0;
    }
}
