namespace Gu.Analyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GU0007PreferInjecting : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "GU0007";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Prefer injecting.",
            messageFormat: "Prefer injecting {0}.",
            category: AnalyzerCategory.Correctness,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: AnalyzerConstants.DisabledByDefault,
            description: "Prefer injecting.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => HandleObjectCreation(c), SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(c => HandleMemberAccess(c), SyntaxKind.SimpleMemberAccessExpression);
        }

        internal static Inject.Injectable CanInject(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (objectCreation?.ArgumentList == null)
            {
                return Inject.Injectable.No;
            }

            var injectable = Inject.Injectable.Safe;
            foreach (var argument in objectCreation.ArgumentList.Arguments)
            {
                var temp = IsInjectable(argument.Expression, semanticModel, cancellationToken);
                switch (temp)
                {
                    case Inject.Injectable.No:
                        return Inject.Injectable.No;
                    case Inject.Injectable.Safe:
                        break;
                    case Inject.Injectable.Unsafe:
                        injectable = Inject.Injectable.Unsafe;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return injectable;
        }

        internal static Inject.Injectable IsInjectable(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            switch (expression)
            {
                case IdentifierNameSyntax identifierName:
                    return Inject.TryFindConstructor(identifierName, out _)
                        ? Inject.Injectable.Safe
                        : Inject.Injectable.No;
                case ObjectCreationExpressionSyntax nestedObjectCreation:
                    return CanInject(nestedObjectCreation, semanticModel, cancellationToken) == Inject.Injectable.No ? Inject.Injectable.No : Inject.Injectable.Safe;
                case MemberAccessExpressionSyntax memberAccess:
                    if (memberAccess.Parent is AssignmentExpressionSyntax assignment &&
                        assignment.Left == expression)
                    {
                        return Inject.Injectable.No;
                    }

                    if (MemberPath.TryFindRoot(memberAccess, out var rootMember) &&
                        semanticModel.TryGetSymbol(rootMember, cancellationToken, out ISymbol rootSymbol))
                    {
                        switch (rootSymbol)
                        {
                            case IParameterSymbol _:
                            case IFieldSymbol _:
                            case IPropertySymbol _:
                                return IsInjectable(memberAccess.Name, semanticModel, cancellationToken);
                        }
                    }
                    return Inject.Injectable.No;
                default:
                    return Inject.Injectable.No;
            }
        }

        internal static INamedTypeSymbol MemberType(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var symbol = semanticModel.GetSymbolSafe(memberAccess, cancellationToken);
            if (symbol == null ||
                symbol.IsStatic)
            {
                return null;
            }

            if (symbol is IPropertySymbol property)
            {
                if (!property.Type.IsSealed &&
                    !property.Type.IsValueType &&
                    AssignedType(symbol, semanticModel, cancellationToken, out var memberType))
                {
                    return memberType as INamedTypeSymbol;
                }

                return property.Type as INamedTypeSymbol;
            }

            if (symbol is IFieldSymbol field)
            {
                if (!field.Type.IsSealed &&
                    !field.Type.IsValueType &&
                    AssignedType(symbol, semanticModel, cancellationToken, out var memberType))
                {
                    return memberType as INamedTypeSymbol;
                }

                return field.Type as INamedTypeSymbol;
            }

            return null;
        }

        internal static bool IsRootValid(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (MemberPath.TryFindRoot(memberAccess, out var root))
            {
                var symbol = semanticModel.GetSymbolSafe(root, cancellationToken);
                if (symbol is IParameterSymbol parameter)
                {
                    if (parameter.IsParams)
                    {
                        return false;
                    }

                    if (parameter.ContainingSymbol is IMethodSymbol method)
                    {
                        switch (method.MethodKind)
                        {
                            case MethodKind.AnonymousFunction:
                            case MethodKind.Conversion:
                            case MethodKind.DelegateInvoke:
                            case MethodKind.Destructor:
                            case MethodKind.EventAdd:
                            case MethodKind.EventRaise:
                            case MethodKind.EventRemove:
                            case MethodKind.UserDefinedOperator:
                            case MethodKind.ReducedExtension:
                            case MethodKind.StaticConstructor:
                            case MethodKind.BuiltinOperator:
                            case MethodKind.DeclareMethod:
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        private static void HandleObjectCreation(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                !context.ContainingSymbol.IsStatic &&
                context.Node is ObjectCreationExpressionSyntax objectCreation &&
                !context.ContainingSymbol.IsStatic &&
                Inject.TryFindConstructor(objectCreation, out _) &&
                CanInject(objectCreation, context.SemanticModel, context.CancellationToken) is var injectable &&
                injectable != Inject.Injectable.No &&
                context.SemanticModel.TryGetNamedType(objectCreation, context.CancellationToken, out var createdType) &&
                IsInjectionType(createdType))
            {
                var typeName = createdType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart);
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        objectCreation.GetLocation(),
                        ImmutableDictionary<string, string>.Empty.Add(nameof(INamedTypeSymbol), typeName)
                                                                 .Add(nameof(Inject.Injectable), injectable.ToString()),
                        typeName));
            }
        }

        private static void HandleMemberAccess(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                !context.ContainingSymbol.IsStatic &&
                context.Node is MemberAccessExpressionSyntax memberAccess &&
                !context.ContainingSymbol.IsStatic &&
                Inject.TryFindConstructor(memberAccess, out _))
            {
                if (memberAccess.Parent is AssignmentExpressionSyntax assignment &&
                    assignment.Left == memberAccess)
                {
                    return;
                }

                if (memberAccess.Expression is ThisExpressionSyntax ||
                    memberAccess.Expression is BaseExpressionSyntax)
                {
                    return;
                }

                if (!IsRootValid(memberAccess, context.SemanticModel, context.CancellationToken))
                {
                    return;
                }

                var memberType = MemberType(memberAccess, context.SemanticModel, context.CancellationToken);
                if (memberType == null ||
                    !IsInjectionType(memberType))
                {
                    return;
                }

                if (IsInjectable(memberAccess, context.SemanticModel, context.CancellationToken) is var injectable &&
                    injectable != Inject.Injectable.No)
                {
                    var typeName = memberType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart);
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor,
                            memberAccess.Name.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(nameof(INamedTypeSymbol), typeName)
                                                                     .Add(nameof(Inject.Injectable), injectable.ToString()),
                            typeName));
                }
            }
        }

        private static bool AssignedType(ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol memberType)
        {
            foreach (var reference in symbol.DeclaringSyntaxReferences)
            {
                var node = reference.GetSyntax(cancellationToken);

                if (AssignmentExecutionWalker.SingleFor(symbol, node.FirstAncestor<TypeDeclarationSyntax>(), Scope.Member, semanticModel, cancellationToken, out var assignment) &&
                    assignment.Right is IdentifierNameSyntax identifier)
                {
                    var ctor = assignment.FirstAncestor<ConstructorDeclarationSyntax>();
                    if (ctor != null &&
                        ctor.ParameterList != null &&
                        ctor.ParameterList.Parameters.TryFirst(
                            p => p.Identifier.ValueText == identifier.Identifier.ValueText,
                            out var parameter))
                    {
                        memberType = semanticModel.GetDeclaredSymbolSafe(parameter, cancellationToken)?.Type;
                        return true;
                    }
                }
            }

            memberType = null;
            return false;
        }

        private static bool IsInjectionType(ITypeSymbol type)
        {
            if (type?.ContainingNamespace == null ||
                type.IsValueType ||
                type.IsStatic ||
                type.DeclaringSyntaxReferences.Length == 0)
            {
                return false;
            }

            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.Constructors.Length != 1)
                {
                    return false;
                }

                var ctor = namedType.Constructors[0];
                if (ctor.Parameters.Length == 0)
                {
                    return true;
                }

                if (ctor.Parameters[ctor.Parameters.Length - 1]
                        .IsParams)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
