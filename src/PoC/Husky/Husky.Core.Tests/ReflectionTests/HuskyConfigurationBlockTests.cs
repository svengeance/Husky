using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow;
using NUnit.Framework;

namespace Husky.Core.Tests.ReflectionTests
{
    public class HuskyConfigurationBlockTests
    {
        [Test]
        [Category("ReflectionTest")]
        public void All_configuration_blocks_must_implement_a_private_validator() // Maybe a bit heavyhanded but ensures standard validation reporting everywhere
        {
            foreach (var option in GetHuskyConfigurationBlocks())
            {
                var validatorType = typeof(AbstractValidator<>).MakeGenericType(option);
                var privateValidator = option.GetNestedTypes(BindingFlags.NonPublic)
                                             .FirstOrDefault(f => f.BaseType == validatorType);

                Assert.NotNull(privateValidator, $"{option} does not implement a Validator; please add a private validator class that extends from AbstractValidator<{option.Name}>");
            }
        }

        private static IEnumerable<Type> GetHuskyConfigurationBlocks()
            => typeof(HuskyTaskConfiguration).Assembly
                                             .GetExportedTypes()
                                             .Where(w => !w.IsAbstract && w.IsClass)
                                             .Where(w => w.BaseType == typeof(HuskyConfigurationBlock));
    }
}