using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Tasks;
using Serilog;
using Serilog.Context;
using Serilog.Core;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowStageExecutor
    {
        ValueTask ExecuteStage(HuskyStage stage, HuskyContext huskyContext);
    }

    public class WorkflowStageExecutor: IWorkflowStageExecutor
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WorkflowStageExecutor));
        private readonly IWorkflowJobExecutor _jobExecutor;

        public WorkflowStageExecutor(IWorkflowJobExecutor jobExecutor) => _jobExecutor = jobExecutor;

        public async ValueTask ExecuteStage(HuskyStage stage, HuskyContext huskyContext)
        {
            foreach (var job in stage.Jobs)
            {
                _logger.Information("Executing job {job}", job.Name);
                using var jobScope = LogContext.PushProperty("Job", job.Name + ".");
                huskyContext.CurrentJobName = job.Name;

                await _jobExecutor.ExecuteJob(job, huskyContext);
            }
        }
    }
}