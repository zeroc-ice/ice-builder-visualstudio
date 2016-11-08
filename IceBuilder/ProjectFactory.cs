// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace IceBuilder
{
    /// <summary>
    /// The project factory for our project flavor.
    /// </summary>
    [Guid(ProjectFactoryGuidString)]
    public class ProjectFactory : FlavoredProjectFactoryBase
    {
        public const string ProjectFactoryGuidString = "3C53C28F-DC44-46B0-8B85-0C96B85B2042";
        public static readonly Guid ProjectFactoryGuid = new Guid(ProjectFactoryGuidString);

        #region IVsAggregatableProjectFactory

        /// <summary>
        /// Create an instance of CustomPropertyPageProjectFlavor. 
        /// The initialization will be done later when Visual Studio calls
        /// InitalizeForOuter on it.
        /// </summary>
        /// <param name="outerProjectIUnknown">
        /// This value points to the outer project. It is useful if there is a 
        /// Project SubType of this Project SubType.
        /// </param>
        /// <returns>
        /// An CustomPropertyPageProjectFlavor instance that has not been initialized.
        /// </returns>
        protected override object PreCreateForOuter(IntPtr IUnknown)
        {
            Project project = new Project();
            project.Package = Package.Instance;
            return project;
        }

        #endregion
    }
}
