using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Core.Workflow;
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

        private IServiceProvider _services;

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
            {
                HuskyTaskResolver.AddAssemblyForScanning(externalAssembly);
            }

            var services = new ServiceCollection();
            services.AddHuskyServices();
            services.AddHuskyTasks();

            _services = services.BuildServiceProvider(validateScopes: true);
            _workflow.Validate();

            foreach (var stage in _workflow.Stages)
            {
                await ExecuteStage(stage);
            }
        }

        private async Task ExecuteStage(HuskyStage stage)
        {
            foreach (var job in stage.Jobs)
            {
                await ExecuteJob(job);
            }
        }

        private async Task ExecuteJob(HuskyJob job)
        {
            foreach (var step in job.Steps)
            {
                await ExecuteStep(step);
            }
        }

        private async Task ExecuteStep<T>(HuskyStep<T> step) where T: HuskyTaskConfiguration
        {
            // Todo: We currently have a "related type" issue, where we don't give a damn what type <T> is here, we just *know* it's a HuskyTaskConfiguration
            // Unfortunately, the invariance on class-generic-types causes failures when trying to upcast T here, which is a *specific* configuration, to the base HTC.
            // This *can* cause issues if we were to try to send *ANY OTHER* type other than the related type (i.e. send in Task1Configuration to a Task2)
            // However, since I am only resolving and using the related type (i.e. we will *only* ever set Task1Configuration on Task1 here), this is somewhat safe
            // In short, it may behoove us to get away from this if a different approach works better.
            var taskType = HuskyTaskResolver.GetTaskForConfiguration(step.HuskyTaskConfiguration);
            var task = Unsafe.As<HuskyTask<T>>(_services.GetRequiredService(taskType));

            step.ExecutionInformation.Start();

            // Todo: We should be catching exceptions in the Task Execution and returning a detailed Result of what failed.
            // Todo: In addition, Success cases should likewise be returning a receipt of the task-specific execution for analytics
            try
            {
                await ExecuteTask(task, step.HuskyTaskConfiguration);
            }
            catch (Exception)
            {
                step.ExecutionInformation.Fail();
                // Todo: Best mechanism here to initiate a rollback? Most likely returning early and implementing logic in the root Install method
                // That checks to determine if *any* step hasn't completed successfully, begin rollback
                throw;
            }
            step.ExecutionInformation.Finish();
        }

        private Task ExecuteTask<T>(HuskyTask<T> task, HuskyTaskConfiguration huskyTaskConfiguration) where T: HuskyTaskConfiguration
        {
            task.SetConfiguration((T) huskyTaskConfiguration);
            return task.Execute();
        }
    }
}