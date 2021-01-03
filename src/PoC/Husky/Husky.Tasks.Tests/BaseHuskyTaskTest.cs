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
    [TestFixture]
    public abstract class BaseHuskyTaskTest<T>
    {
        protected T Sut { get; private set; }
        
        private HuskyTaskConfiguration DefaultTaskConfiguration { get; set; }
        
        private IServiceProvider ScopedServiceProvider { get; set; }
        private IServiceScope CurrentTestScope { get; set; }
        private IServiceProvider RootServiceProvider { get; set; }
        private HashSet<Type> HuskyTaskTypes { get; } = HuskyTaskResolver.GetAvailableTasks().ToHashSet();

        [OneTimeSetUp]
        protected void BaseSetup()
        {
            var installerConfiguration = new InstallationConfiguration();
            var huskyConfiguration = HuskyConfiguration.Create();
            ConfigureHusky(huskyConfiguration);
            RootServiceProvider = new ServiceCollection().AddHuskyInstaller(installerConfiguration, huskyConfiguration);
        }

        [SetUp]
        public void Setup()
        {
            var scopeFactory = RootServiceProvider.GetRequiredService<IServiceScopeFactory>();
            CurrentTestScope = scopeFactory.CreateScope();
            ScopedServiceProvider = CurrentTestScope.ServiceProvider;
            DefaultTaskConfiguration = CreateDefaultTaskConfiguration();
            SetTask(DefaultTaskConfiguration);
        }

        [TearDown]
        public void TearDown() => CurrentTestScope.Dispose();

        protected void UpdateTaskConfiguration<TOptions>(Action<TOptions> update) where TOptions: HuskyTaskConfiguration
        {
            var updateParameterType = update.GetMethodInfo().GetParameters().First().ParameterType;
            if (updateParameterType != DefaultTaskConfiguration.GetType())
                throw new ArgumentException($"Attempted to update TaskConfiguration {DefaultTaskConfiguration.GetType()} with a different type ({updateParameterType})");

            update((TOptions) DefaultTaskConfiguration);
            ValidateConfiguration();
        }

        protected abstract void ConfigureHusky(HuskyConfiguration huskyConfiguration);

        protected virtual InstallationContext CreateDefaultInstallationContext() => new(Assembly.GetExecutingAssembly());

        protected abstract HuskyTaskConfiguration CreateDefaultTaskConfiguration();

        private void SetTask(HuskyTaskConfiguration taskConfiguration)
        {
            if (!HuskyTaskTypes.Contains(typeof(T)))
                Assert.Fail($"Unable to locate Type {typeof(T)} for testing");

            var task = (T)ScopedServiceProvider.GetRequiredService(typeof(T));
            var installationContext = CreateDefaultInstallationContext();
            var defaultExecutionInformation = new ExecutionInformation();
            Unsafe.As<HuskyTask<HuskyTaskConfiguration>>(task).SetExecutionContext(taskConfiguration, installationContext, defaultExecutionInformation);
            ValidateConfiguration();
            Sut = task;
        }

        private void ValidateConfiguration()
        {
            var validation = DefaultTaskConfiguration.Validate();
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);
        }
    }
}