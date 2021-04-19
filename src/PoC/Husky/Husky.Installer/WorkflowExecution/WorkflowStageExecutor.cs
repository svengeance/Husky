using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Tasks;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowStageExecutor
    {
        ValueTask ExecuteStage(HuskyStage stage, HuskyContext huskyContext);
    }

    public class WorkflowStageExecutor: IWorkflowStageExecutor
    {
        private readonly ILogger<WorkflowStageExecutor> _logger;
        private readonly IWorkflowJobExecutor _jobExecutor;

        public WorkflowStageExecutor(ILogger<WorkflowStageExecutor> logger, IWorkflowJobExecutor jobExecutor)
        {
            _logger = logger;
            _jobExecutor = jobExecutor;
        }

        public async ValueTask ExecuteStage(HuskyStage stage, HuskyContext huskyContext)
        {
            foreach (var job in stage.Jobs)
            {
                _logger.LogInformation("Executing job {job}", job.Name);
                using var jobScope = LogContext.PushProperty("Job", job.Name + ".");
                huskyContext.CurrentJobName = job.Name;

                await _jobExecutor.ExecuteJob(job, huskyContext);
            }
        }
    }
}