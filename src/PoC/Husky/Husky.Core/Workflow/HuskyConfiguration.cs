using System;
using System.Collections.Generic;
using System.Linq;
using Husky.Core.HuskyConfiguration;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Core.Workflow
{
    public class HuskyConfiguration
    {
        private readonly Dictionary<Type, IHuskyConfigurationBlock> _configurations;

        private HuskyConfiguration(Dictionary<Type, IHuskyConfigurationBlock> configurations) => _configurations = configurations;

        internal void Configure<T>(Action<T> configuration) where T : class, IHuskyConfigurationBlock
            => configuration.Invoke((T) _configurations[typeof(T)]);

        // Todo: Not sure if this belongs here. Could expose a readonly to the public & register in HuskyInstaller.
        public void AddConfigurationToServiceCollection(IServiceCollection collection)
        {
            foreach (var typeAndConfiguration in _configurations)
                collection.AddSingleton(typeAndConfiguration.Key, typeAndConfiguration.Value);
        }

        public static HuskyConfiguration Create()
        {
            // Instantiate an instance of each configuration and cache it by its type
            var configBlocks = typeof(ApplicationConfiguration)
                              .Assembly
                              .GetTypes()
                              .Where(w => w.GetInterfaces().Any(a => a == typeof(IHuskyConfigurationBlock)))
                              .Where(w => w.IsClass && !w.IsAbstract)
                              .ToDictionary(k => k, v => (IHuskyConfigurationBlock) Activator.CreateInstance(v)!);

            return new HuskyConfiguration(configBlocks);
        }
    }
}