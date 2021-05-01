using System;
using Husky.Core.Workflow;
using StrongInject;

namespace Husky.Tasks.Infrastructure
{
    public interface IHuskyTaskResolver
    {
        Owned<HuskyTask<HuskyTaskConfiguration>> ResolveTaskForConfiguration(Type configurationType);
    }

    public class HuskyTaskResolver: IHuskyTaskResolver
    {
        public Owned<HuskyTask<HuskyTaskConfiguration>> ResolveTaskForConfiguration(Type configurationType)
        {
            using var container = new HuskyTasksContainer();
            return container.ResolveTaskForConfiguration(configurationType);
        }
    }

    //public static class HuskyTaskResolver
    //{
    //    private static readonly Dictionary<Type, Type> TasksByConfigurationType;

    //    static HuskyTaskResolver() => TasksByConfigurationType = ResolveHuskyTasks(Assembly.GetExecutingAssembly());

    //    internal static IEnumerable<Type> GetAvailableTasks() => TasksByConfigurationType.Values;

    //    internal static void AddAssemblyForScanning(Assembly assembly)
    //    {
    //        foreach (var (key, val) in ResolveHuskyTasks(assembly))
    //            TasksByConfigurationType[key] = val;
    //    }

    //    private static Dictionary<Type, Type> ResolveHuskyTasks(Assembly assembly)
    //        => assembly.GetExportedTypes()
    //                   .Where(w => w.BaseType?.GetGenericArguments().FirstOrDefault()?.BaseType == typeof(HuskyTaskConfiguration))
    //                   .ToDictionary(k => k.BaseType!.GenericTypeArguments[0], v => v);

    //    public static Type GetTaskForConfiguration<TConfiguration>(TConfiguration configuration) where TConfiguration : HuskyTaskConfiguration
    //        => TasksByConfigurationType[configuration.GetType()];
    //}
}