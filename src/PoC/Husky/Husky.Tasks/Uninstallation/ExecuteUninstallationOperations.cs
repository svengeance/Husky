using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.TaskOptions.Uninstallation;
using Husky.Core.Workflow.Uninstallation;
using Husky.Services;
using Microsoft.Extensions.Logging;

namespace Husky.Tasks.Uninstallation
{
    public class ExecuteUninstallationOperations: HuskyTask<ExecuteUninstallationOperationsOptions>
    {
        private readonly ILogger _logger;
        private readonly HuskyContext _huskyContext;
        private readonly IFileSystemService _fileSystemService;
        private readonly IRegistryService _registryService;

        public ExecuteUninstallationOperations(ILogger<ExecuteUninstallationOperations> logger, HuskyContext huskyContext,
            IFileSystemService fileSystemService, IRegistryService registryService)
        {
            _logger = logger;
            _huskyContext = huskyContext;
            _fileSystemService = fileSystemService;
            _registryService = registryService;
        }

        protected override async ValueTask ExecuteTask()
        {
            var operations = _huskyContext.UninstallOperations;

            _logger.LogInformation("Executing file uninstallation operations");
            foreach (var file in operations.ReadEntries(UninstallOperationsList.EntryKind.File))
                await _fileSystemService.DeleteFile(file);

            _logger.LogInformation("Executing directory uninstallation operations");
            foreach (var directory in operations.ReadEntries(UninstallOperationsList.EntryKind.Directory))
                await _fileSystemService.DeleteDirectory(directory);

            _logger.LogInformation("Executing registry value uninstallation operations");
            foreach (var regKeyValuePath in operations.ReadEntries(UninstallOperationsList.EntryKind.RegistryValue))
                _registryService.RemoveKeyValue(regKeyValuePath
);

            _logger.LogInformation("Executing registry key uninstallation operations");
            foreach (var regKeyPath in operations.ReadEntries(UninstallOperationsList.EntryKind.RegistryKey))
                _registryService.RemoveKey(regKeyPath);
        }
    }
}