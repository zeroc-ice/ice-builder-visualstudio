// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell.Flavor;
using System;
using System.Runtime.InteropServices;

namespace IceBuilder;

[Guid(Package.IceBuilderNewFlavorGuid)]
public class ProjectFactory : FlavoredProjectFactoryBase
{
    protected override object PreCreateForOuter(IntPtr IUnknown)
    {
        return new Project
        {
            Package = Package.Instance
        };
    }
}
