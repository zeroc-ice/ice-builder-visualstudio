using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IceBuilder
{
    public class GeneratedFileTracker
    {
        public void Add(EnvDTE.Project project)
        {            
            _generated.Add(project.FullName, ProjectUtil.GetGeneratedFiles(project));
        }

        public void Remove(EnvDTE.Project project)
        {
            if (_generated.ContainsKey(project.FullName))
            {
                _generated.Remove(project.FullName);
            }
        }

        public void Add(EnvDTE.Project project, String sliceFile, List<String> generatedFiles)
        {
            Dictionary<String, List<String>> generated = _generated.ContainsKey(project.FullName) ?
                _generated[project.FullName] : new Dictionary<String, List<String>>();
            generated[sliceFile] = generatedFiles;
            _generated[project.FullName] = generated;
        }

        public void Reap(EnvDTE.Project project)
        {
            Dictionary<String, List<String>> newGenerated = ProjectUtil.GetGeneratedFiles(project);
            if(_generated.ContainsKey(project.FullName))
            {
                Dictionary<String, List<String>> oldGenerated = _generated[project.FullName];
                foreach (KeyValuePair<String, List<String>> i in oldGenerated)
                {
                    if(!newGenerated.ContainsKey(i.Key))
                    {
                        ProjectUtil.DeleteItems(project, ProjectUtil.GetGeneratedFiles(project, i.Key));
                    }
                    else
                    {
                        List<String> newFiles = newGenerated[i.Key];
                        ProjectUtil.DeleteItems(project, i.Value.FindAll(f => !newFiles.Contains(f)));
                    }
                }
            }
            _generated[project.FullName] = newGenerated;
        }

        public bool Contains(EnvDTE.Project project, String path)
        {
            Dictionary<String, List<String>> names;
            if(_generated.TryGetValue(project.FullName, out names))
            { 
                foreach(KeyValuePair<String, List<String>> k in names)
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
