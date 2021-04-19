using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Internal.Generator.Dictify;
using Husky.Services;
using Husky.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowStepExecutor
    {
        ValueTask ExecuteStep<T>(HuskyStep<T> step, HuskyContext huskyContext) where T : HuskyTaskConfiguration;
    }

    public class WorkflowStepExecutor: IWorkflowStepExecutor
    {
        private readonly ILogger _logger;
        private readonly IWorkflowTaskExecutor _taskExecutor;
        private readonly IVariableResolverService _variableResolverService;
        private readonly IServiceProvider _serviceProvider;

        public WorkflowStepExecutor(ILogger<WorkflowStepExecutor> logger, IWorkflowTaskExecutor taskExecutor, IVariableResolverService variableResolverService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _taskExecutor = taskExecutor;
            _variableResolverService = variableResolverService;
            _serviceProvider = serviceProvider;
        }

        public async ValueTask ExecuteStep<T>(HuskyStep<T> step, HuskyContext huskyContext) where T : HuskyTaskConfiguration
        {
            /* Todo: We currently have a "related type" issue, where we don't give a damn what type <T> is here, we just *know* it's a HuskyTaskConfiguration
            *  Unfortunately, the invariance on class-generic-types causes failures when trying to upcast T here, which is a *specific* configuration, to the base HTC.
            *  This *can* cause issues if we were to try to send *ANY OTHER* type other than the related type (i.e. send in Task1Configuration to a Task2)
            *  However, since I am only resolving and using the related type (i.e. we will *only* ever set Task1Configuration on Task1 here), this is somewhat safe
            *  In short, it may behoove us to get away from this if a different approach works better.
            */
            /*
             * Todo: Consider moving the below OUT and into a factory service to clean up this mess. We may also be able to escape our related type
             * issue at the same time if we're clever enough.
             * I have more confidence in future me than present me, though.
             */
            var taskType = HuskyTaskResolver.GetTaskForConfiguration(step.HuskyTaskConfiguration);
            var task = Unsafe.As<HuskyTask<T>>(_serviceProvider.GetRequiredService(taskType));
            _logger.LogDebug("Loaded task {task} for step {step}", taskType.Name, step.GetType().Name);


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
                _logger.LogError(e, "Failed to execute {step}", step.Name);
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
                _logger.LogInformation("Executed {step} with result: {executionResult}", step.Name, step.ExecutionInformation.ToString());
            }
            step.ExecutionInformation.Finish();
        }

    }
}