// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class GeneratedFileTracker
    {
        public void Add(IVsProject project, IceBuilderProjectType type)
        {
            String projectFullPath = ProjectUtil.GetProjectFullPath(project);
            Remove(projectFullPath);
            _generated.Add(projectFullPath,
                type == IceBuilderProjectType.CsharpProjectType ?
                    ProjectUtil.GetCSharpGeneratedFiles(project) :
                    ProjectUtil.GetCppGeneratedFiles(ProjectUtil.GetCppGeneratedFiles(project)));
        }

        public void Remove(String project)
        {
            if (_generated.ContainsKey(project))
            {
                _generated.Remove(project);
            }
        }

        public void Reap(String project, Dictionary<String, List<String>> newGenerated)
        {
            
            if (_generated.ContainsKey(project))
            {
                Dictionary<String, List<String>> oldGenerated = _generated[project];
                foreach (KeyValuePair<String, List<String>> i in oldGenerated)
                {
                    if(!newGenerated.ContainsKey(i.Key))
                    {
                        ProjectUtil.DeleteItems(i.Value);
                    }
                    else
                    {
                        List<String> newFiles = newGenerated[i.Key];
                        List<String> outdated = i.Value.FindAll(f => !newFiles.Contains(f));
                        if (outdated.Count > 0)
                        {
                            ProjectUtil.DeleteItems(outdated);
                        }
                    }
                }
            }
            _generated[project] = newGenerated;
        }

        public bool Contains(EnvDTE.Project project, String path)
        {
            return Contains(project.FullName, path);
        }

        public bool Contains(String project, String path)
        {
            Dictionary<String, List<String>> names;
            if (_generated.TryGetValue(project, out names))
            {
                foreach (KeyValuePair<String, List<String>> k in names)
                {
                    if (k.Value.Contains(path))
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


        private Dictionary<String, Dictionary<String, List<String>>> _generated =
            new Dictionary<String, Dictionary<String, List<String>>>();
    }
}
