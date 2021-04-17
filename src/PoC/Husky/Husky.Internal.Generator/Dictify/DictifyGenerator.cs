﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Husky.Internal.Generator.Dictify
{
    [Generator]
    internal class DictifyGenerator: ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
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

        public class DictifySyntaxReceiver: ISyntaxContextReceiver
        {
            public List<DictableClass> TypeSymbolsToDict { get; } = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is not BaseTypeDeclarationSyntax baseTypeDeclarationSyntax ||
                    context.SemanticModel.GetDeclaredSymbol(baseTypeDeclarationSyntax) is not { } typeSymbol)
                    return;

                if (GetDictifyAttribute(typeSymbol) is (true, false, var portionToRemove))
                    TypeSymbolsToDict.Add(new DictableClass(typeSymbol, portionToRemove));
                else if (typeSymbol.BaseType is { } baseSymbol && GetDictifyAttribute(baseSymbol) is (true, true, var basePortionToRemove))
                    TypeSymbolsToDict.Add(new DictableClass(typeSymbol, basePortionToRemove));
            }

            private (bool hasAttr, bool? isRecursive, string? portionToRemove) GetDictifyAttribute(INamedTypeSymbol symbol)
            {
                var dictifyAttribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(DictifyAttribute));
                if (dictifyAttribute is null)
                    return (false, null, null);

                var isRecursive = dictifyAttribute.ConstructorArguments.Any(a => a.Value is true);
                var portionToRemove = dictifyAttribute.ConstructorArguments.FirstOrDefault(a => a.Value is string).Value?.ToString();
                return (true, isRecursive, portionToRemove);
            }
        }

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