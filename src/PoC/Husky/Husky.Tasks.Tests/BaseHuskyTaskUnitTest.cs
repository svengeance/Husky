﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Moq;
using BindingFlags = System.Reflection.BindingFlags;

namespace Husky.Tasks.Tests
{
    public abstract class BaseHuskyTaskUnitTest<T>: BaseHuskyTaskTest<T> where T: class
    {
        protected IFixture Fixture = null!;
        protected Mock<IUninstallOperationsList> UninstallOperationsMock { get; } = new();

        protected override void BeforeSetup()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            Fixture.Inject(new SemanticVersioning.Version("0.1.2"));
            Fixture.Inject(UninstallOperationsMock);

            var sutConstructorTypes = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                                               .Single()
                                               .GetParameters()
                                               .Select(s => s.ParameterType)
                                               .ToArray();

            foreach (var type in sutConstructorTypes)
            {
                var mock = GetMockForType(type);
                InjectFromTypeAndInstance(Fixture, mock.GetType(), mock);
            }

            var huskyConfiguration = HuskyConfiguration.Create();
            ConfigureHusky(huskyConfiguration);
            
            foreach (var configuration in huskyConfiguration.GetConfigurationBlocks())
                InjectFromTypeAndInstance(Fixture, configuration.GetType(), configuration);
        }

        protected override ValueTask<IUninstallOperationsList> CreateUninstallOperationsList()
            => new (UninstallOperationsMock.Object);

        protected override T CreateInstanceOfType() => Fixture.Create<T>();
        
        private static object GetMockForType(Type type) => Activator.CreateInstance(typeof(Mock<>).MakeGenericType(type))!;

        private static void InjectFromTypeAndInstance(IFixture fixture, Type type, object instance)
            => typeof(FixtureRegistrar).GetMethod(nameof(FixtureRegistrar.Inject))!.MakeGenericMethod(type).Invoke(null, new[] { fixture, instance });
    }
}