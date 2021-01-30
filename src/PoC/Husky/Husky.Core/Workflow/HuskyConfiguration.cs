using System;
using System.Collections.Generic;
using System.Linq;
using Husky.Core.HuskyConfiguration;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Core.Workflow
{
    public class HuskyConfiguration
    {
        private readonly Dictionary<Type, HuskyConfigurationBlock> _configurations;

        public IReadOnlyList<HuskyConfigurationBlock> GetConfigurationBlocks()
            => _configurations.Values
                              .Select(s => s with { })
                              .ToList()
                              .AsReadOnly();

        private HuskyConfiguration(Dictionary<Type, HuskyConfigurationBlock> configurations) => _configurations = configurations;

        public void Configure<T>(Action<T> configuration) where T : HuskyConfigurationBlock
            => configuration.Invoke((T) _configurations[typeof(T)]);

        public static HuskyConfiguration Create()
        {
            // Instantiate an instance of each configuration and cache it by its type
            var configBlocks = typeof(ApplicationConfiguration)
                              .Assembly
                              .GetTypes()
                              .Where(w => w.BaseType == typeof(HuskyConfigurationBlock))
                              .Where(w => w.IsClass && !w.IsAbstract)
                              .ToDictionary(k => k, v => (HuskyConfigurationBlock) Activator.CreateInstance(v)!);

            return new HuskyConfiguration(configBlocks);
        }
    }
}