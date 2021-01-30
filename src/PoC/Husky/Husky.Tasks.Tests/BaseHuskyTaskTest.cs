using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentValidation;
using Husky.Core;
using Husky.Core.Platform;
using Husky.Core.Workflow;
using Moq;
using NUnit.Framework;

namespace Husky.Tasks.Tests
{
    [TestFixture]
    public abstract class BaseHuskyTaskTest<T> where T: class
    {
        protected T Sut { get; private set; } = default!;

        protected Mock<IPlatformInformation> PlatformInformationMock { get; } = new();
            
        private HuskyTaskConfiguration DefaultTaskConfiguration { get; set; } = null!;
        
        private HashSet<Type> HuskyTaskTypes { get; } = HuskyTaskResolver.GetAvailableTasks().ToHashSet();
        
        [SetUp]
        public void SetupBaseHuskyTaskTest()
        {
            DefaultTaskConfiguration = CreateDefaultTaskConfiguration();
            Sut = CreateAndConfigureTask(DefaultTaskConfiguration);
            CurrentPlatform.LoadPlatformInformation(PlatformInformationMock.Object);
        }
        
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

        protected abstract void BeforeSetup();
        
        protected abstract T CreateInstanceOfType();
        
        private T CreateAndConfigureTask(HuskyTaskConfiguration taskConfiguration)
        {
            if (!HuskyTaskTypes.Contains(typeof(T)))
                Assert.Fail($"Unable to locate Type {typeof(T)} for testing");

            BeforeSetup(); // Allows our unit tests to properly setup Mocks 
            var task = CreateInstanceOfType();
            var installationContext = CreateDefaultInstallationContext();
            var defaultExecutionInformation = new ExecutionInformation();
            Unsafe.As<HuskyTask<HuskyTaskConfiguration>>(task)!.SetExecutionContext(taskConfiguration, installationContext, defaultExecutionInformation);
            ValidateConfiguration();
            return task;
        }

        private void ValidateConfiguration()
        {
            var validation = DefaultTaskConfiguration.Validate();
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);
        }
    }
}