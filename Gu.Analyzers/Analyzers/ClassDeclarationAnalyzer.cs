namespace Gu.Analyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ClassDeclarationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.GU0024SealTypeWithDefaultMember,
            Descriptors.GU0025SealTypeWithOverridenEquality);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.ClassDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is INamedTypeSymbol type &&
                !type.IsStatic &&
                !type.IsSealed &&
                context.Node is ClassDeclarationSyntax classDeclaration)
            {
                if (HasDefaultMember(classDeclaration, context))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.GU0024SealTypeWithDefaultMember, classDeclaration.Identifier.GetLocation()));
                }

                if (Equality.IsOverriden(classDeclaration))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.GU0025SealTypeWithOverridenEquality, classDeclaration.Identifier.GetLocation()));
                }
            }
        }

        private static bool HasDefaultMember(ClassDeclarationSyntax classDeclaration, SyntaxNodeAnalysisContext context)
        {
            foreach (var member in classDeclaration.Members)
            {
                switch (member)
                {
                    case PropertyDeclarationSyntax property when IsStaticPublicOrInternal(property.Modifiers) &&
                                                                 property.IsGetOnly() &&
                                                                 IsInitializedWithContainingType(property.Initializer, context):
                        return true;
                    case FieldDeclarationSyntax field when IsStaticPublicOrInternal(field.Modifiers) &&
                                                           field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) &&
                                                           field.Declaration is VariableDeclarationSyntax declaration &&
                                                           declaration.Variables.TrySingle(out var variable) && IsInitializedWithContainingType(variable.Initializer, context):
                        return true;
                }
            }

            return false;

        }

        private static bool IsStaticPublicOrInternal(SyntaxTokenList modifiers)
        {
            return modifiers.Any(SyntaxKind.StaticKeyword) &&
                   (modifiers.Any(SyntaxKind.PublicKeyword) ||
                    modifiers.Any(SyntaxKind.InternalKeyword));
        }

        private static bool IsInitializedWithContainingType(EqualsValueClauseSyntax initializer, SyntaxNodeAnalysisContext context)
        {
            return initializer?.Value is ObjectCreationExpressionSyntax objectCreation &&
                   context.SemanticModel.TryGetType(objectCreation, context.CancellationToken, out var createdType) &&
                   createdType.Equals(context.ContainingSymbol);
        }
    }
}
