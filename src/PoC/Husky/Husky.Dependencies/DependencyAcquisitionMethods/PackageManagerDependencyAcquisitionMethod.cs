using System;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Services;
using Microsoft.Extensions.DependencyInjection;
using Version = SemVer.Version;

namespace Husky.Dependencies.DependencyAcquisitionMethods
{
    public class PackageManagerDependencyAcquisitionMethod<T>: DependencyAcquisitionMethod<T> where T: HuskyDependency
    {
        public string[] PreInstallationCommands { get; init; } = Array.Empty<string>();

        public string PackageName { get; init; } = string.Empty;
        
        public PackageManagerDependencyAcquisitionMethod(T dependency): base(dependency) { }

        public override async ValueTask AcquireDependency(IServiceProvider serviceProvider)
        {
            var shellExecutionService = serviceProvider.GetRequiredService<IShellExecutionService>();

            foreach (var command in PreInstallationCommands)
                await shellExecutionService.ExecuteShellCommand(command);

            var installationCommand = GetPlatformPackageManager().CreateInstallCommand(PackageName);
            await shellExecutionService.ExecuteShellCommand(installationCommand);
        }

        private PackageManager GetPlatformPackageManager()
            => CurrentPlatform.OS switch
               {
                   OS.Linux when CurrentPlatform.LinuxDistribution is null => ThrowUnsupportedPackageManager(),
                   OS.Linux => GetLinuxPackageManager(CurrentPlatform.LinuxDistribution.Value, CurrentPlatform.OSVersion),
                   _        => ThrowUnsupportedPackageManager()
               };

        private PackageManager GetLinuxPackageManager(LinuxDistribution distribution, Version version)
            => (distribution, version) switch
               {
                   (LinuxDistribution.CentOS, { Major: 8 }) => PackageManagers.Dnf,
                   (LinuxDistribution.CentOS, { Major: 7 }) => PackageManagers.Yum,

                   (LinuxDistribution.RHEL, { Major: 8 }) => PackageManagers.Dnf,
                   (LinuxDistribution.RHEL, { Major: 7 }) => PackageManagers.Yum,

                   (LinuxDistribution.Debian, _) => PackageManagers.Apt,

                   (LinuxDistribution.Fedora, _) => PackageManagers.Dnf,

                   (LinuxDistribution.OpenSUSE, _) => PackageManagers.Zypper,

                   (LinuxDistribution.SLES, _) => PackageManagers.Zypper,

                   (LinuxDistribution.Ubuntu, _) => PackageManagers.Apt,
                   
                   _ => ThrowUnsupportedPackageManager()
               };
        
        private PackageManager ThrowUnsupportedPackageManager()
            => throw new PlatformNotSupportedException($"Unable to locate an appropriate Package Manager: {CurrentPlatform.LongDescription}");

        private static class PackageManagers
        {
            public static PackageManager Yum = new("yum", "install", "remove", "-y");
            public static PackageManager Dnf = new("dnf", "install", "erase", "-y");
            public static PackageManager Apt = new("apt", "install", "remove", "-y");
            public static PackageManager Zypper = new("zypper", "install", "remove", "--non-interactive");
            public static PackageManager Pkg = new("pgk", "install", "delete", "-y");
        }

        protected record PackageManager(string Command, string InstallVerb, string RemoveVerb, string SilentArg)
        {
            public string CreateInstallCommand(string packageName) => $"{Command} {InstallVerb} {packageName} {SilentArg}";
            public string CreateUninstallCommand(string packageName) => $"{Command} {RemoveVerb} {packageName} {SilentArg}";
        }
    }
}