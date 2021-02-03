using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskConfiguration.Installation;
using Husky.Core.Workflow;
using Husky.Dependencies;
using Husky.Installer.Extensions;
using Husky.Services;
using Husky.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Installer
{
    public class HuskyInstaller
    {
        private HuskyInstallerSettings HuskyInstallerSettings { get; } = new();

        private readonly HuskyWorkflow _workflow;

        public HuskyInstaller(HuskyWorkflow workflow, Action<HuskyInstallerSettings> configureInstallation) : this(workflow)
            => configureInstallation(HuskyInstallerSettings);

        public HuskyInstaller(HuskyWorkflow workflow)
        {
            _workflow = workflow;
            _workflow.Stages.Insert(0, GeneratePreInstallationStage());
            _workflow.Stages.Add(GeneratePostInstallationStage());
        }

        public async ValueTask Install()
        {
            var serviceProvider = new ServiceCollection().AddHuskyInstaller(HuskyInstallerSettings, _workflow.Configuration);
            _workflow.Validate();

            foreach (var dependency in _workflow.Dependencies)
            {
                var dependencyHandler = GetDependencyHandler(dependency, serviceProvider);
                if (await dependencyHandler.IsAlreadyInstalled(dependency))
                {
                    // Todo: Log installed
                }
                else
                {
                    if (dependencyHandler.TrySatisfyDependency(dependency, out var acquisitionMethod))
                    {
                        await acquisitionMethod.AcquireDependency(serviceProvider);
                        // Todo: Verify installed (maybe call IsAlreadyInstalled again? :D
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to acquire dependency {dependency.GetType()}, installation will abort");
                    }
                }
            }

            foreach (var stage in _workflow.Stages)
            {
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using var scope = scopeFactory.CreateScope();
                await ExecuteStage(stage, scope.ServiceProvider);
            }
        }

        private async ValueTask ExecuteStage(HuskyStage stage, IServiceProvider services)
        {
            foreach (var job in stage.Jobs)
            {
                var installationContext = services.GetRequiredService<InstallationContext>();
                installationContext.CurrentJobName = job.Name;

                await ExecuteJob(job, installationContext, services);
            }
        }

        private async ValueTask ExecuteJob(HuskyJob job, InstallationContext installationContext, IServiceProvider services)
        {
            foreach (var step in job.Steps.Where(w => w.HuskyStepConfiguration.Os == CurrentPlatform.OS))
            {
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
            catch (Exception)
            {
                step.ExecutionInformation.Fail();
                /*
                 * Todo: Best mechanism here to initiate a rollback? Most likely returning early and implementing logic in the root Install method
                 * That checks to determine if *any* step hasn't completed successfully, begin rollback
                 */
                throw;
            }
            step.ExecutionInformation.Finish();
        }

        private static ValueTask ExecuteTask<T>(HuskyTask<T> task) where T : HuskyTaskConfiguration
        {
            return task.Execute();
        }

        private static IDependencyHandler<HuskyDependency> GetDependencyHandler(HuskyDependency dependency, IServiceProvider serviceProvider)
        {
            var dependencyType = dependency.GetType();
            var dependencyHandlerType = typeof(IDependencyHandler<>).MakeGenericType(dependencyType);
            return (IDependencyHandler<HuskyDependency>)serviceProvider.GetRequiredService(dependencyHandlerType);
        }

        private static HuskyStage GeneratePreInstallationStage()
            => HuskyWorkflow.Create()
                            .AddStage(HuskyConstants.Workflows.PreInstallation.DefaultStageName,
                                 stage => stage.AddJob(HuskyConstants.Workflows.PreInstallation.DefaultJobName,
                                     job => job.AddStep<VerifyMachineMeetsRequirementsOptions>(HuskyConstants.Workflows.PreInstallation.Steps.VerifyClientMachineMeetsRequirements)))
                            .Build()
                            .Stages[0];

        private static HuskyStage GeneratePostInstallationStage()
            => HuskyWorkflow.Create()
                            .AddStage(HuskyConstants.Workflows.PostInstallation.DefaultStageName,
                                 stage => stage.AddJob(HuskyConstants.Workflows.PostInstallation.DefaultJobName,
                                     job => job.AddStep<VerifyMachineMeetsRequirementsOptions>(
                                         HuskyConstants.Workflows.PostInstallation.Steps.PostInstallationApplicationRegistration)))
                            .Build()
                            .Stages[0];
    }
}