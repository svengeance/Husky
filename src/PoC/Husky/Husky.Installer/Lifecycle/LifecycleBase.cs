﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Husky.Core;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Dependencies;
using Husky.Installer.Extensions;
using Husky.Internal.Generator.Dictify;
using Husky.Services;
using Husky.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using ObjectFactory = Husky.Internal.Generator.Dictify.ObjectFactory;

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

        private readonly IServiceScopeFactory _scopeFactory;

        protected LifecycleBase(HuskyWorkflow workflow, HuskyInstallerSettings huskyInstallerSettings)
        {
            Workflow = workflow;
            HuskyInstallerSettings = huskyInstallerSettings;

            var serviceProvider = new ServiceCollection().AddHuskyInstaller(HuskyInstallerSettings, Workflow.Configuration);
            
            _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            Logger = LoggerFactory.CreateLogger(this.GetType().FullName);
            Logger.LogDebug("Constructed ServiceProvider");

            var installPath = Workflow.Configuration.GetConfigurationBlock<ApplicationConfiguration>().InstallDirectory;
            UninstallOperationsFile = Path.Combine(installPath, HuskyConstants.WorkflowDefaults.DefaultUninstallFileName);
        }

        public async ValueTask Execute()
        {
            await OnBeforeWorkflowExecute();

            var operationsList = await CreateContextUninstallOperationsList();
            var combinedVariables = Workflow.ExtractAllVariables();

            var huskyContext = new HuskyContext(LoggerFactory.CreateLogger<HuskyContext>(), operationsList, Assembly.GetEntryAssembly()!)
            {
                Variables = new Dictionary<string, object>(combinedVariables, StringComparer.InvariantCultureIgnoreCase)
            };

            await ExecuteWorkflow(huskyContext);
            Logger.LogInformation("Husky has successfully executed {tag}", HuskyInstallerSettings.TagToExecute);

            await OnAfterWorkflowExecute();
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

        private void Validate(Dictionary<string, object> variables)
        {
            using var scope = CreateServiceScope();
            var variableResolverService = scope.ServiceProvider.GetRequiredService<IVariableResolverService>();
            variableResolverService.ResolveVariables(variables);

            static void AppendExceptions(StringBuilder sb, IEnumerable<(string title, ValidationResult validation)> items)
                => items.Where(w => !w.validation.IsValid).Aggregate(sb, (prev, next) => prev.AppendLine(next.title).AppendLine(next.validation.ToString()));

            // Todo: Tasks which contain variables that are only computable at runtime may indeed be valid.
            var taskValidations = new List<(string, ValidationResult)>();
            foreach (var step in Workflow.EnumerateSteps())
            {
                foreach (var (key, val) in ((IDictable) step.HuskyTaskConfiguration).ToDictionary())
                    variables[key] = val;

                var configuredStep = (HuskyTaskConfiguration) ObjectFactory.Create(step.HuskyTaskConfiguration.GetType(), variables);

                taskValidations.Add((
                    $"{step.Name}.{step.HuskyTaskConfiguration.GetType().Name} is not appropriately configured",
                    configuredStep.Validate()
                ));

                step.HuskyTaskConfiguration = configuredStep;
            }

            //var taskValidations = Workflow.Stages.SelectMany(stage => stage.Jobs.SelectMany(job => job.Steps.Select(step =>
            //(
            //    $"{step.Name}.{step.HuskyTaskConfiguration.GetType().Name} is not appropriately configured",
            //    step.HuskyTaskConfiguration.Validate()
            //))));

            // Todo: Remove GetType().Name here
            var configurationValidations = Workflow.Configuration.GetAllConfigurationTypes()
                                                        .Select(s => (HuskyConfigurationBlock)ObjectFactory.Create(s, variables))
                                                        .Select(s => ($"{s.GetType().Name} is not appropriately configured", s.Validate()));

            var exceptions = new StringBuilder();
            AppendExceptions(exceptions, taskValidations);
            AppendExceptions(exceptions, configurationValidations);

            if (exceptions.Length > 0)
                throw new ValidationException(exceptions.ToString());
        }

        private async ValueTask ExecuteWorkflow(HuskyContext huskyContext)
        {
            // Todo: Replace variables here first before validation
            Validate(huskyContext.Variables);

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
            *  However, since I am only resolving and using the related type (i.e. we will *only* ever set Task1Configuration on Task1 here), this is somewhat safe
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
            huskyContext.AppendAllVariables(((IDictable) step.HuskyTaskConfiguration).ToDictionary());
            var taskOptions = variableResolver.Resolve(step.HuskyTaskConfiguration.GetType(), huskyContext.Variables);
            step.HuskyTaskConfiguration = (T) taskOptions;

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