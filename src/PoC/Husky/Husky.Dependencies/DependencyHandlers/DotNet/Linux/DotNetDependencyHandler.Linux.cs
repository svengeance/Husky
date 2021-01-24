using System;
using System.Collections.Generic;
using Husky.Core;
using Husky.Core.Dependencies;
using Husky.Core.Enums;
using Husky.Internal.Shared;
using Range = SemVer.Range;
using Version = SemVer.Version;

namespace Husky.Dependencies.DependencyHandlers
{
    public partial class DotNetDependencyHandler
    {
        protected override IEnumerable<DependencyAcquisitionMethod<DotNet>> GetAvailableLinuxDependencies(DotNet dependency)
            => CurrentPlatform.LinuxDistribution switch
               {
                   LinuxDistribution.CentOS => CalculateCentosDependencies(dependency),
                   LinuxDistribution.Ubuntu => CalculateUbuntuDependencies(dependency),
                   _                        => Array.Empty<DependencyAcquisitionMethod<DotNet>>()
               };

        private IEnumerable<Version> GetSupportedDotNetVersions() => new[]
        {
            new Version(5, 0, 0),
            new Version(3, 1, 0),
            new Version(2, 1, 0),
        };

        private string FormatPackageName(DotNet dependency)
            => $"{DerivePackageProduct(dependency.Kind)}-{dependency.FrameworkType.ToString().ToLowerInvariant()}-{GetHighestMatchingVersionString(dependency.ParsedRange)}";

        private string DerivePackageProduct(DotNet.RuntimeKind kind)
            => kind switch
               {
                   DotNet.RuntimeKind.AspNet => "aspnetcore",
                   DotNet.RuntimeKind.RuntimeOnly => "dotnet",
                   _ => throw new PlatformNotSupportedException($"Invalid DotNet product specified: {kind} is invalid on Linux platforms.")
               };

        private string GetHighestMatchingVersionString(Range versionRange)
        {
            var matchingVersion = versionRange.MaxSatisfying(GetSupportedDotNetVersions());
            return $"{matchingVersion.Major}.{matchingVersion.Minor}";
        }

        private partial IEnumerable<DependencyAcquisitionMethod<DotNet>> CalculateCentosDependencies(DotNet dependency);
        private partial IEnumerable<DependencyAcquisitionMethod<DotNet>> CalculateUbuntuDependencies(DotNet dependency);
    }
}