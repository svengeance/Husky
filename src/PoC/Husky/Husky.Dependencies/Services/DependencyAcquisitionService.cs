using System;
using System.IO;
using System.Threading.Tasks;
using Husky.Core.HuskyConfiguration;
using Husky.Dependencies.DependencyAcquisitionMethods;
using Husky.Services;
using Microsoft.Extensions.Logging;

namespace Husky.Dependencies.Services
{
    public interface IDependencyAcquisitionService
    {
        ValueTask AcquireDependency<T>(IDependencyAcquisitionMethod<T> acquisitionMethod) where T : HuskyDependency;
    }

    public class DependencyAcquisitionService: IDependencyAcquisitionService
    {
        private readonly ILogger<DependencyAcquisitionService> _logger;
        private readonly IShellExecutionService _shellExecutionService;
        private readonly IHttpService _httpService;
        private readonly IFileSystemService _fileSystemService;

        public DependencyAcquisitionService(ILogger<DependencyAcquisitionService> logger, IShellExecutionService shellExecutionService, IHttpService httpService, IFileSystemService fileSystemService)
        {
            _logger = logger;
            _shellExecutionService = shellExecutionService;
            _httpService = httpService;
            _fileSystemService = fileSystemService;
        }

        public ValueTask AcquireDependency<T>(IDependencyAcquisitionMethod<T> acquisitionMethod) where T : HuskyDependency
            => acquisitionMethod switch
               {
                   PackageManagerDependencyAcquisitionMethod<T> package => AcquirePackageDependency(package),
                   HttpDownloadDependencyAcquisitionMethod<T> http => AcquireHttpDependency(http),
                   _ => throw new InvalidOperationException($"Unable to acquire dependency via {acquisitionMethod.GetType().Name}")
               };

        private async ValueTask AcquireHttpDependency<T>(HttpDownloadDependencyAcquisitionMethod<T> httpDownload) where T : HuskyDependency
        {
            _logger.LogInformation("Attempting to download dependency {dependency}", httpDownload.Dependency.GetType().Name);

            var downloadFileDirectory = _fileSystemService.CreateTempDirectory();
            var downloadFileFullPath = Path.Combine(downloadFileDirectory.FullName, httpDownload.GetDownloadFileName());
            var downloadedFile = await _httpService.DownloadFile(httpDownload.GetDownloadUrl(), downloadFileFullPath);

            // Todo: Log execution result
            var executionResult = await _shellExecutionService.ExecuteFile(downloadedFile.FullName, httpDownload.InstallationArguments);

            Console.WriteLine($"Executed HTTP Acquisition for {httpDownload.Dependency.GetType().Name}");
        }


        public async ValueTask AcquirePackageDependency<T>(PackageManagerDependencyAcquisitionMethod<T> package) where T : HuskyDependency
        {
            _logger.LogInformation("Attempting to download dependency {dependency}", package.Dependency.GetType().Name);
            foreach (var command in package.PreInstallationCommands)
                await _shellExecutionService.ExecuteShellCommand(command);

            var installationCommand = package.GetPlatformPackageManager().CreateInstallCommand(package.PackageName);
            await _shellExecutionService.ExecuteShellCommand(installationCommand);
        }
    }
}