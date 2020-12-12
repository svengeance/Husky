using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Husky.Core.Workflow;

namespace Husky.Tasks
{
    public static class HuskyTaskResolver
    {
        private static readonly Dictionary<Type, Type> TasksByConfigurationType;

        static HuskyTaskResolver() => TasksByConfigurationType = ResolveHuskyTasks(Assembly.GetExecutingAssembly());

        internal static IEnumerable<Type> GetAvailableTasks() => TasksByConfigurationType.Values;

        internal static void AddAssemblyForScanning(Assembly assembly)
        {
            foreach (var (key, val) in ResolveHuskyTasks(assembly))
                TasksByConfigurationType[key] = val;
        }

        private static Dictionary<Type, Type> ResolveHuskyTasks(Assembly assembly)
            => assembly.GetExportedTypes()
                       .Where(w => w.BaseType?.GetGenericArguments().Length > 0 && w.BaseType?.GetGenericArguments().FirstOrDefault()?.BaseType == typeof(HuskyTaskConfiguration))
                       .ToDictionary(k => k.BaseType!.GenericTypeArguments[0], v => v);

        public static Type GetTaskForConfiguration<TConfiguration>(TConfiguration configuration) where TConfiguration : HuskyTaskConfiguration
            => TasksByConfigurationType[configuration.GetType()];
    }
}