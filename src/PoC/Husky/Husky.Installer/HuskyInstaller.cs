using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Workflow;
using Husky.Services;
using Husky.Services.Extensions;
using Husky.Tasks;
using Husky.Tasks.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Installer
{
    public class HuskyInstaller
    {
        private InstallationConfiguration Configuration { get; }

        private readonly HuskyWorkflow _workflow;

        public HuskyInstaller(HuskyWorkflow workflow, Action<InstallationConfiguration> configureInstallation): this(workflow)
            => configureInstallation(Configuration = new InstallationConfiguration());

        public HuskyInstaller(HuskyWorkflow workflow)
        {
            _workflow = workflow;
        }

        public async Task Install()
        {
            // Todo: Register services from external assemblies
            // Todo: Move this to a cleaner home?
            foreach (var externalAssembly in Configuration.ResolveModulesFromAssemblies)
                HuskyTaskResolver.AddAssemblyForScanning(externalAssembly);

            var services = new ServiceCollection();
            services.AddScoped<InstallationContext>();
            services.AddHuskyServices();
            services.AddHuskyTasks();
            /*
             * Todo: Maybe make the internal configurations visible and do the registrations here? Seems like a slight bit of cross-contamination to have the
             * models/public config be responsible for its own registration
             */
            _workflow.Configuration.AddConfigurationToServiceCollection(services);

            var servicesRoot = services.BuildServiceProvider(validateScopes: true);
            _workflow.Validate();

            foreach (var stage in _workflow.Stages)
            {
                var scopeFactory = servicesRoot.GetRequiredService<IServiceScopeFactory>();
                using var scope = scopeFactory.CreateScope();
                await ExecuteStage(stage, scope.ServiceProvider);
            }
        }

        private async Task ExecuteStage(HuskyStage stage, IServiceProvider services)
        {
          
            foreach (var job in stage.Jobs)
            {
                await ExecuteJob(job, services); 
            }
        }

        private async Task ExecuteJob(HuskyJob job, IServiceProvider services)
        {
            foreach (var step in job.Steps)
            {
                await ExecuteStep(step, services);
            }
        }

        private async Task ExecuteStep<T>(HuskyStep<T> step, IServiceProvider services) where T: HuskyTaskConfiguration
        {
            /* Todo: We currently have a "related type" issue, where we don't give a damn what type <T> is here, we just *know* it's a HuskyTaskConfiguration
            *  Unfortunately, the invariance on class-generic-types causes failures when trying to upcast T here, which is a *specific* configuration, to the base HTC.
            *  This *can* cause issues if we were to try to send *ANY OTHER* type other than the related type (i.e. send in Task1Configuration to a Task2)
            *  However, since I am only resolving and using the related type (i.e. we will *only* ever set Task1Configuration on Task1 here), this is somewhat safe
            *  In short, it may behoove us to get away from this if a different approach works better.
            */
            var taskType = HuskyTaskResolver.GetTaskForConfiguration(step.HuskyTaskConfiguration);
            var task = Unsafe.As<HuskyTask<T>>(services.GetRequiredService(taskType));
            
            var installationContext = services.GetRequiredService<InstallationContext>();
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

        private Task ExecuteTask<T>(HuskyTask<T> task) where T: HuskyTaskConfiguration
        {
            return task.Execute();
        }
    }
}