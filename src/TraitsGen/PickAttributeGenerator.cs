using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace TraitsGen
{
    [Generator]
    public class PickAttributeGenerator : AttributeGenerator
    {
        protected override string AttributeName => "PickAttribute`1";

        protected override (string Name, string Source) CreateAttribteSource(IncrementalGeneratorInitializationContext context)
        {
            return ($"PickAttribute.g.cs", SourceGenerationHelper.PickAttribute);
        }

        protected override string? TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
        {

            var sb = new StringBuilder();

            foreach (var attribute in attributeList)
            {
                var traitsType = attribute.AttributeClass.TypeArguments[0];
                var members = traitsType.GetMembers().Where(x => x.CanBeReferencedByName);

                foreach (var member in members)
                {
                    if (member is IPropertySymbol property)
                    {
                        sb.AppendLine($"public {property.OriginalDefinition.Type} {property.OriginalDefinition.Name} {{ get; set; }}");
                    }
                }

                var namedArgument = attribute.ConstructorArguments[0];
                foreach (var item in namedArgument.Values)
                {
                    sb.AppendLine($"public string {item.Value} {{ get; set; }}");
                }
            }

            return """
                namespace {{namespace}};

                public partial class {{className}}
                {
                    {{members}}
                }
                """
                .Replace("{{className}}", typeSymbol.Name)
                .Replace("{{members}}", sb.ToString())
                .Replace("{{namespace}}", typeSymbol.ContainingNamespace.ToDisplayString())
                ;
        }
    }
}
