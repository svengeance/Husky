using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Husky.Core.HuskyConfiguration;
using Husky.Internal.Generator.Dictify;
using NUnit.Framework;

namespace Husky.Services.Tests.VariableResolverTests
{
    public class VariableResolverServiceTests: BaseUnitTest<VariableResolverService>
    {
        private Dictionary<string, object> _fullVariableSource = new();

        [SetUp]
        public void SetupTest()
            => _fullVariableSource = new Dictionary<string, object>(
                Assembly.GetExecutingAssembly()
                        .GetReferencedAssemblies()
                        .Select(Assembly.Load)
                        .SelectMany(s => s.ExportedTypes)
                        .Where(w => w.GetInterface(nameof(IDictable)) is not null ||
                                    w.BaseType?.GetInterface(nameof(IDictable)) is not null)
                        .Select(s => (IDictable) Activator.CreateInstance(s)!)
                        .SelectMany(s => s.ToDictionary()));
        [Test]
        [Category("UnitTest")]
        public void Resolver_updates_variable_sources_when_variables_are_found()
        {
            // Arrange
            _fullVariableSource.Clear();

            AddVariableSource(("Cat", "Kitten"), ("GhostKitten", "Ghost{Cat}"), ("Dog", "Puppy"));
            var expectedValue = "GhostKitten";

            // Act
            Sut.ResolveVariables(_fullVariableSource);

            // Assert
            Assert.AreEqual(expectedValue, _fullVariableSource["GhostKitten"]);
        }

        [Test]
        [Category("UnitTest")]
        public void Resolver_can_resolve_variables_within_other_variables()
        {
            // Arrange
            _fullVariableSource.Clear();
            
            AddVariableSource(("WhoAttacked", "The Fire Nation"));
            AddVariableSource(("ZukoIs", "{WhoAttacked}"));
            AddVariableSource(("Avatar", "Aang"));
            AddVariableSource(("NationHuntingAvatar", "{ZukoIs}"));
            AddVariableSource(("Sentence", "{NationHuntingAvatar} is hunting {Avatar}"));

            var expectedValue = "The Fire Nation is hunting Aang";

            // Act
            Sut.ResolveVariables(_fullVariableSource);

            // Assert
            Assert.AreEqual(expectedValue, _fullVariableSource["Sentence"]);
        }

        [Test]
        [Category("UnitTest")]
        public void Resolver_does_not_throw_when_requested_variable_is_not_found()
        {
            // Arrange
            AddVariableSource(($"MischievousVariable", $"{{{Guid.NewGuid()}}}"));
            var dictable = typeof(AuthorConfiguration);

            // Act
            void Resolve() => _ = Sut.Resolve(dictable, _fullVariableSource);

            // Assert
            Assert.DoesNotThrow(Resolve);
        }

        [Test]
        [Category("UnitTest")]
        public void Resolver_resolves_requested_item_with_defaults_with_no_additional_sources()
        {
            // Arrange
            var dictable = typeof(AuthorConfiguration);
            var expectedDictable = new AuthorConfiguration();

            // Act
            var result = Sut.Resolve(dictable, _fullVariableSource);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(expectedDictable, result);
        }

        [Test]
        [Category("UnitTest")]
        public void Resolver_resolves_requested_item_with_updated_variables()
        {
            // Arrange
            var authorConfiguration = new AuthorConfiguration
            {
                Publisher = "Cat!"
            };

            var authorPublisherKey = ((IDictable) authorConfiguration).ToDictionary()
                                                                      .First(f => (f.Value as string) == "Cat!")
                                                                      .Key;

            var expectedValue = "Kittens!";
            _fullVariableSource[authorPublisherKey] = expectedValue;

            // Act
            var newAuthorConfiguration = (AuthorConfiguration) Sut.Resolve(typeof(AuthorConfiguration), _fullVariableSource);

            // Assert
            Assert.AreEqual(expectedValue, newAuthorConfiguration.Publisher);
        }

        private void AddVariableSource(params (string key, string value)[] input)
        {
            foreach (var (k, v) in input)
                _fullVariableSource[k] = v;
        }
    }
}