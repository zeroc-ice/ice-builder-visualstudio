// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public static class IVsProjectExtension
    {
        public static EnvDTE.Project GetDTEProject(this IVsProject project)
        {
            IVsHierarchy hierarchy = project as IVsHierarchy;
            object obj = null;
            if (hierarchy != null)
            {
                hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            }
            return obj as EnvDTE.Project;
        }
    }
}
