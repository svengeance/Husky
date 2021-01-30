using System;
using System.Collections.Generic;
using System.Linq;
using Husky.Core;
using Husky.Core.Dependencies;
using Husky.Core.Platform;
using Husky.Dependencies.DependencyAcquisitionMethods;
using Version = SemVer.Version;

namespace Husky.Dependencies.DependencyHandlers
{
    public partial class DotNetDependencyHandler
    {
        private partial IEnumerable<DependencyAcquisitionMethod<DotNet>> CalculateCentosDependencies(DotNet dependency)
        {
            if (CurrentPlatform.OSVersion.Major is not 7 or 8)
                throw new PlatformNotSupportedException($"Unable to install DotNet on unsupported CentOS version {CurrentPlatform.OSVersion}");

            var preInstallationCommands = Array.Empty<string>();
            if (CurrentPlatform.OSVersion.Major == 7)
                preInstallationCommands = new[] { "sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm" };
            
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