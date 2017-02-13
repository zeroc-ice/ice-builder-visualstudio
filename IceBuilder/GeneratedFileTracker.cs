// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class GeneratedFileTracker
    {
        public void Add(IVsProject project, IceBuilderProjectType type)
        {
            string projectFullPath = ProjectUtil.GetProjectFullPath(project);
            Remove(projectFullPath);
            _generated.Add(projectFullPath,
                type == IceBuilderProjectType.CsharpProjectType ?
                    ProjectUtil.GetCSharpGeneratedFiles(project) :
                    ProjectUtil.GetCppGeneratedFiles(ProjectUtil.GetCppGeneratedFiles(project)));
        }

        public void Remove(string project)
        {
            if(_generated.ContainsKey(project))
            {
                _generated.Remove(project);
            }
        }

        public void Reap(string project, Dictionary<string, List<string>> newGenerated)
        {

            if(_generated.ContainsKey(project))
            {
                Dictionary<string, List<string>> oldGenerated = _generated[project];
                foreach(KeyValuePair<string, List<string>> i in oldGenerated)
                {
                    if(!newGenerated.ContainsKey(i.Key))
                    {
                        ProjectUtil.DeleteItems(i.Value);
                    }
                    else
                    {
                        List<string> newFiles = newGenerated[i.Key];
                        List<string> outdated = i.Value.FindAll(f => !newFiles.Contains(f));
                        if(outdated.Count > 0)
                        {
                            ProjectUtil.DeleteItems(outdated);
                        }
                    }
                }
            }
            _generated[project] = newGenerated;
        }

        public bool Contains(EnvDTE.Project project, string path)
        {
            return Contains(project.FullName, path);
        }

        public bool Contains(string project, string path)
        {
            Dictionary<string, List<string>> names;
            if(_generated.TryGetValue(project, out names))
            {
                foreach(KeyValuePair<string, List<string>> k in names)
                {
                    if(k.Value.Contains(path))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Clear()
        {
            _generated.Clear();
        }


        private Dictionary<string, Dictionary<string, List<string>>> _generated =
            new Dictionary<string, Dictionary<string, List<string>>>();
    }

}
