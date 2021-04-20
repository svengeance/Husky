using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Husky.Internal.Generator.Dictify
{
    internal class DictifySyntaxReceiver: ISyntaxContextReceiver
    {
        public List<DictifyGenerator.DictableClass> TypeSymbolsToDict { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not BaseTypeDeclarationSyntax baseTypeDeclarationSyntax ||
                context.SemanticModel.GetDeclaredSymbol(baseTypeDeclarationSyntax) is not { } typeSymbol)
                return;

            if (GetDictifyAttribute(typeSymbol) is (true, false, var portionToRemove))
                TypeSymbolsToDict.Add(new DictifyGenerator.DictableClass(typeSymbol, portionToRemove));
            else if (typeSymbol.BaseType is { } baseSymbol && GetDictifyAttribute(baseSymbol) is (true, true, var basePortionToRemove))
                TypeSymbolsToDict.Add(new DictifyGenerator.DictableClass(typeSymbol, basePortionToRemove));
        }

        private (bool hasAttr, bool? isRecursive, string? portionToRemove) GetDictifyAttribute(INamedTypeSymbol symbol)
        {
            var dictifyAttribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "DictifyAttribute");
            if (dictifyAttribute is null)
                return (false, null, null);

            var isRecursive = dictifyAttribute.ConstructorArguments.Any(a => a.Value is true);
            var portionToRemove = dictifyAttribute.ConstructorArguments.FirstOrDefault(a => a.Value is string).Value?.ToString();
            return (true, isRecursive, portionToRemove);
        }
    }
}