using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Dependencies;
using Husky.Core.Enums;
using Husky.Services;
using Microsoft.Extensions.Logging;
using Version = SemVer.Version;

namespace Husky.Dependencies.DependencyHandlers
{
    public partial class DotNetDependencyHandler: DependencyHandler<DotNet>
    {
        private readonly ILogger _logger;
        private readonly IShellExecutionService _shellExecutionService;

        /*
         * Todo: This method of listing how to resolve dependencies is going to grow explosively large.
         *       It may be best to think about potentially offloading these to text files, and resolving them as-necessary.
         *       Maybe..this is just the cost of trying to have first-class support for dependencies. Yikes.
         *
         *       *sigh*. What did we sign up for. (:
         */
        public DotNetDependencyHandler(DotNet dependency, ILogger<DotNetDependencyHandler> logger, IShellExecutionService shellExecutionService): base(dependency)
        {
            _logger = logger;
            _shellExecutionService = shellExecutionService;
        }

        protected override IEnumerable<DependencyAcquisitionMethod<DotNet>> GetAvailableOsxDependencies(DotNet dependency) => Array.Empty<DependencyAcquisitionMethod<DotNet>>();

        public override async ValueTask<bool> IsAlreadyInstalled()
            => IsAlreadyInstalled(Dependency, await GetDotnetInstallationOutput(Dependency.FrameworkType));

        private bool IsAlreadyInstalled(DotNet dependency, string[] splitDotnetOutput)
            => dependency.FrameworkType switch
            {
                FrameworkInstallationType.Sdk => GetInstalledSdks(splitDotnetOutput).Any(a => dependency.ParsedRange.IsSatisfied(a)),
                FrameworkInstallationType.Runtime => GetInstalledRuntimes(splitDotnetOutput).Any(a => a.runtimeKind == dependency.Kind && dependency.ParsedRange.IsSatisfied(a.version)),
                _ => false
            };
        
        private async ValueTask<string[]> GetDotnetInstallationOutput(FrameworkInstallationType frameworkInstallationType)
        {
            var command = frameworkInstallationType == FrameworkInstallationType.Runtime
                ? "--list-runtimes"
                : "--list-sdks";

            var (exitCode, stdOutput, _) = await _shellExecutionService.ExecuteShellCommand($"dotnet {command}");

            _logger.LogDebug("Retrieved dotnet installation output using {command}\n{dotnetOutput}", command, stdOutput);

            return exitCode != 0
                ? Array.Empty<string>()
                : stdOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }

        // 5.0.100 [C:\Program Files\dotnet\sdk]
        private Version[] GetInstalledSdks(string[] dotnetSdksList)
            => dotnetSdksList.Select(s => s[..s.IndexOf(' ')])
                             .Select(s => Version.Parse(s))
                             .ToArray();

        // Microsoft.AspNetCore.App 5.0.0 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
        // Microsoft.NETCore.App 2.1.23 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
        private (DotNet.RuntimeKind runtimeKind, Version version)[] GetInstalledRuntimes(IEnumerable<string> dotnetRuntimesList)
            => dotnetRuntimesList.Select(s => s.Split(' '))
                                 .Select(s => (DeriveRuntimeKindFromString(s[0].Split('.')[1]), Version.Parse(s[1])))
                                 .ToArray();

        private DotNet.RuntimeKind DeriveRuntimeKindFromString(string dotnetRuntimeKindOutput)
            => dotnetRuntimeKindOutput == "AspNetCore"
                ? DotNet.RuntimeKind.AspNet
                : dotnetRuntimeKindOutput == "WindowsDesktop"
                    ? DotNet.RuntimeKind.Desktop
                    : DotNet.RuntimeKind.RuntimeOnly;
    }
}