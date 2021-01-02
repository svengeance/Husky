using System;
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
        public static IServiceProvider AddHuskyInstaller(this IServiceCollection serviceCollection, InstallationConfiguration installationConfiguration, HuskyConfiguration huskyConfiguration)
        {
            foreach (var externalAssembly in installationConfiguration.ResolveModulesFromAssemblies)
                HuskyTaskResolver.AddAssemblyForScanning(externalAssembly);

            var services = new ServiceCollection();
            services.AddScoped<InstallationContext>(svc => new InstallationContext(Assembly.GetEntryAssembly()!));
            services.AddHuskyServices();
            services.AddHuskyTasks();
            /*
             * Todo: Maybe make the internal configurations visible and do the registrations here? Seems like a slight bit of cross-contamination to have the
             * models/public config be responsible for its own registration
             */
            huskyConfiguration.AddConfigurationToServiceCollection(services);

            return services.BuildServiceProvider(validateScopes: true);
        }
    }
}