using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Tasks;
using Serilog;
using Serilog.Core;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowTaskExecutor
    {
        ValueTask ExecuteTask<T>(HuskyTask<T> task) where T : HuskyTaskConfiguration;
    }

    public class WorkflowTaskExecutor: IWorkflowTaskExecutor
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WorkflowTaskExecutor));

        public ValueTask ExecuteTask<T>(HuskyTask<T> task) where T : HuskyTaskConfiguration
        {
            _logger.Debug("Beginning execution of {task}", task.GetType().Name);
            return task.Execute();
        }
    }
}