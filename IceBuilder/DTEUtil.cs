// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IceBuilder;

public class DTEUtil
{
    public static uint GetItemId(object value)
    {
        if (value == null)
        {
            return VSConstants.VSITEMID_NIL;
        }
        if (value is int)
        {
            return (uint)(int)value;
        }
        if (value is uint)
        {
            return (uint)value;
        }
        if (value is short)
        {
            return (uint)(short)value;
        }
        if (value is long)
        {
            return (uint)(long)value;
        }
        return VSConstants.VSITEMID_NIL;
    }

    public static List<IVsProject> GetProjects()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        Guid guid = Guid.Empty;
        uint flags = (uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS;
        ErrorHandler.ThrowOnFailure(
            Package.Instance.IVsSolution.GetProjectEnum(flags, guid, out IEnumHierarchies enumHierarchies));

        var projects = new List<IVsProject>();

        IVsHierarchy[] hierarchies = new IVsHierarchy[1];
        uint sz;
        do
        {
            ErrorHandler.ThrowOnFailure(enumHierarchies.Next(1, hierarchies, out sz));
            if (sz > 0)
            {
                if (hierarchies[0] is IVsProject project)
                {
                    projects.Add(project);
                }
            }
        }
        while (sz == 1);
        return projects;
    }

    public static void GetSubProjects(IVsProject p, ref List<IVsProject> projects)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        IVsHierarchy h = p as IVsHierarchy;
        // Get the first visible child node
        int result = h.GetProperty(
            VSConstants.VSITEMID_ROOT,
            (int)__VSHPROPID.VSHPROPID_FirstVisibleChild,
            out object value);
        while (ErrorHandler.Succeeded(result))
        {
            uint child = GetItemId(value);
            if (child == VSConstants.VSITEMID_NIL)
            {
                // No more nodes
                break;
            }
            else
            {
                GetSubProjects(h, child, ref projects);

                // Get the next visible sibling node
                result = h.GetProperty(child, (int)__VSHPROPID.VSHPROPID_NextVisibleSibling, out value);
            }
        }
    }

    public static void GetSubProjects(IVsHierarchy h, uint itemId, ref List<IVsProject> projects)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        Guid nestedGuid = typeof(IVsHierarchy).GUID;
        int result = h.GetNestedHierarchy(itemId, ref nestedGuid, out IntPtr nestedValue, out uint nestedId);
        if (ErrorHandler.Succeeded(result) && nestedValue != IntPtr.Zero && nestedId == VSConstants.VSITEMID_ROOT)
        {
            // Get the nested hierachy
            Marshal.Release(nestedValue);
            if (Marshal.GetObjectForIUnknown(nestedValue) is IVsProject project)
            {
                projects.Add(project);
                GetSubProjects(project, ref projects);
            }
        }
    }

    public static IVsProject GetSelectedProject()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        IVsHierarchy hier = null;
        var sp = new ServiceProvider(
            Package.Instance.DTE2.DTE as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

        // There isn't an open project.
        if (sp.GetService(typeof(IVsMonitorSelection)) is IVsMonitorSelection selectionMonitor)
        {
            _ = ErrorHandler.ThrowOnFailure(
                selectionMonitor.GetCurrentSelection(out IntPtr ppHier, out _, out _, out IntPtr ppSC));

            if (ppHier != IntPtr.Zero)
            {
                hier = (IVsHierarchy)Marshal.GetObjectForIUnknown(ppHier);
                Marshal.Release(ppHier);
            }

            if (ppSC != IntPtr.Zero)
            {
                Marshal.Release(ppSC);
            }
        }
        return hier as IVsProject;
    }
}
