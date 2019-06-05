using NuGet.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace IceBuilder
{
    public class NuGetI : NuGet
    {
        IVsPackageInstallerEvents PackageInstallerEvents
        {
            get;
            set;
        }

        IVsPackageInstallerServices PackageInstallerServices
        {
            get;
            set;
        }
        IVsPackageInstaller PackageInstaller
        {
            get;
            set;
        }

        IVsPackageRestorer PackageRestorer
        {
            get;
            set;
        }

        NuGetBatchEnd BatchEnd
        {
            get;
            set;
        }

        public NuGetI()
        {
            var model = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            PackageInstallerServices = model.GetService<IVsPackageInstallerServices>();
            PackageInstaller = model.GetService<IVsPackageInstaller>();
            PackageRestorer = model.GetService<IVsPackageRestorer>();
            PackageInstallerEvents = model.GetService<IVsPackageInstallerEvents>();
            PackageInstallerEvents.PackageReferenceAdded += PackageInstallerEvents_PackageReferenceAdded;
        }

        private void PackageInstallerEvents_PackageReferenceAdded(IVsPackageMetadata metadata)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (BatchEnd != null)
                {
                    BatchEnd();
                }
            });
        }

        public bool IsPackageInstalled(EnvDTE.Project project, string packageId)
        {
            return PackageInstallerServices.IsPackageInstalled(project, packageId);
        }

        public void InstallLatestPackage(EnvDTE.Project project, string packageId)
        {
            PackageInstaller.InstallPackage(null, project, packageId, (string)null, false);
        }

        public void Restore(EnvDTE.Project project)
        {
            PackageRestorer.RestorePackages(project);
        }
        private void PackageInstallerEvents_PackageInstalled(IVsPackageMetadata metadata)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if(BatchEnd != null)
                {
                    BatchEnd();
                }
            });
        }

        void NuGet.OnNugetBatchEnd(NuGetBatchEnd batchEnd)
        {
            BatchEnd = batchEnd;
        }

        public bool IsUserConsentGranted()
        {
            return PackageRestorer.IsUserConsentGranted();
        }
    }
}
