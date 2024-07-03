using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace TraitsGen
{
    [Generator]
    public class OmitAttributeGenerator : AttributeGenerator
    {
        protected override string AttributeName => "OmitAttribute`1";

        protected override (string Name, string Source) CreateAttribteSource(IncrementalGeneratorInitializationContext context)
        {
            return ($"OmitAttribute.g.cs", SourceGenerationHelper.OmitAttribute);
        }

        protected override string? TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
        {

            var sb = new StringBuilder();

            foreach (var attribute in attributeList)
            {
                var traitsType = attribute.AttributeClass.TypeArguments[0];
                var members = traitsType.GetMembers().Where(x => x.CanBeReferencedByName);

                var omitProperties = attribute.ConstructorArguments[0].Values.Select(x=>x.Value).Cast<string>();

                foreach (var member in members)
                {
                    if (omitProperties.Contains(member.OriginalDefinition.Name))
                    {
                        continue;
                    }

                    if (member is IPropertySymbol property)
                    {
                        sb.AppendLine($"public {property.OriginalDefinition.Type} {property.OriginalDefinition.Name} {{ get; set; }}");
                    }
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
