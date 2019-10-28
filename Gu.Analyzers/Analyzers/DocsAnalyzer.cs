namespace Gu.Analyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DocsAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.GU0100WrongCrefType);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.XmlCrefAttribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is XmlCrefAttributeSyntax attribute &&
                attribute.Parent is XmlEmptyElementSyntax emptyElement &&
                emptyElement.Parent is XmlElementSyntax candidate &&
                IsAutoDoc() &&
                candidate.HasLocalName("param") &&
                TryGetTypes(out var parameterType, out var crefType) &&
                IsWrongCref(parameterType, crefType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.GU0100WrongCrefType,
                        attribute.Cref.GetLocation(),
                        parameterType.ToDisplayString()));
            }

            bool IsAutoDoc()
            {
                return candidate.Content.TryElementAt(1, out var match) &&
                       Equals(match, emptyElement) &&
                       candidate.Content.TryFirst(out var first) &&
                       first is XmlTextSyntax text &&
                       IsAutoPrefix();

                bool IsAutoPrefix()
                {
                    switch (text.ToString())
                    {
                        case "The ":
                        case "The left ":
                        case "The right ":
                        case "The first ":
                        case "The other ":
                            return true;
                        default:
                            return false;
                    }
                }
            }

            bool TryGetTypes(out ITypeSymbol parameterType, out ITypeSymbol crefType)
            {
                if (candidate.TryGetNameAttribute(out var nameAttribute) &&
                    context.ContainingSymbol is IMethodSymbol method &&
                    method.TryFindParameter(nameAttribute.Identifier.Identifier.ValueText, out var parameter) &&
                    context.SemanticModel.TryGetType(attribute.Cref, context.CancellationToken, out crefType!))
                {
                    parameterType = parameter.Type;
                    return true;
                }

                parameterType = null!;
                crefType = null!;
                return false;
            }

            bool IsWrongCref(ITypeSymbol parameterType, ITypeSymbol crefType)
            {
                return !Equals(parameterType.MetadataName, crefType.MetadataName);
            }
        }
    }
}
