﻿using System;
using System.Collections.Generic;
using System.Linq;
using Husky.Core.HuskyConfiguration;
using Husky.Internal.Generator.Dictify;

namespace Husky.Core.Workflow
{
    public class HuskyConfiguration
    {
        private readonly Dictionary<Type, HuskyConfigurationBlock> _configurations;

        public IEnumerable<HuskyConfigurationBlock> GetConfigurationBlocks()
            => _configurations.Values
                              .Select(s => (s with { }))
                              .ToList()
                              .AsReadOnly()!;

        public IEnumerable<Type> GetAllConfigurationTypes() => _configurations.Keys;

        public IReadOnlyDictionary<string, object> ExtractConfigurationBlockVariables()
            => new Dictionary<string, object>(_configurations.SelectMany(s => ((IDictable) s.Value).ToDictionary()));

        private HuskyConfiguration(Dictionary<Type, HuskyConfigurationBlock> configurations) => _configurations = configurations;

        public T GetConfigurationBlock<T>() where T: HuskyConfigurationBlock => (T) _configurations[typeof(T)];

        public void Configure<T>(Action<T> configuration) where T : HuskyConfigurationBlock
            => configuration.Invoke((T) _configurations[typeof(T)]);

        public static HuskyConfiguration Create()
        {
            // Todo: Replace with an internal sgen to avoid reflection!
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