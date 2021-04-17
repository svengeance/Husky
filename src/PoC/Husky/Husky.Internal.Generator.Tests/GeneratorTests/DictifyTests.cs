using System;
using Husky.Internal.Generator.Dictify;
using NUnit.Framework;

namespace Husky.Internal.Generator.Tests.GeneratorTests
{
    internal class DictifyTests: BaseGeneratorTest<DictifyGenerator>
    {
        [TestCase(
@"namespace Foo
{
    [global::Husky.Internal.Generator.Dictify.Dictify]
    partial class KittenFactory
    {
        public int NumberOfKittens { get; set; }
        public bool ParallelizeKittenEmissions { get; set; }
        public object KittenHats { get; set; }
        public string KittenNamePrefix { get; set; }
        public decimal RandomizedAgeSeed { get; set; }
    }
}",
@"namespace Foo
{
    partial class KittenFactory: global::Husky.Internal.Generator.Dictify.IDictable
    {
        public global::System.Collections.Generic.Dictionary<string, object> ToDictionary() => new()
        {
            [""KittenFactory.NumberOfKittens""] = NumberOfKittens,
            [""KittenFactory.ParallelizeKittenEmissions""] = ParallelizeKittenEmissions,
            [""KittenFactory.KittenHats""] = KittenHats,
            [""KittenFactory.KittenNamePrefix""] = KittenNamePrefix,
            [""KittenFactory.RandomizedAgeSeed""] = RandomizedAgeSeed,
        };
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static class Dictable
    {
        public static global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::System.Collections.Generic.IReadOnlyDictionary<string, object>, object>> GetDictableFactories()
        {
            return new()
            {
                [typeof(global::Foo.KittenFactory)] = dict => new global::Foo.KittenFactory
                {
                    NumberOfKittens = (int) dict[""KittenFactory.NumberOfKittens""],
                    ParallelizeKittenEmissions = (bool) dict[""KittenFactory.ParallelizeKittenEmissions""],
                    KittenHats = (object) dict[""KittenFactory.KittenHats""],
                    KittenNamePrefix = (string) dict[""KittenFactory.KittenNamePrefix""],
                    RandomizedAgeSeed = (decimal) dict[""KittenFactory.RandomizedAgeSeed""],
                },
            };
        }
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static partial class ObjectFactory
    {
        static ObjectFactory()
        {
            LoadKnownTypes();
        }
        
        private static System.Collections.Generic.Dictionary<System.Type, System.Func<System.Collections.Generic.IReadOnlyDictionary<string, object>, object>> _factories = new();

        public static void AddFactory(System.Type t, System.Func<System.Collections.Generic.IReadOnlyDictionary<string, object>, object> createFn)
            => _factories[t] = createFn;

        static partial void LoadKnownTypes();

        public static object Create(System.Type t, System.Collections.Generic.IReadOnlyDictionary<string, object> dict)
            => _factories[t](dict);
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static partial class ObjectFactory
    {
        static partial void LoadKnownTypes()
        {
            foreach (var typeFactory in global::Husky.Internal.Generator.Dictify.Dictable.GetDictableFactories())
            {
                AddFactory(typeFactory.Key, typeFactory.Value);
            }
        }
    }
}
")]
        [Category("UnitTest")]
        public void Dictify_generator_dictifies_base_class_with_no_recursion(string source, string generated)
        {
            // Arrange
            // Act
            string output = GenerateSource(source);

            // Assert
            Console.WriteLine(output);
            Assert.AreEqual(generated, output);
        }

        [TestCase(
@"namespace Factories
{
    [global::Husky.Internal.Generator.Dictify.Dictify(true)]
    public record FactoryBase
    {
        public int DoNotGenerateMe { get; set; }
    }
}
namespace Kittens
{
    public partial record KittenFactory: Factories.FactoryBase
    {
        public int NumberOfKittens { get; set; }
    }
}
namespace Puppies
{
    public partial record PuppyFactory: Factories.FactoryBase
    {
        public int NumberOfPuppies{ get; set; }
    }
}",
@"namespace Kittens
{
    partial record KittenFactory: global::Husky.Internal.Generator.Dictify.IDictable
    {
        public global::System.Collections.Generic.Dictionary<string, object> ToDictionary() => new()
        {
            [""KittenFactory.NumberOfKittens""] = NumberOfKittens,
        };
    }
}
namespace Puppies
{
    partial record PuppyFactory: global::Husky.Internal.Generator.Dictify.IDictable
    {
        public global::System.Collections.Generic.Dictionary<string, object> ToDictionary() => new()
        {
            [""PuppyFactory.NumberOfPuppies""] = NumberOfPuppies,
        };
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static class Dictable
    {
        public static global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::System.Collections.Generic.IReadOnlyDictionary<string, object>, object>> GetDictableFactories()
        {
            return new()
            {
                [typeof(global::Kittens.KittenFactory)] = dict => new global::Kittens.KittenFactory
                {
                    NumberOfKittens = (int) dict[""KittenFactory.NumberOfKittens""],
                },
                [typeof(global::Puppies.PuppyFactory)] = dict => new global::Puppies.PuppyFactory
                {
                    NumberOfPuppies = (int) dict[""PuppyFactory.NumberOfPuppies""],
                },
            };
        }
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static partial class ObjectFactory
    {
        static ObjectFactory()
        {
            LoadKnownTypes();
        }
        
        private static System.Collections.Generic.Dictionary<System.Type, System.Func<System.Collections.Generic.IReadOnlyDictionary<string, object>, object>> _factories = new();

        public static void AddFactory(System.Type t, System.Func<System.Collections.Generic.IReadOnlyDictionary<string, object>, object> createFn)
            => _factories[t] = createFn;

        static partial void LoadKnownTypes();

        public static object Create(System.Type t, System.Collections.Generic.IReadOnlyDictionary<string, object> dict)
            => _factories[t](dict);
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static partial class ObjectFactory
    {
        static partial void LoadKnownTypes()
        {
            foreach (var typeFactory in global::Husky.Internal.Generator.Dictify.Dictable.GetDictableFactories())
            {
                AddFactory(typeFactory.Key, typeFactory.Value);
            }
        }
    }
}
")]
        [Category("UnitTest")]
        public void Dictify_generator_dictifies_derived_classes_with_recursion(string source, string generated)
        {
            // Arrange
            // Act
            string output = GenerateSource(source);

            // Assert
            Console.WriteLine(output);
            Assert.AreEqual(generated, output);
        }

        [TestCase(
@"namespace Foo
{
    [global::Husky.Internal.Generator.Dictify.Dictify(portionToRemove: ""Factory"")]
    partial class KittenFactory
    {
        public int NumberOfKittens { get; set; }
    }
}",
@"namespace Foo
{
    partial class KittenFactory: global::Husky.Internal.Generator.Dictify.IDictable
    {
        public global::System.Collections.Generic.Dictionary<string, object> ToDictionary() => new()
        {
            [""Kitten.NumberOfKittens""] = NumberOfKittens,
        };
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static class Dictable
    {
        public static global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::System.Collections.Generic.IReadOnlyDictionary<string, object>, object>> GetDictableFactories()
        {
            return new()
            {
                [typeof(global::Foo.KittenFactory)] = dict => new global::Foo.KittenFactory
                {
                    NumberOfKittens = (int) dict[""Kitten.NumberOfKittens""],
                },
            };
        }
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static partial class ObjectFactory
    {
        static ObjectFactory()
        {
            LoadKnownTypes();
        }
        
        private static System.Collections.Generic.Dictionary<System.Type, System.Func<System.Collections.Generic.IReadOnlyDictionary<string, object>, object>> _factories = new();

        public static void AddFactory(System.Type t, System.Func<System.Collections.Generic.IReadOnlyDictionary<string, object>, object> createFn)
            => _factories[t] = createFn;

        static partial void LoadKnownTypes();

        public static object Create(System.Type t, System.Collections.Generic.IReadOnlyDictionary<string, object> dict)
            => _factories[t](dict);
    }
}
namespace Husky.Internal.Generator.Dictify
{
    public static partial class ObjectFactory
    {
        static partial void LoadKnownTypes()
        {
            foreach (var typeFactory in global::Husky.Internal.Generator.Dictify.Dictable.GetDictableFactories())
            {
                AddFactory(typeFactory.Key, typeFactory.Value);
            }
        }
    }
}
")]
        [Category("UnitTest")]
        public void Dictify_generator_removes_portion_of_class_name_from_dictionary_entries(string source, string generated)
        {
            // Arrange
            // Act
            string output = GenerateSource(source);

            // Assert
            Console.WriteLine(output);
            Assert.AreEqual(generated, output);
        }
    }
}