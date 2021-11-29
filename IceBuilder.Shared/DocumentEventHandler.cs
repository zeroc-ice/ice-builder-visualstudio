// Copyright (c) ZeroC, Inc. All rights reserved.

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IceBuilder
{
    public class RunningDocumentTableEventHandler : IVsRunningDocTableEvents2
    {
        public RunningDocumentTableEventHandler(IVsRunningDocumentTable runningDocumentTable) =>
            RunningDocumentTable = runningDocumentTable;

        public void BeginTrack()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                RunningDocumentTable.AdviseRunningDocTableEvents(this, out _cookie);
            });
        }

        public void EndTrack() =>
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                RunningDocumentTable.UnadviseRunningDocTableEvents(_cookie);
            });

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => 0;

        public int OnAfterAttributeChangeEx(
            uint docCookie, uint grfAttribs,
            IVsHierarchy pHierOld,
            uint itemidOld,
            string pszMkDocumentOld,
            IVsHierarchy pHierNew,
            uint itemidNew,
            string pszMkDocumentNew) => 0;

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => 0;

        public int OnAfterFirstDocumentLock(
            uint docCookie,
            uint dwRDTLockType,
            uint dwReadLocksRemaining,
            uint dwEditLocksRemaining) => 0;

        public int OnAfterSave(uint docCookie)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    if (Package.Instance.AutoBuilding)
                    {
                        IVsProject project = null;
                        uint item = 0;
                        string path = null;
                        GetDocumentInfo(docCookie, ref project, ref item, ref path);
                        if (ProjectUtil.IsSliceFileName(path) && project.IsMSBuildIceBuilderInstalled())
                        {
                            Package.Instance.QueueProjectsForBuilding(new List<IVsProject>(new IVsProject[] { project }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Package.UnexpectedExceptionWarning(ex);
                }
            });
            return 0;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    if (fFirstShow != 0)
                    {
                        IVsProject project = null;
                        uint itemref = 0;
                        string path = null;
                        GetDocumentInfo(docCookie, ref project, ref itemref, ref path);
                        if (project != null && !string.IsNullOrEmpty(path) &&
                           (path.EndsWith(".cs", StringComparison.CurrentCultureIgnoreCase) ||
                            path.EndsWith(".cpp", StringComparison.CurrentCultureIgnoreCase) ||
                            path.EndsWith(".h", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            if (project.IsIceBuilderGeneratedItem(path))
                            {
                                ProjectItem item = project.GetProjectItem(itemref);
                                if (item != null)
                                {
                                    item.Document.ReadOnly = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Could happen with some document types
                }
            });
            return 0;
        }

        public int OnBeforeLastDocumentUnlock(
            uint docCookie,
            uint dwRDTLockType,
            uint dwReadLocksRemaining,
            uint dwEditLocksRemaining) => 0;

        private void GetDocumentInfo(uint cookie, ref IVsProject project, ref uint item, ref string path)
        {
            string pbstrMkDocument = "";
            IVsProject pProject = null;
            uint pitemid = 0;

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ErrorHandler.ThrowOnFailure(RunningDocumentTable.GetDocumentInfo(
                    cookie,
                    out uint pgrfRDTFlags,
                    out uint pdwReadLocks,
                    out uint pdwEditLocks,
                    out pbstrMkDocument,
                    out IVsHierarchy ppHier,
                    out pitemid,
                    out IntPtr ppunkDocData));
                pProject = ppHier as IVsProject;
            });

            project = pProject;
            path = pbstrMkDocument;
            item = pitemid;
        }

        IVsRunningDocumentTable RunningDocumentTable { get; set; }

        private uint _cookie;
    }

    public class DocumentEventHandler : IVsTrackProjectDocumentsEvents2
    {
        public DocumentEventHandler(IVsTrackProjectDocuments2 trackProjectDocuments2) =>
            TrackProjectDocuments2 = trackProjectDocuments2;

        public void BeginTrack() =>
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                TrackProjectDocuments2.AdviseTrackProjectDocumentsEvents(this, out _cookie);
            });

        public void EndTrack() =>
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                TrackProjectDocuments2.UnadviseTrackProjectDocumentsEvents(_cookie);
            });

        public int OnAfterAddDirectoriesEx(
            int cProjects,
            int cDirectories,
            IVsProject[] rgpProjects,
            int[] rgFirstIndices,
            string[] rgpszMkDocuments,
            VSADDDIRECTORYFLAGS[] rgFlags) => 0;

        public int OnAfterAddFilesEx(
            int projectsLength,
            int filesLength,
            IVsProject[] projects,
            int[] indices,
            string[] names,
            VSADDFILEFLAGS[] rgFlags)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    for (int i = 0; i < projectsLength; ++i)
                    {
                        IVsProject project = projects[i];
                        if (project.IsMSBuildIceBuilderInstalled())
                        {
                            int j = indices[i];
                            int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;
                            for (; j < k; ++j)
                            {
                                string path = names[i];
                                if (ProjectUtil.IsSliceFileName(path))
                                {
                                    // Ensure the .ice file item has SliceCompile ItemType
                                    var projectItem = project.GetProjectItem(path);
                                    if (projectItem != null)
                                    {
                                        var property = projectItem.Properties.Item("ItemType");
                                        if (property != null && !property.Value.Equals("SliceCompile"))
                                        {
                                            project.EnsureIsCheckout();
                                            property.Value = "SliceCompile";
                                            ProjectUtil.AddGeneratedFiles(project, path);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore, this could happen if the project is reloaded
                }
                catch (Exception ex)
                {
                    Package.UnexpectedExceptionWarning(ex);
                }
            });
            return 0;
        }

        public int OnAfterRemoveDirectories(
            int cProjects,
            int cDirectories,
            IVsProject[] rgpProjects,
            int[] rgFirstIndices,
            string[] rgpszMkDocuments,
            VSREMOVEDIRECTORYFLAGS[] rgFlags) => 0;

        public int OnAfterRemoveFiles(
            int projectsLength,
            int filesLength,
            IVsProject[] projects,
            int[] indices,
            string[] names,
            VSREMOVEFILEFLAGS[] rgFlags)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    for (int i = 0; i < projectsLength; ++i)
                    {
                        IVsProject project = projects[i];
                        if (project.IsMSBuildIceBuilderInstalled())
                        {
                            int j = indices[i];
                            int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;
                            for (; j < k; ++j)
                            {
                                string path = names[i];
                                if (ProjectUtil.IsSliceFileName(path))
                                {
                                    ProjectUtil.SetupGenerated(project);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Package.UnexpectedExceptionWarning(ex);
                }
            });
            return 0;
        }

        public int OnAfterRenameDirectories(
            int cProjects,
            int cDirs,
            IVsProject[] rgpProjects,
            int[] rgFirstIndices,
            string[] rgszMkOldNames,
            string[] rgszMkNewNames,
            VSRENAMEDIRECTORYFLAGS[] rgFlags) => 0;

        public int OnAfterRenameFiles(
            int projectsLength,
            int filesLength,
            IVsProject[] projects,
            int[] indices,
            string[] oldNames,
            string[] newNames,
            VSRENAMEFILEFLAGS[] rgFlags)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    for (int i = 0; i < projectsLength; ++i)
                    {
                        IVsProject project = projects[i];
                        if (project.IsMSBuildIceBuilderInstalled())
                        {
                            int j = indices[i];
                            int k = i < (projectsLength - 1) ? indices[i + 1] : filesLength;
                            for (; j < k; ++j)
                            {
                                string oldPath = oldNames[i];
                                string newPath = newNames[j];
                                if (ProjectUtil.IsSliceFileName(oldPath) || ProjectUtil.IsSliceFileName(newPath))
                                {
                                    ProjectUtil.SetupGenerated(project);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Package.UnexpectedExceptionWarning(ex);
                }
            });
            return 0;
        }

        public int OnAfterSccStatusChanged(
            int cProjects,
            int cFiles,
            IVsProject[] rgpProjects,
            int[] rgFirstIndices,
            string[] rgpszMkDocuments,
            uint[] rgdwSccStatus) => 0;

        public int OnQueryAddDirectories(
            IVsProject pProject,
            int cDirectories,
            string[] rgpszMkDocuments,
            VSQUERYADDDIRECTORYFLAGS[] rgFlags,
            VSQUERYADDDIRECTORYRESULTS[] pSummaryResult,
            VSQUERYADDDIRECTORYRESULTS[] rgResults) => 0;

        public int OnQueryAddFiles(
            IVsProject project,
            int length, string[] files,
            VSQUERYADDFILEFLAGS[] rgFlags,
            VSQUERYADDFILERESULTS[] pSummaryResult,
            VSQUERYADDFILERESULTS[] rgResults)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    if (files.Any(f => ProjectUtil.IsSliceFileName(f)))
                    {
                        if (project.IsMSBuildIceBuilderInstalled())
                        {
                            for (int i = 0; i < length; ++i)
                            {
                                if (ProjectUtil.IsSliceFileName(files[i]))
                                {
                                    if (!ProjectUtil.CheckGenerateFileIsValid(project, files[i]))
                                    {
                                        if (rgResults != null)
                                        {
                                            rgResults[i] = VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddNotOK;
                                        }
                                        pSummaryResult[0] = VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddNotOK;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Package.UnexpectedExceptionWarning(ex);
                }
            });
            return 0;
        }

        public int OnQueryRemoveDirectories(
            IVsProject pProject,
            int cDirectories,
            string[] rgpszMkDocuments,
            VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags,
            VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult,
            VSQUERYREMOVEDIRECTORYRESULTS[] rgResults) => 0;

        public int OnQueryRemoveFiles(
            IVsProject pProject,
            int cFiles,
            string[] rgpszMkDocuments,
            VSQUERYREMOVEFILEFLAGS[] rgFlags,
            VSQUERYREMOVEFILERESULTS[] pSummaryResult,
            VSQUERYREMOVEFILERESULTS[] rgResults) => 0;

        public int OnQueryRenameDirectories(
            IVsProject pProject,
            int cDirs,
            string[] rgszMkOldNames,
            string[] rgszMkNewNames,
            VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags,
            VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult,
            VSQUERYRENAMEDIRECTORYRESULTS[] rgResults) => 0;

        public int OnQueryRenameFiles(
            IVsProject project,
            int filesLength,
            string[] oldNames,
            string[] newNames,
            VSQUERYRENAMEFILEFLAGS[] rgFlags,
            VSQUERYRENAMEFILERESULTS[] pSummaryResult,
            VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    if (project.IsMSBuildIceBuilderInstalled())
                    {
                        for (int i = 0; i < filesLength; ++i)
                        {
                            if (ProjectUtil.IsSliceFileName(newNames[i]))
                            {

                                if (!ProjectUtil.CheckGenerateFileIsValid(project, newNames[i]))
                                {
                                    if (rgResults != null)
                                    {
                                        rgResults[i] = VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameNotOK;
                                    }
                                    pSummaryResult[0] = VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameNotOK;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Package.UnexpectedExceptionWarning(ex);
                }
            });
            return 0;
        }

        IVsTrackProjectDocuments2 TrackProjectDocuments2 { get; set; }

        private uint _cookie = 0;
    }
}
