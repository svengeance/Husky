using System.IO;
using System.Threading.Tasks;
using Husky.Core.TaskOptions.Scripting;
using Husky.Services;
using Microsoft.Extensions.Logging;

namespace Husky.Tasks.Scripting
{
    public class CreateScriptFile : HuskyTask<CreateScriptFileOptions>
    {
        private readonly ILogger _logger;
        private readonly IFileSystemService _fileSystemService;

        public CreateScriptFile(ILogger<CreateScriptFile> logger, IFileSystemService fileSystemService)
        {
            _logger = logger;
            _fileSystemService = fileSystemService;
        }

        protected override async ValueTask ExecuteTask()
        {
            var createdScriptFile = await _fileSystemService.CreateScriptFile(Configuration.Directory, Configuration.FileName, Configuration.Script);
            InstallationContext.SetVariable("CreatedFileName", createdScriptFile);
        }

        protected override ValueTask RollbackTask()
        {
            var createdExtension = _fileSystemService.GetScriptFileExtension();
            var createdScriptFile = Path.Combine(Configuration.Directory, Configuration.FileName + createdExtension);
            _logger.LogDebug("Removing script file if exists at {scriptFilePath}", createdScriptFile);

            var existingScriptFile = new FileInfo(createdScriptFile);
            if (existingScriptFile.Exists)
                existingScriptFile.Delete();

            return ValueTask.CompletedTask;
        }
    }
}