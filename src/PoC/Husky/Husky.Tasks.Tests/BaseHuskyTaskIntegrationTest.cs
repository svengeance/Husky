using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Husky.Core.Infrastructure;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Installer;
using Husky.Services.Infrastructure;
using Husky.Tasks.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using StrongInject;

namespace Husky.Tasks.Tests
{
    public abstract class BaseHuskyTaskIntegrationTest<T>: BaseHuskyTaskTest<T> where T: class
    {
        protected IUninstallOperationsList UninstallOperationsList { get; private set; } = null!;

        private IServiceProvider ScopedServiceProvider { get; set; } = null!;
        private IServiceScope CurrentTestScope { get; set; } = null!;
        private IServiceProvider RootServiceProvider { get; set; } = null!;

        private readonly string _uninstallOperationsListFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        [OneTimeSetUp]
        protected void BaseSetup()
        {
            var huskyConfiguration = HuskyConfiguration.Create();
            ConfigureHusky(huskyConfiguration);
            RootServiceProvider = CreateServiceProvider();
        }

        protected override T CreateInstanceOfType() => ScopedServiceProvider.GetRequiredService<T>();

        protected override async ValueTask<IUninstallOperationsList> CreateUninstallOperationsList()
            => UninstallOperationsList =
                await new ValueTask<IUninstallOperationsList>(
                    await Core.Workflow.Uninstallation.UninstallOperationsList.CreateOrRead(_uninstallOperationsListFile));

        protected override void BeforeSetup()
        {
            var scopeFactory = RootServiceProvider.GetRequiredService<IServiceScopeFactory>();
            CurrentTestScope = scopeFactory.CreateScope();
            ScopedServiceProvider = CurrentTestScope.ServiceProvider;
        }

        private IServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            var modules = new[] { typeof(HuskyServicesModule), typeof(HuskyTasksContainer), typeof(HuskyCoreModule) };

            foreach (var module in modules)
                foreach (var registration in module.GetCustomAttributes<RegisterAttribute>())
                    serviceCollection.AddScoped(registration.RegisterAs.Length == 0 ? registration.Type : registration.RegisterAs.Single(), registration.Type);

            return serviceCollection.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            CurrentTestScope.Dispose();
            File.Delete(_uninstallOperationsListFile);
        }
    }
}