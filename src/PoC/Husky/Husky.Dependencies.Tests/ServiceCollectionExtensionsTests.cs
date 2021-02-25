using System;
using Husky.Dependencies.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Husky.Dependencies.Test
{
    // Todo: Going to want to mirror this for other assemblies which have this pattern
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        [Category("IntegrationTest")]
        public void Dependency_handlers_successfully_register_themselves_into_service_collection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHuskyDependencies();

            CollectionAssert.IsNotEmpty(serviceCollection);
            
            foreach (var descriptor in serviceCollection)
                Console.WriteLine($"Registered {descriptor.ServiceType} as {descriptor.ImplementationType}");
        }
    }
}