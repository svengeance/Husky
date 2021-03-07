using System;
using System.IO;
using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Installer;
using Husky.Installer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

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
            var installerConfiguration = new HuskyInstallerSettings();
            var huskyConfiguration = HuskyConfiguration.Create();
            ConfigureHusky(huskyConfiguration);
            RootServiceProvider = new ServiceCollection().AddHuskyInstaller(installerConfiguration, huskyConfiguration);
        }

        protected override T CreateInstanceOfType() => ScopedServiceProvider.GetRequiredService<T>();

        protected override async ValueTask<IUninstallOperationsList> CreateUninstallOperationsList()
            => (UninstallOperationsList = await new ValueTask<IUninstallOperationsList>(await Core.Workflow.Uninstallation.UninstallOperationsList.CreateOrRead(_uninstallOperationsListFile)));

        protected override void BeforeSetup()
        {
            var scopeFactory = RootServiceProvider.GetRequiredService<IServiceScopeFactory>();
            CurrentTestScope = scopeFactory.CreateScope();
            ScopedServiceProvider = CurrentTestScope.ServiceProvider;
        }

        [TearDown]
        public void TearDown()
        {
            CurrentTestScope.Dispose();
            File.Delete(_uninstallOperationsListFile);
        }
    }
}