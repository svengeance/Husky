using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Workflow;
using Husky.Installer.Extensions;
using Husky.Services;
using Husky.Services.Extensions;
using Husky.Tasks;
using Husky.Tasks.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Installer
{
    public class HuskyInstaller
    {
        private InstallationConfiguration InstallationConfiguration { get; } = new();

        private readonly HuskyWorkflow _workflow;

        public HuskyInstaller(HuskyWorkflow workflow, Action<InstallationConfiguration> configureInstallation): this(workflow)
            => configureInstallation(InstallationConfiguration);

        public HuskyInstaller(HuskyWorkflow workflow)
        {
            _workflow = workflow;
        }

        public async Task Install()
        {
            var serviceProvider = new ServiceCollection().AddHuskyInstaller(InstallationConfiguration, _workflow.Configuration);
            _workflow.Validate();

            foreach (var stage in _workflow.Stages)
            {
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using var scope = scopeFactory.CreateScope();
                await ExecuteStage(stage, scope.ServiceProvider);
            }
        }

        private async Task ExecuteStage(HuskyStage stage, IServiceProvider services)
        {
            foreach (var job in stage.Jobs)
            {
                var installationContext = services.GetRequiredService<InstallationContext>();
                installationContext.CurrentJobName = job.Name;
                
                await ExecuteJob(job, installationContext, services); 
            }
        }

        private async Task ExecuteJob(HuskyJob job, InstallationContext installationContext, IServiceProvider services)
        {
            foreach (var step in job.Steps.Where(w => w.HuskyStepConfiguration.SupportedPlatforms.IsCurrentPlatformSupported()))
            {
                installationContext.CurrentStepName = step.Name;
                
                await ExecuteStep(step, installationContext, services);
            }
        }

        private async Task ExecuteStep<T>(HuskyStep<T> step, InstallationContext installationContext, IServiceProvider services) where T: HuskyTaskConfiguration
        {
            /* Todo: We currently have a "related type" issue, where we don't give a damn what type <T> is here, we just *know* it's a HuskyTaskConfiguration
            *  Unfortunately, the invariance on class-generic-types causes failures when trying to upcast T here, which is a *specific* configuration, to the base HTC.
            *  This *can* cause issues if we were to try to send *ANY OTHER* type other than the related type (i.e. send in Task1Configuration to a Task2)
            *  However, since I am only resolving and using the related type (i.e. we will *only* ever set Task1Configuration on Task1 here), this is somewhat safe
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
             * In addition, Success cases should likewise be returning a receipt of the task-specific execution for analytics
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

        private static Task ExecuteTask<T>(HuskyTask<T> task) where T: HuskyTaskConfiguration
        {
            return task.Execute();
        }
    }
}