using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Core.Workflow;
using Husky.Services;

namespace Husky.Tasks.Scripting
{
    public class ExecuteInlineScript : HuskyTask<ExecuteInlineScriptOptions>
    {
        private readonly IScriptingService _scriptingService;

        public ExecuteInlineScript(IScriptingService scriptingService)
        {
            _scriptingService = scriptingService;
        }
        
        protected override async Task ExecuteTask()
        {
            var exitCode = await _scriptingService.ExecuteCommand(Configuration.Script, Configuration.IsWindowVisible);
            if (exitCode != 0)
                throw new ApplicationException("Script execution resulted in non-0 exit code");
        }

        protected override Task RollbackTask() => Task.CompletedTask;
    }
}