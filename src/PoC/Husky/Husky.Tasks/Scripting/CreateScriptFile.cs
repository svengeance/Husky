using System.IO;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Services;

namespace Husky.Tasks.Scripting
{
    public class CreateScriptFile : HuskyTask<CreateScriptFileOptions>
    {
        private readonly IScriptingService _scriptingService;

        public CreateScriptFile(IScriptingService scriptingService)
        {
            _scriptingService = scriptingService;
        }

        protected override async Task ExecuteTask()
        {
            var createdScriptFile = await _scriptingService.CreateScriptFile(Configuration.Directory, Configuration.FileName, Configuration.Script);
            InstallationContext.SetVariable("CreatedFileName", createdScriptFile);
        }

        protected override Task RollbackTask()
        {
            var createdExtension = _scriptingService.GetScriptFileExtension();
            var createdScriptFile = Path.Combine(Configuration.Directory, Configuration.FileName + createdExtension);

            var existingScriptFile = new FileInfo(createdScriptFile);
            if (existingScriptFile.Exists)
                existingScriptFile.Delete();

            return Task.CompletedTask;
        }
    }
}