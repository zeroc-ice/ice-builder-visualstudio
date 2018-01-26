// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
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

        void SetProjectItemMetadata(string name, string value);

        void SetProjectItemMetadata(string itemType, string label, string name, string value);

        string GetProjectProperty(string name);

        string GetProjectItemMetadata(string name, bool evaluated, string defaultValue);

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

        public string GetProjectItemMetadata(string name, bool evaluated, string defaultValue)
        {
            return MSBuildProject.GetDefaultItemMetadata(name, evaluated, defaultValue);
        }

        public void SetProjectProperty(string name,
                                       string value,
                                       string label = "")
        {
            UpdateProjectAsync(project =>
                {
                    project.SetProperty(name, value, label);
                    return true;
                });
        }

        public void SetProjectItemMetadata(string name, string value)
        {
            SetProjectItemMetadata("SliceCompile", "IceBuilder", name, value);
        }

        public void SetProjectItemMetadata(string itemType, string label, string name, string value)
        {
            UpdateProjectAsync(project =>
            {
                project.SetItemMetadata(itemType, label, name, value);
                return true;
            });
        }

        public abstract void UpdateProjectAsync(ProjectUpdateAction update);
    }
}
