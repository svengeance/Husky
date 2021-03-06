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
            HuskyContext.SetVariable("createdFileName", createdScriptFile);
        }
    }
}