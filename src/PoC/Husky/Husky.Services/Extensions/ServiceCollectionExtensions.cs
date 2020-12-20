using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHuskyServices(this IServiceCollection services)
        {
            var huskyServices = Assembly.GetExecutingAssembly()
                                        .GetExportedTypes()
                                        .Where(w => w.IsClass && w.IsPublic && !(w.IsSealed && w.IsAbstract))
                                        .Select(s => (s.GetInterface($"I{s.Name}"), s));

            foreach (var (serviceInterface, serviceClass) in huskyServices)
            {
                if (serviceInterface is not null)
                    services.AddScoped(serviceInterface, serviceClass);
                else
                    throw new ApplicationException($"Could not locate service interface for class {serviceClass.Name}");
            }
        }
    }
}