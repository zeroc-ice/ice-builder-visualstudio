// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell.Flavor;
using System;
using System.Runtime.InteropServices;

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
        protected override object PreCreateForOuter(IntPtr IUnknown) => new ProjectOld();
    }
}
