using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentValidation;
using Husky.Core.Platform;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Tasks.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
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
        
        [SetUp]
        public async ValueTask SetupBaseHuskyTaskTest()
        {
            DefaultTaskConfiguration = CreateDefaultTaskConfiguration();
            Sut = await CreateAndConfigureTask(DefaultTaskConfiguration);
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

        protected abstract ValueTask<IUninstallOperationsList> CreateUninstallOperationsList();

        protected abstract HuskyTaskConfiguration CreateDefaultTaskConfiguration();

        protected abstract void BeforeSetup();

        protected abstract T CreateInstanceOfType();

        private async ValueTask<HuskyContext> CreateDefaultHuskyContext()
            => await ValueTask.FromResult<HuskyContext>(new(await CreateUninstallOperationsList(), Assembly.GetExecutingAssembly(), "Test"));
        
        private async ValueTask<T> CreateAndConfigureTask(HuskyTaskConfiguration taskConfiguration)
        {

            BeforeSetup(); // Allows our unit tests to properly setup Mocks 
            var huskyTask = CreateInstanceOfType();
            var installationContext = await CreateDefaultHuskyContext();
            var defaultExecutionInformation = new ExecutionInformation();
            Unsafe.As<HuskyTask<HuskyTaskConfiguration>>(huskyTask)!.SetExecutionContext(taskConfiguration, installationContext, defaultExecutionInformation);
            ValidateConfiguration();
            return huskyTask;
        }

        private void ValidateConfiguration()
        {
            var validation = DefaultTaskConfiguration.Validate();
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);
        }
    }
}