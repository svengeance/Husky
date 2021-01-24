using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Husky.Dependencies.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHuskyDependencies(this IServiceCollection services)
        {
            var huskyDependencies = Assembly.GetExecutingAssembly()
                                            .GetExportedTypes()
                                            .Where(w => w.BaseType?.IsGenericType == true &&
                                                        w.BaseType.GetGenericTypeDefinition() == typeof(DependencyHandler<>))
                                            .Where(w => w.IsClass && w.IsPublic && !(w.IsSealed && w.IsAbstract))
                                            .Select(s => (s.BaseType!.GetInterfaces().First(), s));

            foreach (var (serviceInterface, serviceClass) in huskyDependencies)
                services.TryAddScoped(serviceInterface, serviceClass);
        }
    }
}