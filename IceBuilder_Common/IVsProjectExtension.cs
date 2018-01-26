// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

namespace IceBuilder
{
    public static class IVsProjectExtension
    {
        private static void EnsureIsCheckout(IVsProject project)
        {
            EnsureIsCheckout(project.GetDTEProject(), project.GetProjectFullPath());
        }

        private static void EnsureIsCheckout(EnvDTE.Project project, string path)
        {
            var sc = project.DTE.SourceControl;
            if(sc != null)
            {
                if(sc.IsItemUnderSCC(path) && !sc.IsItemCheckedOut(path))
                {
                    sc.CheckOutItem(path);
                }
            }
        }

        public static EnvDTE.Project GetDTEProject(this IVsProject project)
        {
            IVsHierarchy hierarchy = project as IVsHierarchy;
            object obj = null;
            if (hierarchy != null)
            {
                hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            }
            var dteproject = obj as EnvDTE.Project;
            EnsureIsCheckout(dteproject, project.GetProjectFullPath());
            return dteproject;
        }

        public static Microsoft.Build.Evaluation.Project GetMSBuildProject(this IVsProject project, bool cached = false)
        {
            EnsureIsCheckout(project);
            return MSProjectExtension.LoadedProject(project.GetProjectFullPath(), cached);
        }

        public static string GetProjectBaseDirectory(this IVsProject project)
        {
            string fullPath;
            ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out fullPath));
            return Path.GetFullPath(Path.GetDirectoryName(fullPath));
        }

        public static string GetProjectFullPath(this IVsProject project)
        {
            try
            {
                string fullPath;
                ErrorHandler.ThrowOnFailure(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out fullPath));
                return Path.GetFullPath(fullPath);
            }
            catch(NotImplementedException)
            {
                return string.Empty;
            }
        }
    }
}
