using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.TaskOptions.Resources;
using Husky.Core.Workflow.Uninstallation;
using Husky.Services;
using Microsoft.Extensions.Logging;

namespace Husky.Tasks.Resources
{
    public class ExtractBundledResource : HuskyTask<ExtractBundledResourceOptions>
    {
        private readonly ILogger _logger;
        private readonly IEmbeddedResourceService _embeddedResourceService;
        private readonly IFileSystemService _fileSystemService;

        public ExtractBundledResource(ILogger<ExtractBundledResource> logger, IEmbeddedResourceService embeddedResourceService, IFileSystemService fileSystemService)
        {
            _logger = logger;
            _embeddedResourceService = embeddedResourceService;
            _fileSystemService = fileSystemService;
        }

        protected override async ValueTask ExecuteTask()
        {
            var availableFiles = _embeddedResourceService.ListResources(HuskyContext.InstallationAssembly, Configuration.Resources).ToArray();
            _logger.LogDebug("Located {numberOfFilteredEmbeddedFiles} files for copying", availableFiles.Length);

            if (Directory.Exists(Configuration.TargetDirectory))
            {
                _logger.LogDebug("Directory {targetDirectory} exists already - cleaning as necessary", Configuration.TargetDirectory);
                if (Configuration.CleanFiles)
                {
                    _logger.LogTrace("Preparing to clean files");
                    foreach (var file in Directory.EnumerateFiles(Configuration.TargetDirectory))
                        await _fileSystemService.DeleteFile(file);
                }

                if (Configuration.CleanDirectories)
                {
                    _logger.LogTrace("Preparing to clean directories");
                    foreach (var dir in Directory.EnumerateDirectories(Configuration.TargetDirectory))
                        await _fileSystemService.DeleteDirectoryRecursive(dir);
                }
            }
            else
            {
                await _fileSystemService.CreateDirectory(Configuration.TargetDirectory);
                HuskyContext.UninstallOperations.AddEntry(UninstallOperationsList.EntryKind.Directory, Configuration.TargetDirectory);
            }

            foreach (var file in availableFiles)
            {
                _logger.LogDebug("Preparing to copy {file} from embedded resource", file);
                var destPath = Path.Combine(Configuration.TargetDirectory, file);
                var destDir = Path.GetDirectoryName(destPath);

                if (!string.IsNullOrWhiteSpace(destDir) && !Directory.Exists(destDir))
                {
                    await _fileSystemService.CreateDirectory(destDir);
                    HuskyContext.UninstallOperations.AddEntry(UninstallOperationsList.EntryKind.Directory, destDir);
                }

                await using var resourceStream = _embeddedResourceService.RetrieveResource(HuskyContext.InstallationAssembly, file);
                await _fileSystemService.WriteToFile(resourceStream, destPath, resourceStream.Length);
                HuskyContext.UninstallOperations.AddEntry(UninstallOperationsList.EntryKind.File, destPath);
            }
        }
    }
}