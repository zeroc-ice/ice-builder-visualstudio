// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.IO;

namespace IceBuilder
{
    public class VCUtilI : IVCUtil
    {
        public bool SetupSliceFilter(EnvDTE.Project dteProject)
        {
            VCProject project = dteProject.Object as VCProject;
            IVCCollection filters = (IVCCollection)project.Filters;
            foreach (VCFilter f in filters)
            {
                if (f.Name == "Slice Files")
                {
                    if (string.IsNullOrEmpty(f.Filter) || f.Filter != "ice")
                    {
                        f.Filter = "ice";
                        return true;
                    }
                    return false;
                }
            }

            VCFilter filter = (VCFilter)project.AddFilter("Slice Files");
            filter.Filter = "ice";
            return true;
        }

        public VCFilter FindOrCreateFilter(VCFilter parent, string name)
        {
            IVCCollection filters = (IVCCollection)parent.Filters;
            foreach (VCFilter f in filters)
            {
                if (f.Name == name)
                {
                    return f;
                }
            }
            return (VCFilter)parent.AddFilter(name);
        }

        public VCFile FindFile(VCProject project, string name)
        {
            IVCCollection files = (IVCCollection)project.Files;
            foreach (VCFile file in files)
            {
                if (name == file.RelativePath)
                {
                    return file;
                }
            }
            return null;
        }

        public VCFilter FindOrCreateFilter(VCProject parent, string name)
        {
            IVCCollection filters = (IVCCollection)parent.Filters;
            foreach (VCFilter f in filters)
            {
                if (f.Name == name)
                {
                    return f;
                }
            }
            return (VCFilter)parent.AddFilter(name);
        }

        public string Evaluate(EnvDTE.Configuration dteConfig, string value)
        {
            EnvDTE.Project dteProject = dteConfig.Owner as EnvDTE.Project;
            VCProject project = dteProject.Object as VCProject;
            VCConfiguration config = (VCConfiguration)(project.Configurations as IVCCollection).Item(dteConfig.ConfigurationName + "|" + dteConfig.PlatformName);
            return config.Evaluate(value);
        }

        public void AddGeneratedFile(IVsProject project, VCFilter filter, string path, EnvDTE.Configuration config)
        {
            VSDOCUMENTPRIORITY[] priority = new VSDOCUMENTPRIORITY[1];
            project.IsDocumentInProject(path, out int found, priority, out uint _);
            if (found == 0)
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }

                if (config == null)
                {
                    filter.AddFile(path);
                }
                else
                {
                    filter = FindOrCreateFilter(filter, config.PlatformName);
                    filter = FindOrCreateFilter(filter, config.ConfigurationName);
                    VCFile file = (VCFile)filter.AddFile(path);
                    VCFileConfiguration[] configurations = (VCFileConfiguration[])file.FileConfigurations;
                    foreach (VCFileConfiguration c in configurations)
                    {
                        VCConfiguration projectConfiguration = (VCConfiguration)c.ProjectConfiguration;
                        VCPlatform projectPlatform = (VCPlatform)projectConfiguration.Platform;
                        if (projectConfiguration.ConfigurationName != config.ConfigurationName || projectPlatform.Name != config.PlatformName)
                        {
                            c.ExcludedFromBuild = true;
                        }
                    }
                }

                try
                {
                    // Remove the file otherwise it will be considered up to date.
                    File.Delete(path);
                }
                catch (Exception)
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
            if (file != null)
            {
                file.Move(parent);
            }
        }
    }
}
