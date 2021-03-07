using Microsoft.Extensions.DependencyInjection;

namespace Husky.Tasks.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHuskyTasks(this IServiceCollection services)
        {
            foreach (var huskyTask in HuskyTaskResolver.GetAvailableTasks())
                services.AddTransient(huskyTask);
        }
    }
}