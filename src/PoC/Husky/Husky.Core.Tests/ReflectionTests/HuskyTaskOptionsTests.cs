using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Husky.Core.Workflow;
using NUnit.Framework;

namespace Husky.Core.Tests.ReflectionTests
{
    public class HuskyTaskOptionsTests
    {
        [Test]
        [Category("ReflectionTest")]
        public void All_tasks_must_implement_a_private_validator()
        {
            foreach (var option in GetHuskyTaskOptions())
            {
                var validatorType = typeof(AbstractValidator<>).MakeGenericType(option);
                var privateValidator = option.GetNestedTypes(BindingFlags.NonPublic)
                                             .FirstOrDefault(f => f.BaseType == validatorType);

                Assert.NotNull(privateValidator, $"{option} does not implement a Validator; please add a private validator class that extends from AbstractValidator<{option.Name}>");
            }
        }

        private static IEnumerable<Type> GetHuskyTaskOptions()
            => typeof(HuskyTaskConfiguration).Assembly
                                             .GetExportedTypes()
                                             .Where(w => !w.IsAbstract && w.IsClass)
                                             .Where(w => w.BaseType == typeof(HuskyTaskConfiguration));
    }
}