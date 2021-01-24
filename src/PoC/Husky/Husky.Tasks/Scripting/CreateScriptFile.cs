using System.IO;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Services;

namespace Husky.Tasks.Scripting
{
    public class CreateScriptFile : HuskyTask<CreateScriptFileOptions>
    {
        private readonly IFileSystemService _fileSystemService;

        public CreateScriptFile(IFileSystemService fileSystemService)
        {
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

            var existingScriptFile = new FileInfo(createdScriptFile);
            if (existingScriptFile.Exists)
                existingScriptFile.Delete();

            return ValueTask.CompletedTask;
        }
    }
}