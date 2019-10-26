namespace Gu.Analyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofFix))]
    [Shared]
    internal class UseNameofFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.GU0006UseNameof.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText) || token.IsMissing)
                {
                    continue;
                }

                if (syntaxRoot.FindNode(diagnostic.Location.SourceSpan) is ArgumentSyntax argument &&
                    argument.Expression is LiteralExpressionSyntax literal)
                {
                    context.RegisterCodeFix(
                        "Use nameof",
                        (editor, cancellationToken) => FixAsync(editor, argument, literal.Token.ValueText, cancellationToken),
                        this.GetType().FullName,
                        diagnostic);
                }
            }
        }

        private static async Task FixAsync(DocumentEditor editor, ArgumentSyntax argument, string name, CancellationToken cancellationToken)
        {
            if (!IsStaticContext(argument, editor.SemanticModel, cancellationToken) &&
                editor.SemanticModel.LookupSymbols(argument.SpanStart, name: name).TrySingle(out var member) &&
                (member is IFieldSymbol || member is IPropertySymbol || member is IMethodSymbol) &&
                !member.IsStatic &&
                await Qualify(member).ConfigureAwait(false) != CodeStyleResult.No)
            {
                editor.ReplaceNode(
                    argument.Expression,
                    (x, _) => SyntaxFactory.ParseExpression($"nameof(this.{name})").WithTriviaFrom(x));
            }
            else
            {
                editor.ReplaceNode(
                    argument.Expression,
                    (x, _) => SyntaxFactory.ParseExpression($"nameof({name})").WithTriviaFrom(x));
            }

            Task<CodeStyleResult> Qualify(ISymbol symbol)
            {
                return symbol.Kind switch
                {
                    SymbolKind.Field => editor.QualifyFieldAccessAsync(cancellationToken),
                    SymbolKind.Event => editor.QualifyEventAccessAsync(cancellationToken),
                    SymbolKind.Property => editor.QualifyPropertyAccessAsync(cancellationToken),
                    SymbolKind.Method => editor.QualifyMethodAccessAsync(cancellationToken),
                    _ => throw new ArgumentOutOfRangeException(nameof(symbol)),
                };
            }
        }

        private static bool IsStaticContext(SyntaxNode context, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var accessor = context.FirstAncestor<AccessorDeclarationSyntax>();
            if (accessor != null)
            {
                return semanticModel.GetDeclaredSymbolSafe(accessor.FirstAncestor<PropertyDeclarationSyntax>(), cancellationToken)
                                    ?.IsStatic != false;
            }

            var methodDeclaration = context.FirstAncestor<MethodDeclarationSyntax>();
            return semanticModel.GetDeclaredSymbolSafe(methodDeclaration, cancellationToken)?.IsStatic != false;
        }
    }
}
