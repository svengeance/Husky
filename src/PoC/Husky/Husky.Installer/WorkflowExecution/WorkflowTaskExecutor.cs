using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Tasks;
using Microsoft.Extensions.Logging;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowTaskExecutor
    {
        ValueTask ExecuteTask<T>(HuskyTask<T> task) where T : HuskyTaskConfiguration;
    }

    public class WorkflowTaskExecutor: IWorkflowTaskExecutor
    {
        private readonly ILogger _logger;

        public WorkflowTaskExecutor(ILogger<WorkflowTaskExecutor> logger) => _logger = logger;

        public ValueTask ExecuteTask<T>(HuskyTask<T> task) where T : HuskyTaskConfiguration
        {
            _logger.LogDebug("Beginning execution of {task}", task.GetType().Name);
            return task.Execute();
        }
    }
}