using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace IceBuilder
{
    public class ProjectFactory : FlavoredProjectFactoryBase
    {
        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            throw new NotImplementedException();
        }
    }
}
