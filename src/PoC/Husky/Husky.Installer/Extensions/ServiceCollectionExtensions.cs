using System;
using Husky.Core.Workflow;
using Husky.Dependencies.Extensions;
using Husky.Installer.WorkflowExecution;
using Husky.Services.Extensions;
using Husky.Tasks;
using Husky.Tasks.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Husky.Installer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider AddHuskyInstaller(this IServiceCollection serviceCollection, HuskyInstallerSettings huskyInstallerSettings, HuskyConfiguration huskyConfiguration)
        {
            foreach (var externalAssembly in huskyInstallerSettings.ResolveModulesFromAssemblies)
                HuskyTaskResolver.AddAssemblyForScanning(externalAssembly);

            serviceCollection.AddLogging(logging => logging.AddSerilog());
            serviceCollection.AddHuskyDependencies();
            serviceCollection.AddHuskyServices();
            serviceCollection.AddHuskyTasks();

            serviceCollection.AddScoped<IWorkflowDependencyInstaller, WorkflowDependencyInstaller>();
            serviceCollection.AddScoped<IWorkflowExecutor, WorkflowExecutor>();
            serviceCollection.AddScoped<IWorkflowJobExecutor, WorkflowJobExecutor>();
            serviceCollection.AddScoped<IWorkflowStageExecutor, WorkflowStageExecutor>();
            serviceCollection.AddScoped<IWorkflowStepExecutor, WorkflowStepExecutor>();
            serviceCollection.AddScoped<IWorkflowTaskExecutor, WorkflowTaskExecutor>();
            serviceCollection.AddScoped<IWorkflowValidator, WorkflowValidator>();

            foreach (var configurationBlock in huskyConfiguration.GetConfigurationBlocks())
                serviceCollection.AddSingleton(configurationBlock.GetType(), configurationBlock);

            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            var logger = serviceProvider.GetRequiredService<ILogger<HuskyInstaller>>();
            foreach (var service in serviceCollection)
                logger.LogTrace("Registered {service} implemented by {implementation} as {lifetime}", service.ServiceType, service.ImplementationType?.ToString() ?? "Factory", service.Lifetime);

            return serviceProvider;
        }
    }
}