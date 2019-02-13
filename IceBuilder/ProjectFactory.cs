// **********************************************************************
//
// Copyright (c) ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace IceBuilder
{
    [Guid(Package.IceBuilderNewFlavorGuid)]
    public class ProjectFactory : FlavoredProjectFactoryBase
    {
        protected override object PreCreateForOuter(IntPtr IUnknown)
        {
            Project project = new Project();
            project.Package = Package.Instance;
            return project;
        }
    }

    [Guid(Package.IceBuilderOldFlavorGuid)]
    public class ProjectFactoryOld : FlavoredProjectFactoryBase
    {
        protected override object PreCreateForOuter(IntPtr IUnknown)
        {
            return new ProjectOld();
        }
    }
}
