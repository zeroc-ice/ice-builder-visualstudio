// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
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
    public class DTEUtil
    {
        public void UnloadProject()
        {
            Package.Instance.DTE.ExecuteCommand("Project.UnloadProject");
        }

        public void ReloadProject()
        {
            Package.Instance.DTE.ExecuteCommand("Project.ReloadProject");
        }

        public EnvDTE.Project GetSelectedProject()
        {
            Microsoft.VisualStudio.Shell.ServiceProvider sp = new Microsoft.VisualStudio.Shell.ServiceProvider(
                Package.Instance.DTE as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            IVsMonitorSelection selectionMonitor = sp.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            //
            // There isn't an open project.
            //
            if(selectionMonitor == null)
            {
                return null;
            }

            EnvDTE.Project project = null;
            IntPtr ppHier;
            uint pitemid;
            IVsMultiItemSelect ppMIS;
            IntPtr ppSC;
            if (ErrorHandler.Failed(selectionMonitor.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC)))
            {
                return null;
            }

            if (ppHier != IntPtr.Zero)
            {
                IVsHierarchy hier = (IVsHierarchy)Marshal.GetObjectForIUnknown(ppHier);
                Marshal.Release(ppHier);
                object obj;
                hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
                if (obj != null)
                {
                    project = obj as EnvDTE.Project;
                }
            }

            if (ppSC != IntPtr.Zero)
            {
                Marshal.Release(ppSC);
            }

            return project;
        }

        public static List<EnvDTE.Project> GetProjects(Solution solution)
        {
            List<EnvDTE.Project> projects = new List<EnvDTE.Project>();
            foreach (EnvDTE.Project p in solution.Projects)
            {
                if (String.IsNullOrEmpty(p.Kind) || p.Kind.Equals(unloadedProjectGUID))
                {
                    continue;
                }

                if(projects.Contains(p))
                {
                    continue;
                }
                GetProjects(p, ref projects);
            }
            return projects;
        }

        private static void GetProjects(EnvDTE.Project project, ref List<EnvDTE.Project> projects)
        {
            if (String.IsNullOrEmpty(project.Kind) || project.Kind.Equals(unloadedProjectGUID))
            {
                return;
            }

            if(project.Kind == EnvDTE80.ProjectKinds.vsProjectKindSolutionFolder)
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    EnvDTE.Project p = item.Object as EnvDTE.Project;
                    if (p != null)
                    {
                        GetProjects(p, ref projects);
                    }
                }
                return;
            }

            if (project.Kind == cppProjectGUID ||
                project.Kind == csharpProjectGUID ||
                project.Kind == cppStoreAppProjectGUID)
            {
                if (!projects.Contains(project))
                {
                    projects.Add(project);
                }
            }
        }

        public const string cppProjectGUID = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";
        public const string cppStoreAppProjectGUID = "{BC8A1FFA-BEE3-4634-8014-F334798102B3}";
        public const string csharpProjectGUID = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        public const string unloadedProjectGUID = "{67294A52-A4F0-11D2-AA88-00C04F688DDE}";
    }
}
