using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Husky.Services.Tests.VariableResolverTests
{
    public class VariableResolverServiceTests: BaseUnitTest<VariableResolverService>
    {
        [Test]
        [Category("UnitTest")]
        public void Resolver_does_not_change_null_values()
        {
            // Arrange
            var emptySource = CreateSource();
            var testObject = new Cat(null, null, 1);

            // Act
            Sut.Resolve(testObject, emptySource);

            // Assert
            Assert.IsNull(testObject.Name);
            Assert.IsNull(testObject.NickName);
            Assert.AreEqual(1, testObject.Age);
        }

        [Test]
        [Category("UnitTest")]
        public void Resolver_changes_single_value_of_known_string()
        {
            // Arrange
            var expectedName = "4";
            var varSource = CreateSource(("randomName", expectedName));
            var testObject = new Cat("{randomName}", null, 1);

            // Act
            Sut.Resolve(testObject, varSource);

            // Assert
            Assert.AreEqual(expectedName, testObject.Name);
        }

        [Test]
        [Category("UnitTest")]
        public void Resolver_changes_multiple_properties_from_multiple_sources()
        {
            // Arrange
            var expectedName = "Ser Archibald";
            var expectedNickName = "Archie";

            var firstSource = CreateSource(("cat.Name", expectedName));
            var secondSource = CreateSource(("cat.NickName", expectedNickName));

            var testObject = new Cat("{cat.Name}", "{cat.NickName}", 1);

            // Act
            Sut.Resolve(testObject, firstSource, secondSource);

            // Assert
            Assert.AreEqual(expectedName, testObject.Name);
            Assert.AreEqual(expectedNickName, testObject.NickName);
        }

        [Test]
        [Category("UnitTest")]
        public void Resolver_changes_multiple_variables_in_one_property()
        {
            // Arrange
            var expectedNamePrefix = "Ser";
            var expectedName = "Archibald";
            var expectedNameLine1 = "First of his Name";
            var expectedNameLine2 = "Protector of the Realm";
            var expectedFinalName = "Ser Archibald, First of his Name; Protector of the Realm";
            
            var prefixesSource = CreateSource(("prefixes.Ser", expectedNamePrefix));
            var namesSource = CreateSource(("catNames.Name", expectedName));
            var titlesSource1 = CreateSource(("titles.FirstOfName", expectedNameLine1));
            var titlesSource2 = CreateSource(("titles.Protector", expectedNameLine2));

            var testObject = new Cat("{prefixes.Ser} {catNames.Name}, {titles.FirstOfName}; {titles.Protector}", string.Empty, 5);

            // Act
            Sut.Resolve(testObject, prefixesSource, namesSource, titlesSource1, titlesSource2);

            // Assert
            Assert.AreEqual(expectedFinalName, testObject.Name);
            Assert.AreEqual(string.Empty, testObject.NickName);
            Assert.AreEqual(5, testObject.Age);
        }

        [Test]
        [Category("UnitTest")]
        public void Resolver_throws_detailed_exception_when_unable_to_locate_variables()
        {
            // Arrange
            var varSource = CreateSource(("she-loves-me", string.Empty));
            var testObject = new Cat("{she.loves.me.not}", null, 1);

            // Act
            void Resolve() => Sut.Resolve(testObject, varSource);

            // Assert
            var exception = Assert.Catch<ArgumentException>(Resolve);
            Assert.True(exception.Message.Contains("she.loves.me.not"));
            Assert.True(exception.Message.Contains(nameof(Cat)));
            Assert.True(exception.Message.Contains(nameof(Cat.Name)));
        }
        
        private static Dictionary<string, string> CreateSource(params (string key, string value)[] input)
            => input.ToDictionary(k => k.key, v => v.value);

        private class Cat
        {
            public string Name { get; set; }
            public string NickName { get; set; }
            public int Age { get; set; }
            
            public Cat(string name, string nickName, int age)
            {
                Name = name;
                NickName = nickName;
                Age = age;
            }
        }
    }
}