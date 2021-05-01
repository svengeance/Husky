using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Platform;
using Husky.Core.Workflow;
using Husky.Tasks;
using Serilog;
using Serilog.Context;
using Serilog.Core;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowJobExecutor
    {
        ValueTask ExecuteJob(HuskyJob job, HuskyContext huskyContext);
    }

    public class WorkflowJobExecutor: IWorkflowJobExecutor
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WorkflowJobExecutor));
        private readonly IWorkflowStepExecutor _stepExecutor;

        public WorkflowJobExecutor(IWorkflowStepExecutor stepExecutor) => _stepExecutor = stepExecutor;

        public async ValueTask ExecuteJob(HuskyJob job, HuskyContext huskyContext)
        {
            var stepsToExecute = job.Steps.Where(w => w.HuskyStepConfiguration.Os == CurrentPlatform.OS && w.HuskyStepConfiguration.Tags.Contains(huskyContext.TagToExecute));
            foreach (var step in stepsToExecute)
            {
                _logger.Information("Executing step {step}", step.Name);
                using var stepScope = LogContext.PushProperty("Step", step.Name + ":");
                huskyContext.CurrentStepName = step.Name;

                await _stepExecutor.ExecuteStep(step, huskyContext);
            }
        }
    }
}