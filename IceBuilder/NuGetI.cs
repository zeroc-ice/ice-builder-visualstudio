// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;

namespace IceBuilder;

public delegate void NuGetBatchEnd();

public class NuGet
{
    IVsPackageInstallerEvents PackageInstallerEvents { get; set; }

    NuGetBatchEnd BatchEnd { get; set; }

    public NuGet()
    {
        var model = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
        PackageInstallerEvents = model.GetService<IVsPackageInstallerEvents>();
        PackageInstallerEvents.PackageReferenceAdded += PackageInstallerEvents_PackageReferenceAdded;
    }

    private void PackageInstallerEvents_PackageReferenceAdded(IVsPackageMetadata metadata) =>
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            BatchEnd?.Invoke();
        });

    public void OnNugetBatchEnd(NuGetBatchEnd batchEnd) =>
        BatchEnd = batchEnd;
}
