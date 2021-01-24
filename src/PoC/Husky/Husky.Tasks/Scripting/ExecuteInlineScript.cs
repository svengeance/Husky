using System;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Services;

namespace Husky.Tasks.Scripting
{
    public class ExecuteInlineScript : HuskyTask<ExecuteInlineScriptOptions>
    {
        private readonly IShellExecutionService _shellExecutionService;

        public ExecuteInlineScript(IShellExecutionService shellExecutionService)
        {
            _shellExecutionService = shellExecutionService;
        }
        
        protected override async ValueTask ExecuteTask()
        {
            var scriptExecutionResult = await _shellExecutionService.ExecuteShellCommand(Configuration.Script);

            if (scriptExecutionResult.ExitCode != 0)
                throw new ApplicationException("Script execution resulted in non-0 exit code");
        }

        protected override ValueTask RollbackTask() => ValueTask.CompletedTask;
    }
}