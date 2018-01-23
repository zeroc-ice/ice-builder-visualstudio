// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder
{
    public delegate bool ProjectUpdateAction(MSBuildProject project);

    public class ProjectChangedEventArgs : EventArgs
    {
        public ProjectChangedEventArgs(Microsoft.Build.Execution.ProjectInstance Project)
        {
            this.Project = Project;
        }
        public Microsoft.Build.Execution.ProjectInstance Project
        {
            get;
            private set;
        }
    }

    public interface IVsProjectManager
    {
        MSBuildProject MSBuildProject
        {
            get;
        }
        void SetProjectProperty(string name, string value, string label = "");

        string GetProjectProperty(string name);

        void UpdateProjectAsync(ProjectUpdateAction update);

        event EventHandler<ProjectChangedEventArgs> ProjectChanged;
    }

    public interface IVsProjectManagerFactory
    {
        IVsProjectManager GetProjectManager(IVsProject project);
    }

    public abstract class ProjectManager : IVsProjectManager
    {

        public MSBuildProject MSBuildProject
        {
            get;
            protected set;
        }

        public event EventHandler<ProjectChangedEventArgs> ProjectChanged;
        protected virtual void OnProjectChanged(ProjectChangedEventArgs e)
        {
            var handler = ProjectChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public string GetProjectProperty(string name)
        {
            var property = MSBuildProject.GetProperty(name);
            return property == null ? String.Empty : property.UnevaluatedValue;
        }

        public void SetProjectProperty(string name,
                                       string value,
                                       string label = "")
        {
            UpdateProjectAsync(project =>
                {
                    if (string.IsNullOrEmpty(label))
                    {
                        project.SetProperty(name, value);
                    }
                    else
                    {
                        var group = project.Xml.PropertyGroups.FirstOrDefault(
                            g => g.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));
                        if (group == null)
                        {
                            //
                            // Create our property group after the main language targets are imported so we can use the properties
                            // defined in this files.
                            //
                            var import = project.Xml.Imports.FirstOrDefault(
                                p => (p.Project.IndexOf("Microsoft.Cpp.targets", StringComparison.CurrentCultureIgnoreCase) != -1 ||
                                      p.Project.Equals("Microsoft.CSharp.targets", StringComparison.CurrentCultureIgnoreCase)));
                            if (import != null)
                            {
                                group = project.Xml.CreatePropertyGroupElement();
                                project.Xml.InsertAfterChild(group, import);
                            }
                            else
                            {
                                group = project.Xml.CreatePropertyGroupElement();
                                project.Xml.AppendChild(group);
                            }
                            group.Label = label;
                        }

                        var property = group.Properties.FirstOrDefault(
                            p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                        if (property != null)
                        {
                            property.Value = value;
                        }
                        else
                        {
                            group.AddProperty(name, value);
                        }
                    }
                    return true;
                });
        }

        public abstract void UpdateProjectAsync(ProjectUpdateAction update);
    }
}
