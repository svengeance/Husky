﻿using System;
using System.Reflection;
using Husky.Core.Workflow;
using Husky.Services.Extensions;
using Husky.Tasks;
using Husky.Tasks.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Installer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider AddHuskyInstaller(this IServiceCollection serviceCollection, HuskyInstallerSettings huskyInstallerSettings, HuskyConfiguration huskyConfiguration)
        {
            foreach (var externalAssembly in huskyInstallerSettings.ResolveModulesFromAssemblies)
                HuskyTaskResolver.AddAssemblyForScanning(externalAssembly);

            serviceCollection.AddScoped(svc => new InstallationContext(Assembly.GetEntryAssembly()!));
            serviceCollection.AddHuskyServices();
            serviceCollection.AddHuskyTasks();

            foreach (var configurationBlock in huskyConfiguration.GetConfigurationBlocks())
                serviceCollection.AddSingleton(configurationBlock.GetType(), configurationBlock);

            return serviceCollection.BuildServiceProvider(validateScopes: true);
        }
    }
}