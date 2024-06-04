using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;
using System.Threading;

namespace TraitsGen
{
    [Generator]
    public class TraitsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Add the marker attribute to the compilation
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "TraitsAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

            var details = context.SyntaxProvider.ForAttributeWithMetadataName("TraitsGen.TraitsAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
               transform: GetTypeToGenerate);
        }

        static ClassToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
        {
            return new ClassToGenerate();
        }
    }
}
