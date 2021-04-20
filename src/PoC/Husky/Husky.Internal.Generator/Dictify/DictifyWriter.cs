using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Husky.Internal.Generator.Dictify
{
    internal class DictifyWriter
    {
        public static readonly string SupportingClasses = @"namespace Husky.Internal.Generator.Dictify
{
    [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
    public class DictifyAttribute: global::System.Attribute
    {
        private readonly bool _applyToDerivedClasses;
        private readonly string _portionToRemove;

        public DictifyAttribute(bool applyToDerivedClasses = false, string portionToRemove = null)
        {
            _applyToDerivedClasses = applyToDerivedClasses;
            _portionToRemove = portionToRemove;
        }
    }

    public static partial class ObjectFactory
    {
        private static System.Collections.Generic.Dictionary<System.Type, System.Func<System.Collections.Generic.IReadOnlyDictionary<string, object>, object>> _factories = new();
        
static ObjectFactory()
        {
            LoadKnownTypes();
        }
       
        public static void AddFactory(System.Type t, System.Func<System.Collections.Generic.IReadOnlyDictionary<string, object>, object> createFn)
            => _factories[t] = createFn;

        static partial void LoadKnownTypes();

        public static object Create(System.Type t, System.Collections.Generic.IReadOnlyDictionary<string, object> dict)
            => _factories[t](dict);
    }

    public interface IDictable
    {
        public global::System.Collections.Generic.Dictionary<string, object> ToDictionary();
    }
}";

        private readonly SourceBuilder _sb = new();

        private readonly List<DictableItem> _dictableItems = new();

        public void AppendDictableItem(string className, string dictClassName, string namespaceName, bool isRecord, Dictionary<string, ITypeSymbol> propertiesByName)
        {
            using (_sb.Block($"namespace {namespaceName}"))
            using (_sb.Block($"partial {(isRecord ? "record" : "class")} {className}: global::Husky.Internal.Generator.Dictify.IDictable"))
                WriteToDictionary(dictClassName, propertiesByName);

            _dictableItems.Add(new DictableItem
            {
                NamespaceName = namespaceName,
                ClassName = className,
                DictClassName = dictClassName,
                PropertiesByName = propertiesByName
            });
        }

        private void WriteCreateFunctionStaticClass()
        {
            using (_sb.Block("namespace Husky.Internal.Generator.Dictify"))
            using (_sb.Block("public static partial class ObjectFactory"))
            using (_sb.Block("static partial void LoadKnownTypes()"))
                WriteCreateFunctionsForAllClasses();
        }

        private void WriteCreateFunctionsForAllClasses()
        {
            foreach (var item in _dictableItems)
            {
                using (_sb.Parens("AddFactory", ";"))
                {
                    _sb.Line($"typeof(global::{item.NamespaceName}.{item.ClassName}),");
                    _sb.Line($"dict => new global::{item.NamespaceName}.{item.ClassName}");
                    using (_sb.Block())
                        _sb.DelimitedLines(",", item.PropertiesByName.Select(s => $"{s.Key} = ({s.Value.ToDisplayString()}) dict[\"{item.DictClassName}.{s.Key}\"]"));
                }
            }
        }

        private void WriteToDictionary(string dictClassName, Dictionary<string, ITypeSymbol> propertiesByName)
        {
            _sb.Line($"public global::System.Collections.Generic.Dictionary<string, object> ToDictionary() => new()");
            using (_sb.Block(postFix: ";"))
                _sb.DelimitedLines(",", propertiesByName.Select(s => $"[\"{dictClassName}.{s.Key}\"] = {s.Key}"));
        }

        public override string ToString()
        {
            WriteCreateFunctionStaticClass();
            return _sb.ToString();
        }

        private class DictableItem
        {
            public string? NamespaceName { get; set; }
            public string? ClassName { get; set; }
            public string? DictClassName { get; set; }
            public Dictionary<string, ITypeSymbol> PropertiesByName { get; set; } = new();
        }
    }
}