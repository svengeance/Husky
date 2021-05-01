using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.TaskOptions.Resources;
using Husky.Core.Workflow.Uninstallation;
using Husky.Services;
using Serilog;
using Serilog.Core;

namespace Husky.Tasks.Resources
{
    public class ExtractBundledResource : HuskyTask<ExtractBundledResourceOptions>
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ExtractBundledResource));
        private readonly IEmbeddedResourceService _embeddedResourceService;
        private readonly IFileSystemService _fileSystemService;

        public ExtractBundledResource(IEmbeddedResourceService embeddedResourceService, IFileSystemService fileSystemService)
        {
            _embeddedResourceService = embeddedResourceService;
            _fileSystemService = fileSystemService;
        }

        protected override async ValueTask ExecuteTask()
        {
            var availableFiles = _embeddedResourceService.ListResources(HuskyContext.InstallationAssembly, Configuration.Resources).ToArray();
            _logger.Debug("Located {numberOfFilteredEmbeddedFiles} files for copying", availableFiles.Length);

            if (Directory.Exists(Configuration.TargetDirectory))
            {
                _logger.Debug("Directory {targetDirectory} exists already - cleaning as necessary", Configuration.TargetDirectory);
                if (Configuration.CleanFiles)
                {
                    _logger.Verbose("Preparing to clean files");
                    foreach (var file in Directory.EnumerateFiles(Configuration.TargetDirectory))
                        await _fileSystemService.DeleteFile(file);
                }

                if (Configuration.CleanDirectories)
                {
                    _logger.Verbose("Preparing to clean directories");
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
                _logger.Debug("Preparing to copy {file} from embedded resource", file);
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