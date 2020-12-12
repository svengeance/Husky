using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Husky.Services.Extensions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Husky.Services.Tests.ReflectionTests
{
    [TestFixture]
    public class ServiceInterfacesTests
    {
        private Type[] _services;

        [OneTimeSetUp]
        public void GetServicesInformation()
        {
            _services = typeof(ServiceCollectionExtensions).Assembly
                                                           .GetExportedTypes()
                                                           .Where(w => w.IsPublic && w.Name.EndsWith("Service"))
                                                           .ToArray();
        }

        [Test]
        [Category("ReflectionTest")]
        public void All_services_must_have_a_single_interface()
        {
            foreach (var serviceClass in GetServiceClasses())
                Assert.AreEqual(1, serviceClass.GetInterfaces().Length, $"{serviceClass.Name} should have a single interface!");
        }

        [Test]
        [Category("ReflectionTest")]
        public void All_service_interfaces_must_follow_naming_convention()
        {
            foreach (var serviceClass in GetServiceClasses())
                Assert.NotNull(serviceClass.GetInterface($"I{serviceClass.Name}"), $"{serviceClass.Name} does not have a valid interface! Should be $I{serviceClass.Name}");
        }


        [Test]
        [Category("ReflectionTest")]
        public void All_services_must_expose_public_methods_via_interface()
        {
            var methodCountsByTypeName = GetServiceClasses().Select(s => new
            {
                s.Name,
                NumberOfInterfaceMethods = s.GetInterfaces().Single().GetMethods(BindingFlags.Instance).Length,
                NumberOfPublicClassMethods = s.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length
            });

            foreach (var classMethodInfo in methodCountsByTypeName)
                Assert.AreEqual(classMethodInfo.NumberOfInterfaceMethods, classMethodInfo.NumberOfPublicClassMethods, $"{classMethodInfo.Name} should not expose public methods not in an interface!");
        }

        private IEnumerable<Type> GetServiceClasses()
        {
            return _services.Where(w => w.IsClass);
        }
    }
}