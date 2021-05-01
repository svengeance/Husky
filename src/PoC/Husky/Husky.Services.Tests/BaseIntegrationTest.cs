using System;
using System.Linq;
using System.Reflection;
using Husky.Services.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using StrongInject;

namespace Husky.Services.Tests
{
    public class BaseIntegrationTest<T> where T: class
    {
        private IServiceProvider Services { get; set; } = null!;
        private IServiceScope ServiceScope { get; set; } = null!;
        
        protected T Sut { get; private set; }
        
        [SetUp]
        public void SetupServices()
        {
            var serviceCollection = new ServiceCollection();
            RegisterServicesFromModule(serviceCollection);

            Services = serviceCollection.BuildServiceProvider(validateScopes: true);

            var serviceScopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
            ServiceScope = serviceScopeFactory.CreateScope();

            var sutDescriptor = serviceCollection.First(f => f.ImplementationType == typeof(T));
            Sut = (T) ServiceScope.ServiceProvider.GetRequiredService(sutDescriptor.ServiceType);
        }

        private void RegisterServicesFromModule(ServiceCollection serviceCollection)
        {
            foreach (var registration in typeof(HuskyServicesModule).GetCustomAttributes<RegisterAttribute>())
                serviceCollection.AddScoped(registration.RegisterAs.Single(), registration.Type);
        }

        [TearDown]
        public void TearDownServices() => ServiceScope.Dispose();
    }
}