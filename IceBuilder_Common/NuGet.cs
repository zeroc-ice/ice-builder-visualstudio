// **********************************************************************
//
// Copyright (c) ZeroC, Inc. All rights reserved.
//
// **********************************************************************

namespace IceBuilder
{
    public delegate void NuGetBatchEnd();

    public interface NuGet
    {
        void OnNugetBatchEnd(NuGetBatchEnd batchEnd);
        void Restore(EnvDTE.Project project);
        bool IsPackageInstalled(EnvDTE.Project project, string packageId);
        void InstallLatestPackage(EnvDTE.Project project, string packageId);

        bool IsUserConsentGranted();
    }
}
