using System;
using System.Linq;
using Husky.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Husky.Services.Tests
{
    public class BaseIntegrationTest<T> where T: class
    {
        private IServiceProvider Services { get; set; }
        private IServiceScope ServiceScope { get; set; }
        
        protected T Sut { get; private set; }
        
        [SetUp]
        public void SetupServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHuskyServices();
            Services = serviceCollection.BuildServiceProvider(validateScopes: true);

            var serviceScopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
            ServiceScope = serviceScopeFactory.CreateScope();

            var sutDescriptor = serviceCollection.First(f => f.ImplementationType == typeof(T));
            Sut = (T) ServiceScope.ServiceProvider.GetRequiredService(sutDescriptor.ServiceType);
        }

        [TearDown]
        public void TearDownServices() => ServiceScope.Dispose();
    }
}