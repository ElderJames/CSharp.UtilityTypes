using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace TraitsGen
{
    public abstract class AttributeGenerator : IIncrementalGenerator
    {
        protected abstract string AttributeName { get; }

        private string AttributeFullName => "TraitsGen." + AttributeName;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var attribute = CreateAttribteSource(context);
            // Add the marker attribute to the compilation
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                attribute.Name,
                SourceText.From(attribute.Source, Encoding.UTF8)));

            var generatorAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeFullName,
                (_, _) => true,
                (syntaxContext, _) => syntaxContext
                ).Combine(context.CompilationProvider);

            context.RegisterSourceOutput(generatorAttributes, (spc, tuple) =>
            {
                var (ga, compilation) = tuple;

                // 注：此处我指定了一个特殊的`Attribute`，如果使用了它就禁用所有源生成器。
                // 如：[assembly: DisableSourceGenerator]
                //if (compilation.Assembly.GetAttributes().Any(attrData => attrData.AttributeClass?.ToDisplayString() == DisableSourceGeneratorAttribute))
                //    return;

                if (ga.TargetSymbol is not INamedTypeSymbol symbol)
                    return;

                if (TypeWithAttribute(symbol, ga.Attributes) is { } source)
                    spc.AddSource($"{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}_MixinAttribute`1.g.cs",
                        source);
            });
        }

        protected abstract (string Name, string Source) CreateAttribteSource(IncrementalGeneratorInitializationContext context);

        protected abstract string? TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList);
    }
}
