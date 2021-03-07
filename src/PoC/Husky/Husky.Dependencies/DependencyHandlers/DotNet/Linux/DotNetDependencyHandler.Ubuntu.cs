using System;
using System.Collections.Generic;
using System.Linq;
using Husky.Core.Dependencies;
using Husky.Core.Platform;
using Husky.Dependencies.DependencyAcquisitionMethods;
using Version = SemVer.Version;

namespace Husky.Dependencies.DependencyHandlers
{
    public partial class DotNetDependencyHandler
    {
        private partial IEnumerable<DependencyAcquisitionMethod<DotNet>> CalculateUbuntuDependencies(DotNet dependency)
        {
            var supportedUbuntuVersions = new[]
            {
                new Version(20, 10, 0),
                new Version(20, 04, 0),
                new Version(18, 04, 0),
                new Version(16, 04, 0)
            };
            
            if (!supportedUbuntuVersions.Contains(CurrentPlatform.OSVersion))
                throw new PlatformNotSupportedException($"Unable to install DotNet on unsupported Ubuntu version {CurrentPlatform.OSVersion}");

            var preInstallationCommands = new[]
            {
                $"wget https://packages.microsoft.com/config/ubuntu/{CurrentPlatform.OSVersion.Major}.{CurrentPlatform.OSVersion.Minor}/packages-microsoft-prod.deb -O packages-microsoft-prod.deb",
                "sudo dpkg -i packages-microsoft-prod.deb",
                "sudo apt-get update",
                "sudo apt-get install -y apt-transport-https",
                "sudo apt-get update"
            };

            return new[]
            {
                new PackageManagerDependencyAcquisitionMethod<DotNet>(dependency)
                {
                    PreInstallationCommands = preInstallationCommands,
                    PackageName = FormatPackageName(dependency)
                }
            };
        }
    }
}