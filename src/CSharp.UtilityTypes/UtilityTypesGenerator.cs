using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSharp.UtilityTypes
{
    [Generator]
    public class UtilityTypesGenerator : IIncrementalGenerator
    {
        internal record struct ClassInfo(string Code, string Compilation, string AddSource);

        string[] mixinAttributes = new[] { "Omit", "Pick", "Mixin" };

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "OmitAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.OmitAttribute, Encoding.UTF8)));
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "PickAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.PickAttribute, Encoding.UTF8)));
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
              "MixinAttribute.g.cs",
              SourceText.From(SourceGenerationHelper.MixinAttribute, Encoding.UTF8)));

            IncrementalValuesProvider<ClassDeclarationSyntax> declares = context.SyntaxProvider.CreateSyntaxProvider(
                (s, _) =>
                {
                    if (s is not ClassDeclarationSyntax ctx) return false;

                    if (ctx.AttributeLists.Any(x => x.Attributes.Any(o => o.Name is GenericNameSyntax gns && mixinAttributes.Contains(gns.Identifier.ValueText))))
                        return true;

                    return false;
                },
                (context, _) => context.GetClassNode())
                .Where(m => m != null)!;

            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilation
                = context.CompilationProvider.Combine(declares.Collect());

            context.RegisterSourceOutput(compilation, (spc, source) =>
            {
                Execute(source.Item1, source.Item2, spc);
            });
        }

        private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> list, SourceProductionContext context)
        {
            List<ClassInfo> others = new();

            Dictionary<string, string> properties = new();
            foreach (var ourClass in list)
            {
                string className = ourClass.Identifier.ValueText;
                INamedTypeSymbol symbol = compilation.GetClassSymbol(ourClass)!;
                var attributes = ourClass.AttributeLists.SelectMany(x => x.Attributes).Where(x => x.Name is GenericNameSyntax gns && mixinAttributes.Contains(gns.Identifier.ValueText));

                foreach (var attribute in attributes)
                {
                    var attributeName = ((GenericNameSyntax)attribute.Name).Identifier.ValueText;
                    var attributeData = symbol.TryGetAttribute(attributeName, out var data) ? data : [];
                    switch (attributeName)
                    {
                        case "Mixin":
                            GetMixinProperties(properties, attributeData);
                            break;
                        case "Omit":
                            GetOmitProperties(properties, attributeData);
                            break;
                        case "Pick":
                            GetPickProperties(properties, attributeData);
                            break;
                    }
                }

                var sb = new StringBuilder();
                foreach (var item in properties)
                {
                    sb.AppendLine($"public {item.Value} {item.Key} {{ get; set; }}");
                }

                var source = """
                namespace {{namespace}};

                public partial class {{className}}
                {
                    {{members}}
                }
                """
                 .Replace("{{className}}", symbol.Name)
                 .Replace("{{members}}", sb.ToString())
                 .Replace("{{namespace}}", symbol.ContainingNamespace.ToDisplayString())
                 ;

                context.AddSource($"{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}_MixinAttribute`1.g.cs", source);
            }
        }

        private void GetPickProperties(Dictionary<string, string> properties, IEnumerable<AttributeData> attributeData)
        {
            foreach (var attribute in attributeData)
            {
                var traitsType = attribute.AttributeClass.TypeArguments[0];
                var members = traitsType.GetMembers().Where(x => x.CanBeReferencedByName).OfType<IPropertySymbol>(); ;
                var namedArgument = attribute.ConstructorArguments[0];

                foreach (var member in members)
                {
                    var definition = member.OriginalDefinition;
                    if (namedArgument.Values.Any(x => x.Value.ToString() != definition.Name))
                    {
                        continue;
                    }

                    if (properties.ContainsKey(definition.Name))
                    {
                        properties[definition.Name] = "object";
                    }
                    else
                    {
                        properties.Add(definition.Name, definition.Type.Name);
                    }
                }
            }
        }

        private void GetMixinProperties(Dictionary<string, string> properties, IEnumerable<AttributeData> attributeList)
        {
            foreach (var attribute in attributeList)
            {
                var traitsType = attribute.AttributeClass?.TypeArguments[0];
                if (traitsType == null) continue;

                var members = traitsType.GetMembers().Where(x => x.CanBeReferencedByName).OfType<IPropertySymbol>();

                foreach (var member in members)
                {
                    var definition = member.OriginalDefinition;
                    if (properties.ContainsKey(definition.Name))
                    {
                        properties[definition.Name] = "object";
                    }
                    else
                    {
                        properties.Add(definition.Name, definition.Type.Name);
                    }
                }
            }
        }

        private void GetOmitProperties(Dictionary<string, string> properties, IEnumerable<AttributeData> attributeList)
        {
            foreach (var attribute in attributeList)
            {
                var traitsType = attribute.AttributeClass.TypeArguments[0];
                var members = traitsType.GetMembers().Where(x => x.CanBeReferencedByName).OfType<IPropertySymbol>();

                var omitProperties = attribute.ConstructorArguments[0].Values.Select(x => x.Value).Cast<string>();

                foreach (var member in members)
                {
                    var definition = member.OriginalDefinition;
                    if (omitProperties.Contains(definition.Name))
                    {
                        continue;
                    }

                    if (properties.ContainsKey(definition.Name))
                    {
                        properties.Remove(definition.Name);
                    }
                    else if (member is IPropertySymbol property)
                    {
                        properties.Add(definition.Name, definition.Type.Name);
                    }
                }
            }
        }
    }
}
