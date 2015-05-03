// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
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
        Dispatcher UIDispatcher
        {
            get;
            set;
        }

        System.Threading.Thread UIThread
        {
            get;
            set;
        }

        public RunningDocumentTableEventHandler()
        {
            UIDispatcher = Dispatcher.CurrentDispatcher;
            UIThread = System.Threading.Thread.CurrentThread;
        }

        public void BeginTrack()
        {
            Package.Instance.IVsRunningDocumentTable.AdviseRunningDocTableEvents(this, out _cookie);
        }

        public void EndTrack()
        {
            Package.Instance.IVsRunningDocumentTable.UnadviseRunningDocTableEvents(_cookie);
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
            ProjectItem item = GetProjectItemFromDocumentCookie(docCookie);
            if(item != null )
            {
                if (item.ContainingProject != null && DTEUtil.IsIceBuilderEnabled(item.ContainingProject) &&
                   !String.IsNullOrEmpty(item.Name) && ProjectUtil.IsSliceFileName(item.Name))
                {
                    Package.Instance.QueueProjectsForBuilding(
                        new List<EnvDTE.Project>(new EnvDTE.Project[]{item.ContainingProject}));
                }
            }
            return 0;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if(fFirstShow != 0)
            {
                ProjectItem item = GetProjectItemFromDocumentCookie(docCookie);
                if(ProjectUtil.IsGeneratedItem(item))
                {
                    item.Document.ReadOnly = true;
                }
            }
            return 0;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
                                              uint dwEditLocksRemaining)
        {
            return 0;
        }

        private EnvDTE.ProjectItem GetProjectItemFromDocumentCookie(uint cookie)
        {
            uint pgrfRDTFlags;
            uint pdwReadLocks;
            uint pdwEditLocks;
            string pbstrMkDocument;
            IVsHierarchy ppHier;
            uint pitemid;
            IntPtr ppunkDocData;

            if (Package.Instance.IVsRunningDocumentTable.GetDocumentInfo(cookie,
                                                                         out pgrfRDTFlags,
                                                                         out pdwReadLocks,
                                                                         out pdwEditLocks,
                                                                         out pbstrMkDocument,
                                                                         out ppHier,
                                                                         out pitemid,
                                                                         out ppunkDocData) == VSConstants.S_OK)
            {
                return DTEUtil.GetProjectItem(ppHier, pitemid);
            }
            return null;
        }

        private HashSet<EnvDTE.Project> _buildProjects = new HashSet<EnvDTE.Project>();
        private uint _cookie;
    }

    public class DocumentEventHandler : IVsTrackProjectDocumentsEvents2
    {
        public void BeginTrack()
        {
            Package.Instance.IVsTrackProjectDocuments2.AdviseTrackProjectDocumentsEvents(this, out _cookie);
        }

        public void EndTrack()
        {
            Package.Instance.IVsTrackProjectDocuments2.UnadviseTrackProjectDocumentsEvents(_cookie);
        }

        #region IVsTrackProjectDocumentsEvents2
        public int OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects,
                                           int[] rgFirstIndices, string[] rgpszMkDocuments,
                                           VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return 0;
        }

        public int OnAfterAddFilesEx(int projectsLength, int filesLength, IVsProject[] projects, int[] indices,
                                     string[] paths, VSADDFILEFLAGS[] rgFlags)
        {
            for (int i = 0; i < projectsLength; ++i)
            {
                EnvDTE.Project project = DTEUtil.GetProject(projects[i] as IVsHierarchy);
                int j = indices[i]; 
                int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;

                for (; j < k; ++j)
                {
                    if(ProjectUtil.IsSliceFileName(paths[j]))
                    {
                        ProjectUtil.AddItems(project, ProjectUtil.GetGeneratedFiles(project, paths[j]));
                    }
                }
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
            for(int i = 0; i < projectsLength; ++i)
            {
                EnvDTE.Project project = DTEUtil.GetProject(projects[i] as IVsHierarchy);
                if (DTEUtil.IsIceBuilderEnabled(project))
                {
                    int j = indices[i];
                    int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;
                    for (; j < k; ++j)
                    {
                        if(ProjectUtil.IsSliceFileName(names[j]))
                        {
                            ProjectUtil.DeleteItems(project, ProjectUtil.GetGeneratedFiles(project, names[i]));
                        }
                    }
                }
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
            for (int i = 0; i < projectsLength; ++i)
            {
                EnvDTE.Project project = DTEUtil.GetProject(projects[i] as IVsHierarchy);
                int j = indices[i];
                int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;
                for (; j < k; ++j)
                {
                    ProjectUtil.DeleteItems(project, ProjectUtil.GetGeneratedFiles(project, oldNames[j]));
                    if(ProjectUtil.IsSliceFileName(newNames[j]))
                    {
                        ProjectUtil.AddItems(project, ProjectUtil.GetGeneratedFiles(project, newNames[j]));
                    }
                }
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

        public int OnQueryAddFiles(IVsProject p, int length, string[] files,
                                   VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult,
                                   VSQUERYADDFILERESULTS[] rgResults)
        {
            EnvDTE.Project project = DTEUtil.GetProject(p as IVsHierarchy);
            if (DTEUtil.IsIceBuilderEnabled(project))
            {
                for (int i = 0; i < length; ++i)
                {
                    String path = files[i];
                    if(Path.GetExtension(path).Equals(".ice"))
                    {
                        if (!ProjectUtil.CheckGenerateFileIsValid(project, path))
                        {
                            pSummaryResult[i] = VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddNotOK;
                        }
                    }
                }
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

        public int OnQueryRenameFiles(IVsProject ivsProject, int filesLength, string[] oldNames, string[] newNames,
                                      VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult,
                                      VSQUERYRENAMEFILERESULTS[] rgResults)
        {

            EnvDTE.Project project = DTEUtil.GetProject(ivsProject as IVsHierarchy);
            if(DTEUtil.IsIceBuilderEnabled(project))
            {
                for(int i = 0; i < filesLength; ++i)
                {
                    if(Path.GetExtension(oldNames[i]).Equals(".ice") && 
                       Path.GetExtension(newNames[i]).Equals(".ice"))
                    {

                        if(!ProjectUtil.CheckGenerateFileIsValid(project, newNames[i]))
                        {
                            rgResults[i] = VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameNotOK;
                        }
                    }
                }
            }
            return 0;
        }
        #endregion IVsTrackProjectDocumentsEvents2

        
        private uint _cookie = 0;
    }
}
