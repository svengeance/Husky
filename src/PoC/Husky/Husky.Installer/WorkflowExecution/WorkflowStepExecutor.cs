using System;
using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Internal.Generator.Dictify;
using Husky.Services;
using Husky.Tasks;
using Husky.Tasks.Infrastructure;
using Serilog;
using Serilog.Core;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowStepExecutor
    {
        ValueTask ExecuteStep<T>(HuskyStep<T> step, HuskyContext huskyContext) where T : HuskyTaskConfiguration;
    }

    public class WorkflowStepExecutor: IWorkflowStepExecutor
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WorkflowStepExecutor));
        private readonly IWorkflowTaskExecutor _taskExecutor;
        private readonly IVariableResolverService _variableResolverService;
        private readonly IHuskyTaskResolver _taskResolver;

        public WorkflowStepExecutor(IWorkflowTaskExecutor taskExecutor, IVariableResolverService variableResolverService, IHuskyTaskResolver taskResolver)
        {
            _taskExecutor = taskExecutor;
            _variableResolverService = variableResolverService;
            _taskResolver = taskResolver;
        }

        public async ValueTask ExecuteStep<T>(HuskyStep<T> step, HuskyContext huskyContext) where T : HuskyTaskConfiguration
        {
            //var taskType = HuskyTaskResolver.GetTaskForConfiguration(step.HuskyTaskConfiguration);
            //var task = Unsafe.As<HuskyTask<T>>(_serviceProvider.GetRequiredService(taskType));
            using var ownedTask = _taskResolver.ResolveTaskForConfiguration(typeof(T));
            var task = ownedTask.Value;
            _logger.Debug("Loaded task {task} for step {step}", typeof(T).Name, step.GetType().Name);


            huskyContext.AppendAllVariables(((IDictable)step.HuskyTaskConfiguration).ToDictionary());
            var taskOptions = _variableResolverService.Resolve(step.HuskyTaskConfiguration.GetType(), huskyContext.Variables);
            step.HuskyTaskConfiguration = (T)taskOptions;

            task.SetExecutionContext(step.HuskyTaskConfiguration, huskyContext, step.ExecutionInformation);

            step.ExecutionInformation.Start();

            /*
             * Todo: We should be catching exceptions in the Task Execution and returning a detailed Result of what failed.
             * In addition, Success cases should likewise be returning a receipt of the ValueTask-specific execution for analytics
             */

            try
            {
                await _taskExecutor.ExecuteTask(task);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to execute {step}", step.Name);
                step.ExecutionInformation.Fail();
                /*
                 * Todo: Best mechanism here to initiate a rollback? Most likely returning early and implementing logic in the root Install method
                 * That checks to determine if *any* step hasn't completed successfully, begin rollback
                 *
                 * Notes on rolling back: Reverse the current workflow, and rollback all tasks where executionstatus is error or completed.
                 */
                throw;
            }
            finally
            {
                await huskyContext.UninstallOperations.Flush();
                _logger.Information("Executed {step} with result: {executionResult}", step.Name, step.ExecutionInformation.ToString());
            }
            step.ExecutionInformation.Finish();
        }

    }
}