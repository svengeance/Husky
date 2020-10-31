using System;
using System.Collections.Generic;
using System.Linq;
using Husky.Core.HuskyConfiguration;

namespace Husky.Core.Workflow
{
    public class HuskyConfiguration
    {
        private readonly Dictionary<Type, IHuskyConfigurationBlock> _configurations;

        private HuskyConfiguration(Dictionary<Type, IHuskyConfigurationBlock> configurations) => _configurations = configurations;

        public void Configure<T>(Action<T> configuration) where T : class, IHuskyConfigurationBlock
            => configuration.Invoke((T) _configurations[typeof(T)]);

        public static HuskyConfiguration Create()
        {
            var configBlocks = typeof(ApplicationConfiguration)
                              .Assembly
                              .GetTypes()
                              .Where(w => w.GetInterfaces().Any(a => a == typeof(IHuskyConfigurationBlock)))
                              .Where(w => w.IsClass && !w.IsAbstract)
                              .ToDictionary(k => k, v => (IHuskyConfigurationBlock) Activator.CreateInstance(v));

            return new HuskyConfiguration(configBlocks);
        }
    }
}