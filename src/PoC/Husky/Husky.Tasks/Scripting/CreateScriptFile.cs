using System.Threading.Tasks;
using Husky.Core.TaskOptions.Scripting;
using Husky.Core.Workflow.Uninstallation;
using Husky.Services;

namespace Husky.Tasks.Scripting
{
    public class CreateScriptFile : HuskyTask<CreateScriptFileOptions>
    {
        private readonly IFileSystemService _fileSystemService;

        public CreateScriptFile(IFileSystemService fileSystemService) => _fileSystemService = fileSystemService;

        protected override async ValueTask ExecuteTask()
        {
            var createdScriptFile = await _fileSystemService.CreateScriptFile(Configuration.Directory, Configuration.FileName, Configuration.Script);
            HuskyContext.SetCurrentTaskVariable("createdFileName", createdScriptFile);
            HuskyContext.UninstallOperations.AddEntry(UninstallOperationsList.EntryKind.File, createdScriptFile);
        }
    }
}