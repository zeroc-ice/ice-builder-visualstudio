// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VCProjectEngine;

namespace IceBuilder;

public static class VCUtil
{
    public static bool SetupSliceFilter(EnvDTE.Project dteProject)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
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

    public static VCFilter FindOrCreateFilter(VCFilter parent, string name)
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

    public static VCFile FindFile(VCProject project, string name)
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

    public static VCFilter FindOrCreateFilter(VCProject parent, string name)
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

    public static string Evaluate(EnvDTE.Configuration dteConfig, string value)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        EnvDTE.Project dteProject = dteConfig.Owner as EnvDTE.Project;
        VCProject project = dteProject.Object as VCProject;
        VCConfiguration config = (VCConfiguration)(project.Configurations as IVCCollection).Item(dteConfig.ConfigurationName + "|" + dteConfig.PlatformName);
        return config.Evaluate(value);
    }

    public static void AddGenerated(IVsProject project, string path, string filter, string platform, string configuration)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var dteproject = project.GetDTEProject();
        var vcproject = dteproject.Object as VCProject;
        var parent = FindOrCreateFilter(vcproject, filter);
        parent = FindOrCreateFilter(parent, platform);
        parent = FindOrCreateFilter(parent, configuration);
        var file = FindFile(vcproject, path);
        file?.Move(parent);
    }
}
