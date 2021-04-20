using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Husky.Internal.Generator.Dictify
{
    [Generator]
    internal partial class DictifyGenerator: ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(cb => cb.AddSource("DictifyClasses", DictifyWriter.SupportingClasses));

            context.RegisterForSyntaxNotifications(() => new DictifySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not DictifySyntaxReceiver dictifyReceiver)
                return;

            var dictifyWriter = new DictifyWriter();
            foreach (var classToDict in dictifyReceiver.TypeSymbolsToDict)
            {
                var symbolName = classToDict.TypeSymbol.Name;
                var dictClassName = string.IsNullOrWhiteSpace(classToDict.PortionToRemove)
                    ? symbolName
                    : symbolName.Replace(classToDict.PortionToRemove!, string.Empty);
                var namespaceName = classToDict.TypeSymbol.ContainingNamespace.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
                var propertiesToDict = GetPublicPropertiesFromSymbol(classToDict.TypeSymbol);
                dictifyWriter.AppendDictableItem(symbolName, dictClassName, namespaceName, classToDict.TypeSymbol.IsRecord, propertiesToDict);
            }

            context.AddSource("DictedClasses.cs", dictifyWriter.ToString());
        }

#pragma warning disable RS1024 // Compare symbols correctly
        private Dictionary<string, ITypeSymbol> GetPublicPropertiesFromSymbol(INamedTypeSymbol classDeclarationSyntax)
            => classDeclarationSyntax.GetMembers()
                                     .OfType<IPropertySymbol>()
                                     .Where(w => !w.IsImplicitlyDeclared)
                                     .ToDictionary(k => k.Name, v => v.Type);
#pragma warning restore RS1024 // Compare symbols correctly

        public class DictableClass
        {
            public INamedTypeSymbol TypeSymbol { get; set; }
            public string? PortionToRemove { get; set; }

            public DictableClass(INamedTypeSymbol typeSymbol, string? portionToRemove = null)
            {
                TypeSymbol = typeSymbol;
                PortionToRemove = portionToRemove;
            }
        }
    }
}