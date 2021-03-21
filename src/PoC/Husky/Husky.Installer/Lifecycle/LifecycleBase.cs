using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Dependencies;
using Husky.Installer.Extensions;
using Husky.Services;
using Husky.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Husky.Installer.Lifecycle
{
    // Todo: Not in love with this name.
    public abstract class LifecycleBase
    {
        protected HuskyWorkflow Workflow { get; }

        protected HuskyInstallerSettings HuskyInstallerSettings { get; }

        protected ILogger Logger { get; private set; }

        protected readonly string UninstallOperationsFile;
        protected readonly ILoggerFactory LoggerFactory;

        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;

        protected LifecycleBase(HuskyWorkflow workflow, HuskyInstallerSettings huskyInstallerSettings)
        {
            Workflow = workflow;
            HuskyInstallerSettings = huskyInstallerSettings;

            _serviceProvider = new ServiceCollection().AddHuskyInstaller(HuskyInstallerSettings, Workflow.Configuration);
            _scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            LoggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            Logger = LoggerFactory.CreateLogger(this.GetType().FullName);
            Logger.LogDebug("Constructed ServiceProvider");

            var installPath = Workflow.Configuration.GetConfigurationBlock<ApplicationConfiguration>().InstallDirectory;
            UninstallOperationsFile = Path.Combine(installPath, HuskyConstants.WorkflowDefaults.DefaultUninstallFileName);
        }

        public async ValueTask Execute()
        {
            await OnBeforeWorkflowExecute();

            var operationsList = await CreateContextUninstallOperationsList();
            var huskyContext = ActivatorUtilities.CreateInstance<HuskyContext>(_serviceProvider, operationsList, Assembly.GetEntryAssembly()!);

            await ExecuteWorkflow(huskyContext);
            Logger.LogInformation("Husky has successfully executed {tag}", HuskyInstallerSettings.TagToExecute);
        }

        protected virtual bool ShouldExecuteStep<T>(HuskyStep<T> step) where T: HuskyTaskConfiguration
            => step.HuskyStepConfiguration.Os == CurrentPlatform.OS &&
               step.HuskyStepConfiguration.Tags.Contains(HuskyInstallerSettings.TagToExecute);

        protected virtual ValueTask OnBeforeWorkflowExecute() => ValueTask.CompletedTask;

        protected virtual ValueTask OnAfterWorkflowExecute() => ValueTask.CompletedTask;

        protected abstract Task<IUninstallOperationsList> CreateContextUninstallOperationsList();

        protected IServiceScope CreateServiceScope() => _scopeFactory.CreateScope();

        protected async Task InstallDependencies()
        {
            using var scope = CreateServiceScope();
            var serviceProvider = scope.ServiceProvider;
            Logger.LogInformation("Analyzing {numberOfDependenciesToInstall} dependencies for potential installation", Workflow.Dependencies.Count);
            foreach (var dependency in Workflow.Dependencies)
            {
                var dependencyHandler = GetDependencyHandler(dependency, serviceProvider);
                if (await dependencyHandler.IsAlreadyInstalled())
                {
                    Logger.LogInformation("Dependency {dependency} is already installed -- skipping", dependency.GetType().Name);
                }
                else
                {
                    if (dependencyHandler.TrySatisfyDependency(out var acquisitionMethod))
                    {
                        Logger.LogInformation("Successfully located a handler for {dependency}, attempting to install", dependency.GetType().Name);
                        await acquisitionMethod.AcquireDependency(serviceProvider);
                        Logger.LogDebug("Successfully installed dependency {dependency}", dependency.GetType().Name);

                        // Todo: Verify installed (maybe call IsAlreadyInstalled again? :D)
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to acquire dependency {dependency.GetType()}, installation will abort");
                    }
                }
            }
        }

        private async ValueTask ExecuteWorkflow(HuskyContext huskyContext)
        {
            // Todo: Replace variables here first before validation
            Workflow.Validate();

            if (HuskyInstallerSettings.TagToExecute == HuskyConstants.StepTags.Install)
                await InstallDependencies();
            
            foreach (var stageToExecute in Workflow.Stages)
            {
                if (!(Workflow.Stages.Any(stage => stage.Jobs.Any(job => job.Steps.Any(ShouldExecuteStep)))))
                {
                    Logger.LogInformation("No steps can be executed for stage {stage} -- skipping", stageToExecute.Name);
                    continue;
                }

                using var stageScope = LogContext.PushProperty("Stage", stageToExecute.Name + ".");
                Logger.LogInformation("Executing stage {stage}", stageToExecute.Name);
                using var scope = CreateServiceScope();
                await ExecuteStage(stageToExecute, scope.ServiceProvider, huskyContext);
            }
        }

        private async ValueTask ExecuteStage(HuskyStage stage, IServiceProvider services, HuskyContext huskyContext)
        {
            foreach (var job in stage.Jobs)
            {
                Logger.LogInformation("Executing job {job}", job.Name);
                using var jobScope = LogContext.PushProperty("Job", job.Name + ".");
                huskyContext.CurrentJobName = job.Name;

                await ExecuteJob(job, huskyContext, services);
            }
        }

        private async ValueTask ExecuteJob(HuskyJob job, HuskyContext huskyContext, IServiceProvider services)
        {
            var stepsToExecute = job.Steps.Where(w => w.HuskyStepConfiguration.Os == CurrentPlatform.OS && w.HuskyStepConfiguration.Tags.Contains(HuskyInstallerSettings.TagToExecute));
            foreach (var step in stepsToExecute)
            {
                Logger.LogInformation("Executing step {step}", step.Name);
                using var stepScope = LogContext.PushProperty("Step", step.Name + ":");
                huskyContext.CurrentStepName = step.Name;

                await ExecuteStep(step, huskyContext, services);
            }
        }

        private async ValueTask ExecuteStep<T>(HuskyStep<T> step, HuskyContext huskyContext, IServiceProvider services) where T : HuskyTaskConfiguration
        {
            /* Todo: We currently have a "related type" issue, where we don't give a damn what type <T> is here, we just *know* it's a HuskyTaskConfiguration
            *  Unfortunately, the invariance on class-generic-types causes failures when trying to upcast T here, which is a *specific* configuration, to the base HTC.
            *  This *can* cause issues if we were to try to send *ANY OTHER* type other than the related type (i.e. send in Task1Configuration to a Task2)
            *  However, since I am only resolving and using the related type (i.e. we will *only* ever set ValueTask1Configuration on ValueTask1 here), this is somewhat safe
            *  In short, it may behoove us to get away from this if a different approach works better.
            */
            /*
             * Todo: Consider moving the below OUT and into a factory service to clean up this mess. We may also be able to escape our related type
             * issue at the same time if we're clever enough.
             * I have more confidence in future me than present me, though.
             */
            var taskType = HuskyTaskResolver.GetTaskForConfiguration(step.HuskyTaskConfiguration);
            var task = Unsafe.As<HuskyTask<T>>(services.GetRequiredService(taskType));
            Logger.LogDebug("Loaded task {task} for step {step}", taskType.Name, step.GetType().Name);

            var variableResolver = services.GetRequiredService<IVariableResolverService>();
            variableResolver.Resolve(step.HuskyTaskConfiguration, huskyContext.Variables, Workflow.Variables, HuskyVariables.AsDictionary());

            task.SetExecutionContext(step.HuskyTaskConfiguration, huskyContext, step.ExecutionInformation);

            step.ExecutionInformation.Start();

            /*
             * Todo: We should be catching exceptions in the Task Execution and returning a detailed Result of what failed.
             * In addition, Success cases should likewise be returning a receipt of the ValueTask-specific execution for analytics
             */

            try
            {
                await ExecuteTask(task);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to execute {step}", step.Name);
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
                Logger.LogInformation("Executed {step} with result: {executionResult}", step.Name, step.ExecutionInformation.ToString());
            }
            step.ExecutionInformation.Finish();
        }

        private ValueTask ExecuteTask<T>(HuskyTask<T> task) where T : HuskyTaskConfiguration
        {
            Logger.LogDebug("Beginning execution of {task}", task.GetType().Name);
            return task.Execute();
        }

        private static IDependencyHandler GetDependencyHandler(HuskyDependency dependency, IServiceProvider serviceProvider)
        {
            var dependencyHandlerResolver = serviceProvider.GetRequiredService<IDependencyHandlerResolver>();
            return dependencyHandlerResolver.Resolve(dependency);
        }
    }
}