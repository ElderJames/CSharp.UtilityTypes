using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CSharp.UtilityTypes
{
    internal static class RoslynExtensions
    {
        public static bool Implements(this ClassDeclarationSyntax source, string interfaceName)
        {
            var attributes= source.AttributeLists.SelectMany(a => a.Attributes);

            //has to be this way now for classes so it can implement generic interfaces.
            if (source.BaseList is null)
            {
                return false;
            }
            IEnumerable<BaseTypeSyntax> baseTypes = source.BaseList.Types.Select(baseType => baseType);
            foreach (BaseTypeSyntax baseType in baseTypes)
            {
                if (baseType.Type is GenericNameSyntax gg && gg.Identifier.ValueText == interfaceName)
                {
                    return true;
                }
                else if (baseType.Type is IdentifierNameSyntax identifierName && identifierName.Identifier.ValueText == interfaceName)
                {
                    return true;
                }
            }
            return false;
        }

        public static ClassDeclarationSyntax GetClassNode(this GeneratorSyntaxContext context)
        {
            return (ClassDeclarationSyntax)context.Node;
        }

        public static INamedTypeSymbol? GetClassSymbol(this Compilation compilation, ClassDeclarationSyntax clazz)
        {
            var model = compilation.GetSemanticModel(clazz.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(clazz)!;
            return classSymbol as INamedTypeSymbol;
        }

        public static bool TryGetAttribute(this ISymbol symbol, string attributeName, out IEnumerable<AttributeData> attributes)
        {
            if (attributeName.EndsWith("Attribute") == false)
            {
                attributeName = $"{attributeName}Attribute";
            }
            List<AttributeData> output = [];
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass is null)
                {
                    continue;
                }
                if (attribute.AttributeClass.Name == attributeName)
                {
                    output.Add(attribute);
                }
            }
            //trying a faster method.
            attributes = output;
            return output.Count > 0;
        }
        public static bool TryGetAttribute(this ISymbol symbol, INamedTypeSymbol attributeType, out IEnumerable<AttributeData> attributes)
        {
            attributes = symbol.GetAttributes()
                .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
            return attributes.Any();
        }
    }
}
