﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Installation;
using Husky.Core.Workflow;
using Husky.Dependencies;
using Husky.Installer.Extensions;
using Husky.Services;
using Husky.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Husky.Installer
{
    public class HuskyInstaller
    {
        private readonly HuskyInstallerSettings _huskyInstallerSettings;

        private readonly HuskyWorkflow _workflow;

        private ILogger _logger;

        public HuskyInstaller(HuskyWorkflow workflow, HuskyInstallerSettings installationSettings)
        {
            _huskyInstallerSettings = installationSettings;
            _workflow = workflow;
            _workflow.Stages.Insert(0, GeneratePreInstallationStage());
            _workflow.Stages.Add(GeneratePostInstallationStage());

            if (_huskyInstallerSettings.TagToExecute == HuskyConstants.StepTags.Uninstall)
                _workflow.Reverse();
        }

        public async ValueTask Execute()
        {
            _workflow.Validate();

            var serviceProvider = new ServiceCollection().AddHuskyInstaller(_huskyInstallerSettings, _workflow.Configuration);
            _logger = serviceProvider.GetRequiredService<ILogger<HuskyInstaller>>();
            _logger.LogDebug("Constructed ServiceProvider");

            if (_huskyInstallerSettings.TagToExecute == HuskyConstants.StepTags.Install ||
                _huskyInstallerSettings.TagToExecute == HuskyConstants.StepTags.Repair)
            {
                var scopeProvider = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using var scope = scopeProvider.CreateScope();
                await InstallDependencies(scope.ServiceProvider);
            }

            foreach (var stage in _workflow.Stages)
            {
                using var stageScope = LogContext.PushProperty("Stage", stage.Name + ".");
                _logger.LogInformation("Executing stage {stage}", stage.Name);
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using var scope = scopeFactory.CreateScope();
                await ExecuteStage(stage, scope.ServiceProvider);
            }
        }

        private async ValueTask ExecuteStage(HuskyStage stage, IServiceProvider services)
        {
            foreach (var job in stage.Jobs)
            {
                _logger.LogInformation("Executing job {job}", job.Name);
                using var jobScope = LogContext.PushProperty("Job", job.Name + ".");
                var installationContext = services.GetRequiredService<InstallationContext>();
                installationContext.CurrentJobName = job.Name;

                await ExecuteJob(job, installationContext, services);
            }
        }

        private async ValueTask ExecuteJob(HuskyJob job, InstallationContext installationContext, IServiceProvider services)
        {
            var stepsToExecute = job.Steps.Where(w => w.HuskyStepConfiguration.Os == CurrentPlatform.OS && w.HuskyStepConfiguration.Tags.Contains(_huskyInstallerSettings.TagToExecute));
            foreach (var step in stepsToExecute)
            {
                _logger.LogInformation("Executing step {step}", step.Name);
                using var stepScope = LogContext.PushProperty("Step", step.Name + ":");
                installationContext.CurrentStepName = step.Name;

                await ExecuteStep(step, installationContext, services);
            }
        }

        private async ValueTask ExecuteStep<T>(HuskyStep<T> step, InstallationContext installationContext, IServiceProvider services) where T : HuskyTaskConfiguration
        {
            /* Todo: We currently have a "related type" issue, where we don't give a damn what type <T> is here, we just *know* it's a HuskyTaskConfiguration
            *  Unfortunately, the invariance on class-generic-types causes failures when trying to upcast T here, which is a *specific* configuration, to the base HTC.
            *  This *can* cause issues if we were to try to send *ANY OTHER* type other than the related type (i.e. send in Task1Configuration to a Task2)
            *  However, since I am only resolving and using the related type (i.e. we will *only* ever set ValueTask1Configuration on ValueTask1 here), this is somewhat safe
            *  In short, it may behoove us to get away from this if a different approach works better.
            */
            var taskType = HuskyTaskResolver.GetTaskForConfiguration(step.HuskyTaskConfiguration);
            var task = Unsafe.As<HuskyTask<T>>(services.GetRequiredService(taskType));
            _logger.LogDebug("Loaded task {task} for step {step}", taskType.Name, step.GetType().Name);

            var variableResolver = services.GetRequiredService<IVariableResolverService>();
            variableResolver.Resolve(step.HuskyTaskConfiguration, installationContext.Variables, _workflow.Variables, HuskyVariables.AsDictionary());

            task.SetExecutionContext(step.HuskyTaskConfiguration, installationContext, step.ExecutionInformation);

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
                _logger.LogInformation("Executed {step} with result: {executionResult}", step.Name, step.ExecutionInformation.ToString());
            }
            step.ExecutionInformation.Finish();
        }

        private ValueTask ExecuteTask<T>(HuskyTask<T> task) where T : HuskyTaskConfiguration
        {
            _logger.LogDebug("Beginning execution of {task}", task.GetType().Name);
            return task.Execute();
        }

        private static IDependencyHandler GetDependencyHandler(HuskyDependency dependency, IServiceProvider serviceProvider)
        {
            var dependencyHandlerResolver = serviceProvider.GetRequiredService<IDependencyHandlerResolver>();
            return dependencyHandlerResolver.Resolve(dependency);
        }

        private async Task InstallDependencies(IServiceProvider serviceProvider)
        {
            _logger.LogInformation("Analyzing {numberOfDependenciesToInstall} dependencies for potential installation", _workflow.Dependencies.Count);
            foreach (var dependency in _workflow.Dependencies)
            {
                var dependencyHandler = GetDependencyHandler(dependency, serviceProvider);
                if (await dependencyHandler.IsAlreadyInstalled())
                {
                    _logger.LogInformation("Dependency {dependency} is already installed -- skipping", dependency.GetType().Name);
                }
                else
                {
                    if (dependencyHandler.TrySatisfyDependency(out var acquisitionMethod))
                    {
                        _logger.LogInformation("Successfully located a handler for {dependency}, attempting to install", dependency.GetType().Name);
                        await acquisitionMethod.AcquireDependency(serviceProvider);
                        _logger.LogDebug("Successfully installed dependency {dependency}", dependency.GetType().Name);
                        
                        // Todo: Verify installed (maybe call IsAlreadyInstalled again? :D)
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to acquire dependency {dependency.GetType()}, installation will abort");
                    }
                }
            }
        }

        private static HuskyStage GeneratePreInstallationStage()
            => HuskyWorkflow.Create()
                            .AddStage(HuskyConstants.WorkflowDefaults.PreInstallation.StageName,
                                 stage => stage.AddJob(HuskyConstants.WorkflowDefaults.PreInstallation.JobName,
                                     job => job.AddStep<VerifyMachineMeetsRequirementsOptions>(HuskyConstants.WorkflowDefaults.PreInstallation.Steps.VerifyClientMachineMeetsRequirements,
                                         new HuskyStepConfiguration(CurrentPlatform.OS, HuskyConstants.StepTags.Install))))
                            .Build()
                            .Stages[0];

        private static HuskyStage GeneratePostInstallationStage()
            => HuskyWorkflow.Create()
                            .AddStage(HuskyConstants.WorkflowDefaults.PostInstallation.StageName,
                                 stage => stage.AddJob(HuskyConstants.WorkflowDefaults.PostInstallation.JobName,
                                     job => job.AddStep<PostInstallationApplicationRegistrationOptions>(HuskyConstants.WorkflowDefaults.PostInstallation.Steps.PostInstallationApplicationRegistration)))
                            .Build()
                            .Stages[0];
    }
}