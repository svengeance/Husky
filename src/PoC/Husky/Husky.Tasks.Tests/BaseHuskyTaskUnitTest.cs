using System;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Husky.Core.Workflow;
using Moq;
using BindingFlags = System.Reflection.BindingFlags;

namespace Husky.Tasks.Tests
{
    public abstract class BaseHuskyTaskUnitTest<T>: BaseHuskyTaskTest<T> where T: class
    {
        protected IFixture _fixture = null!;
        
        protected override void BeforeSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            _fixture.Inject(new SemVer.Version("0.1.2"));
            
            var sutConstructorTypes = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                                               .Single()
                                               .GetParameters()
                                               .Select(s => s.ParameterType)
                                               .ToArray();

            foreach (var type in sutConstructorTypes)
            {
                var mock = GetMockForType(type);
                InjectFromTypeAndInstance(_fixture, mock.GetType(), mock);
            }

            var huskyConfiguration = HuskyConfiguration.Create();
            ConfigureHusky(huskyConfiguration);
            
            foreach (var configuration in huskyConfiguration.GetConfigurationBlocks())
                InjectFromTypeAndInstance(_fixture, configuration.GetType(), configuration);
        }
        
        protected override T CreateInstanceOfType() => _fixture.Create<T>();
        
        private static object GetMockForType(Type type) => Activator.CreateInstance(typeof(Mock<>).MakeGenericType(type))!;

        private static void InjectFromTypeAndInstance(IFixture fixture, Type type, object instance)
            => typeof(FixtureRegistrar).GetMethod(nameof(FixtureRegistrar.Inject))!.MakeGenericMethod(type).Invoke(null, new[] { fixture, instance });
    }
}