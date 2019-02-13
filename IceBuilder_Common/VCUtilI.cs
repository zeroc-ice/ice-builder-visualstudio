// **********************************************************************
//
// Copyright (c) ZeroC, Inc. All rights reserved.
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

        public VCFile FindFile(VCProject project, string name)
        {
            foreach(VCFile file in project.Files)
            {
                var n = file.Name;
                var p = file.RelativePath;
                if(name.Equals(file.RelativePath))
                {
                    return file;
                }
            }
            return null;
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

        public void AddGenerated(IVsProject project, string path, string filter, string platform, string configuration)
        {
            var dteproject = project.GetDTEProject();
            var vcproject = dteproject.Object as VCProject;
            var parent = FindOrCreateFilter(vcproject, filter);
            parent = FindOrCreateFilter(parent, platform);
            parent = FindOrCreateFilter(parent, configuration);
            var file = FindFile(vcproject, path);
            if(file != null)
            {
                file.Move(parent);
            }
        }
    }
}
