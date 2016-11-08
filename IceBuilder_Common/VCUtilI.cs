// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.VCProjectEngine;

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

        public void AddGeneratedFiles(EnvDTE.Project dteProject, EnvDTE.Configuration config, string filterName,
                                      List<string> paths, bool generatedFilesPerConfiguration)
        {
            VCProject project = dteProject.Object as VCProject;

            VCFilter filter = FindOrCreateFilter(project, filterName);
            if(generatedFilesPerConfiguration)
            {
                filter = FindOrCreateFilter(filter, config.PlatformName);
                filter = FindOrCreateFilter(filter, config.ConfigurationName);
            }

            string configurationName = config.ConfigurationName;
            string platformName = config.PlatformName;

            foreach(string path in paths)
            {
                if(!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }

                VCFile file = filter.AddFile(path);

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
                //
                // Exclude the file from all other configurations
                //
                if(generatedFilesPerConfiguration)
                {
                    foreach(VCFileConfiguration c in file.FileConfigurations)
                    {
                        if(!c.ProjectConfiguration.ConfigurationName.Equals(configurationName) ||
                            !c.ProjectConfiguration.Platform.Name.Equals(platformName))
                        {
                            c.ExcludedFromBuild = true;
                        }
                    }
                }
            }
        }
    }
}
