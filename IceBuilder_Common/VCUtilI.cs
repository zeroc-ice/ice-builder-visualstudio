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
            foreach (VCFilter f in project.Filters)
            {
                if (f.Name.Equals("Slice Files"))
                {
                    if (String.IsNullOrEmpty(f.Filter) || !f.Filter.Equals("ice"))
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

        public VCFilter FindOrCreateFilter(VCFilter parent, String name)
        {
            foreach (VCFilter f in parent.Filters)
            {
                if(f.Name.Equals(name))
                {
                    return f;
                }
            }
            return parent.AddFilter(name);
        }

        public VCFilter FindOrCreateFilter(VCProject parent, String name)
        {
            foreach(VCFilter f in parent.Filters)
            {
                if (f.Name.Equals(name))
                {
                    return f;
                }
            }
            return parent.AddFilter(name);
        }

        public String Evaluate(EnvDTE.Configuration dteConfig, String value)
        {
            EnvDTE.Project dteProject = dteConfig.Owner as EnvDTE.Project;
            VCProject project = dteProject.Object as VCProject;
            VCConfiguration config = project.Configurations.Item(dteConfig.ConfigurationName + "|" + dteConfig.PlatformName);
            return config.Evaluate(value);
        }

        public void AddGeneratedFiles(EnvDTE.Project dteProject, EnvDTE.Configuration config, String filterName, List<String> paths, bool generatedFilesPerConfiguration)
        {
            VCProject project = dteProject.Object as VCProject;

            VCFilter filter = FindOrCreateFilter(project, filterName);
            if(generatedFilesPerConfiguration)
            {
                filter = FindOrCreateFilter(filter, config.PlatformName);
                filter = FindOrCreateFilter(filter, config.ConfigurationName);
            }

            String configurationName = config.ConfigurationName;
            String platformName = config.PlatformName;

            foreach (String path in paths)
            {
                VCFile file = filter.AddFile(path);

                //
                // Exclude the file from all other configurations
                //
                if(generatedFilesPerConfiguration)
                {
                    foreach (VCFileConfiguration c in file.FileConfigurations)
                    {
                        if (!c.ProjectConfiguration.ConfigurationName.Equals(configurationName) || 
                            !c.ProjectConfiguration.Platform.Name.Equals(platformName))
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
                catch (Exception)
                {
                }
            }
        }
    }
}
