using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Platform;
using Husky.Core.Workflow;
using Husky.Tasks;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowJobExecutor
    {
        ValueTask ExecuteJob(HuskyJob job, HuskyContext huskyContext);
    }

    public class WorkflowJobExecutor: IWorkflowJobExecutor
    {
        private readonly ILogger<WorkflowJobExecutor> _logger;
        private readonly IWorkflowStepExecutor _stepExecutor;

        public WorkflowJobExecutor(ILogger<WorkflowJobExecutor> logger, IWorkflowStepExecutor stepExecutor)
        {
            _logger = logger;
            _stepExecutor = stepExecutor;
        }

        public async ValueTask ExecuteJob(HuskyJob job, HuskyContext huskyContext)
        {
            var stepsToExecute = job.Steps.Where(w => w.HuskyStepConfiguration.Os == CurrentPlatform.OS && w.HuskyStepConfiguration.Tags.Contains(huskyContext.TagToExecute));
            foreach (var step in stepsToExecute)
            {
                _logger.LogInformation("Executing step {step}", step.Name);
                using var stepScope = LogContext.PushProperty("Step", step.Name + ":");
                huskyContext.CurrentStepName = step.Name;

                await _stepExecutor.ExecuteStep(step, huskyContext);
            }
        }
    }
}