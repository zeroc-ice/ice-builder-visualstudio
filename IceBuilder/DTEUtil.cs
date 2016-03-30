// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public enum IceBuilderProjectType
    {
        None,
        CppProjectType,
        CsharpProjectType
    }

    public class DTEUtil
    {
        public static List<IVsProject> GetProjects()
        {
            IEnumHierarchies enumHierarchies;
            Guid guid = Guid.Empty;
            uint flags = (uint) __VSENUMPROJFLAGS.EPF_ALLPROJECTS;
            ErrorHandler.ThrowOnFailure(Package.Instance.IVsSolution.GetProjectEnum(flags, guid, out enumHierarchies));

            List<IVsProject> projects = new List<IVsProject>();

            IVsHierarchy[] hierarchies = new IVsHierarchy[1];
            uint sz;
            do
            {
                ErrorHandler.ThrowOnFailure(enumHierarchies.Next(1, hierarchies, out sz));
                if (sz > 0)
                {
                    projects.Add(hierarchies[0] as IVsProject);
                }
            }
            while(sz == 1);
            return projects;
        }

        public static void GetSubProjects(IVsProject p, ref List<IVsProject> projects)
        {
            GetSubProjects((IVsHierarchy)p, VSConstants.VSITEMID_ROOT, ref projects);
        }

        public static void GetSubProjects(IVsHierarchy h, uint itemId, ref List<IVsProject> projects)
        {
            IntPtr nestedValue = IntPtr.Zero;
            uint nestedId = 0;
            Guid nestedGuid = typeof(IVsHierarchy).GUID;
            int result = h.GetNestedHierarchy(itemId, ref nestedGuid, out nestedValue, out nestedId);
            if (ErrorHandler.Succeeded(result) && nestedValue != IntPtr.Zero && nestedId == VSConstants.VSITEMID_ROOT)
            {
                // Get the nested hierachy
                IVsProject project = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(nestedValue) as IVsProject;
                System.Runtime.InteropServices.Marshal.Release(nestedValue);
                if(project != null)
                {
                    projects.Add(project);
                    GetSubProjects(project, ref projects);
                }
            }
            else
            {
                // Get the first visible child node
                object value;
                result = h.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_FirstVisibleChild, out value);
                while (result == VSConstants.S_OK && value != null)
                {
                    if(value is int && (uint)(int)value == VSConstants.VSITEMID_NIL)
                    {
                        // No more nodes
                        break;
                    }
                    else
                    {
                        uint child = Convert.ToUInt32(value);

                        GetSubProjects(h, child, ref projects);

                        // Get the next visible sibling node
                        value = null;
                        result = h.GetProperty(child, (int)__VSHPROPID.VSHPROPID_NextVisibleSibling, out value);
                    }
                }
            }
        }

        public static IVsProject GetSelectedProject()
        {
            IVsHierarchy hier = null;
            Microsoft.VisualStudio.Shell.ServiceProvider sp = new Microsoft.VisualStudio.Shell.ServiceProvider(
                Package.Instance.DTE as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            IVsMonitorSelection selectionMonitor = sp.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            //
            // There isn't an open project.
            //
            if (selectionMonitor != null)
            {
                IntPtr ppHier;
                uint pitemid;
                IVsMultiItemSelect ppMIS;
                IntPtr ppSC;
                ErrorHandler.ThrowOnFailure(selectionMonitor.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC));


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

        public static EnvDTE.Project GetProject(IVsHierarchy hierarchy)
        {
            object obj = null;
            if (hierarchy != null)
            {
                hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            }
            return obj as EnvDTE.Project;
        }

        public static EnvDTE.ProjectItem GetProjectItem(IVsHierarchy hierarchy, uint itemId)
        {
            object obj = null;
            if (hierarchy != null)
            {
                hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            }
            return obj as EnvDTE.ProjectItem;
        }

        public static bool IsCppProject(IVsProject project)
        {
            Guid type = ProjectUtil.GetProjecTypeGuid(project);
            return type.Equals(cppProjectGUID) || type.Equals(cppStoreAppProjectGUID);
        }

        public static bool IsCSharpProject(IVsProject project)
        {
            return ProjectUtil.GetProjecTypeGuid(project).Equals(csharpProjectGUID);
        }

        public static IceBuilderProjectType IsIceBuilderEnabled(IVsProject project)
        {
            if (project != null)
            {
                IceBuilderProjectType type = IsCppProject(project) ? IceBuilderProjectType.CppProjectType :
                                             IsCSharpProject(project) ? IceBuilderProjectType.CsharpProjectType : IceBuilderProjectType.None;
                if (type != IceBuilderProjectType.None)
                {
                    if (MSBuildUtils.IsIceBuilderEnabled(MSBuildUtils.LoadedProject(ProjectUtil.GetProjectFullPath(project), 
                                                                                    DTEUtil.IsCppProject(project), 
                                                                                    true)))
                    {
                        return type;
                    }
                }
            }
            return IceBuilderProjectType.None;
        }

        public static readonly Guid cppProjectGUID = 
            new Guid("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}");
        public static readonly Guid cppStoreAppProjectGUID = 
            new Guid("{BC8A1FFA-BEE3-4634-8014-F334798102B3}");
        public static readonly Guid csharpProjectGUID = 
            new Guid("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
        public static readonly Guid unloadedProjectGUID = 
            new Guid("{67294A52-A4F0-11D2-AA88-00C04F688DDE}");
    }
}
