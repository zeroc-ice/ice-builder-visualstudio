// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.VCProjectEngine;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public class VCUtilI : VCUtil
    {
        public bool SetupSliceFilter(EnvDTE.Project dteProject)
        {
            VCProject project = dteProject.Object as VCProject;
            foreach(VCFilter f in project.Filters)
            {
                if(f.Name.Equals("Slice Files"))
                {
                    if(string.IsNullOrEmpty(f.Filter) || !f.Filter.Equals("ice"))
                    {
                        f.Filter = "ice";
                        return true;
                    }
                    return false;
                }
            }

            VCFilter filter = project.AddFilter("Slice Files");
            filter.Filter = "ice";
            return true;
        }

        public VCFilter FindOrCreateFilter(VCFilter parent, string name)
        {
            foreach(VCFilter f in parent.Filters)
            {
                if(f.Name.Equals(name))
                {
                    return f;
                }
            }
            return parent.AddFilter(name);
        }

        public VCFilter FindOrCreateFilter(VCProject parent, string name)
        {
            foreach(VCFilter f in parent.Filters)
            {
                if(f.Name.Equals(name))
                {
                    return f;
                }
            }
            return parent.AddFilter(name);
        }

        public string Evaluate(EnvDTE.Configuration dteConfig, string value)
        {
            EnvDTE.Project dteProject = dteConfig.Owner as EnvDTE.Project;
            VCProject project = dteProject.Object as VCProject;
            VCConfiguration config = project.Configurations.Item(dteConfig.ConfigurationName + "|" + dteConfig.PlatformName);
            return config.Evaluate(value);
        }

        public void AddGeneratedFile(IVsProject project, VCFilter filter, string path, EnvDTE.Configuration config)
        {
            int found;
            uint id;
            VSDOCUMENTPRIORITY[] priority = new VSDOCUMENTPRIORITY[1];
            project.IsDocumentInProject(path, out found, priority, out id);
            if(found == 0)
            {
                if(!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                if(!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }

                VCFile file = null;
                if(config == null)
                {
                    file = filter.AddFile(path);
                }
                else
                {
                    filter = FindOrCreateFilter(filter, config.PlatformName);
                    filter = FindOrCreateFilter(filter, config.ConfigurationName);
                    file = filter.AddFile(path);
                    foreach(VCFileConfiguration c in file.FileConfigurations)
                    {
                        if(!c.ProjectConfiguration.ConfigurationName.Equals(config.ConfigurationName) ||
                           !c.ProjectConfiguration.Platform.Name.Equals(config.PlatformName))
                        {
                            c.ExcludedFromBuild = true;
                        }
                    }
                }

                try
                {
                    //
                    // Remove the file otherwise it will be considered up to date.
                    //
                    File.Delete(path);
                }
                catch(Exception)
                {
                }
            }
        }

        public void AddGeneratedFiles(IVsProject project, List<GeneratedFileSet> filesets)
        {
            var dteproject = project.GetDTEProject();
            var vcproject = dteproject.Object as VCProject;

            var sourcesFilter = FindOrCreateFilter(vcproject, "Source Files");
            var headersFilter = FindOrCreateFilter(vcproject, "Header Files");

            foreach(var fileset in filesets)
            {
                foreach(var entry in fileset.sources)
                {
                    AddGeneratedFile(project, sourcesFilter, entry.Key, entry.Value.Count == 1 ? entry.Value.First() : null);
                }

                foreach(var entry in fileset.headers)
                {
                    AddGeneratedFile(project, headersFilter, entry.Key, entry.Value.Count == 1 ? entry.Value.First() : null);
                }
            }
        }
    }
}
