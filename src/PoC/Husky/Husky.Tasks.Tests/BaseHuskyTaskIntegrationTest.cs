using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentValidation;
using Husky.Core;
using Husky.Core.Workflow;
using Husky.Installer;
using Husky.Installer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Husky.Tasks.Tests
{
    public abstract class BaseHuskyTaskIntegrationTest<T>: BaseHuskyTaskTest<T> where T: class
    {
        private IServiceProvider ScopedServiceProvider { get; set; } = null!;
        private IServiceScope CurrentTestScope { get; set; } = null!;
        private IServiceProvider RootServiceProvider { get; set; } = null!;

        [OneTimeSetUp]
        protected void BaseSetup()
        {
            var installerConfiguration = new InstallationConfiguration();
            var huskyConfiguration = HuskyConfiguration.Create();
            ConfigureHusky(huskyConfiguration);
            RootServiceProvider = new ServiceCollection().AddHuskyInstaller(installerConfiguration, huskyConfiguration);
        }

        protected override T CreateInstanceOfType() => ScopedServiceProvider.GetRequiredService<T>();

        protected override void BeforeSetup()
        {
            var scopeFactory = RootServiceProvider.GetRequiredService<IServiceScopeFactory>();
            CurrentTestScope = scopeFactory.CreateScope();
            ScopedServiceProvider = CurrentTestScope.ServiceProvider;
        }

        [TearDown]
        public void TearDown() => CurrentTestScope.Dispose();
    }
}