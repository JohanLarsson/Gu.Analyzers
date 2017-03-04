namespace Gu.Analyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static partial class Disposable
    {
        internal static bool IsAssignedWithCreatedAndNotCachedOrInjected(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (field == null ||
                !IsPotentiallyAssignableTo(field.Type))
            {
                return false;
            }

            using (var sources = AssignedValueWalker.Create(field, semanticModel, cancellationToken))
            {
                using (var recursive = RecursiveValues.Create(sources.Item, semanticModel, cancellationToken))
                {
                    return IsAssignedWithCreated(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe) &&
                           IsInjectedCore(recursive, semanticModel, cancellationToken) == Result.No;
                }
            }
        }

        internal static bool IsAssignedWithCreatedAndNotCachedOrInjected(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (property == null ||
                 !IsPotentiallyAssignableTo(property.Type))
            {
                return false;
            }

            using (var sources = AssignedValueWalker.Create(property, semanticModel, cancellationToken))
            {
                using (var recursive = RecursiveValues.Create(sources.Item, semanticModel, cancellationToken))
                {
                    return IsAssignedWithCreated(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe) &&
                           IsInjectedCore(recursive, semanticModel, cancellationToken) == Result.No;
                }
            }
        }

        internal static bool IsAssignedWithCreatedAndInjected(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (field == null ||
                !IsPotentiallyAssignableTo(field.Type))
            {
                return false;
            }

            using (var sources = AssignedValueWalker.Create(field, semanticModel, cancellationToken))
            {
                using (var recursive = RecursiveValues.Create(sources.Item, semanticModel, cancellationToken))
                {
                    return IsAssignedWithCreated(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe) &&
                           IsInjectedCore(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe);
                }
            }
        }

        internal static bool IsAssignedWithCreatedAndInjected(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (property == null ||
                !IsPotentiallyAssignableTo(property.Type))
            {
                return false;
            }

            using (var sources = AssignedValueWalker.Create(property, semanticModel, cancellationToken))
            {
                using (var recursive = RecursiveValues.Create(sources.Item, semanticModel, cancellationToken))
                {
                    return IsAssignedWithCreated(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe) &&
                           IsInjectedCore(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe);
                }
            }
        }

        /// <summary>
        /// Check if any path returns a created IDisposable
        /// </summary>
        internal static bool IsPotentiallyCachedOrInjected(InvocationExpressionSyntax disposeCall, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ExpressionSyntax member;
            if (TryGetDisposedRootMember(disposeCall, semanticModel, cancellationToken, out member))
            {
                if (IsInjectedCore(semanticModel.GetSymbolSafe(member, cancellationToken)).IsEither(Result.Yes, Result.Maybe))
                {
                    return true;
                }

                using (var sources = AssignedValueWalker.Create(member, semanticModel, cancellationToken))
                {
                    using (var recursive = RecursiveValues.Create(sources.Item, semanticModel, cancellationToken))
                    {
                        return IsInjectedCore(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if any path returns a created IDisposable
        /// </summary>
        internal static bool IsPotentiallyCachedOrInjected(ExpressionSyntax disposable, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (disposable == null ||
                disposable.IsMissing ||
                !IsPotentiallyAssignableTo(semanticModel.GetTypeInfoSafe(disposable, cancellationToken).Type))
            {
                return false;
            }

            if (IsInjectedCore(semanticModel.GetSymbolSafe(disposable, cancellationToken)) == Result.Yes)
            {
                return true;
            }

            return IsPotentiallyCachedOrInjectedCore(disposable, semanticModel, cancellationToken);
        }

        private static bool IsPotentiallyCachedOrInjectedCore(ExpressionSyntax value, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var symbol = semanticModel.GetSymbolSafe(value, cancellationToken);
            if (IsInjectedCore(symbol) == Result.Yes)
            {
                return true;
            }

            var property = symbol as IPropertySymbol;
            if (property != null &&
                !property.IsAutoProperty(cancellationToken))
            {
                using (var returnValues = ReturnValueWalker.Create(value, false, semanticModel, cancellationToken))
                {
                    using (var recursive = RecursiveValues.Create(returnValues.Item, semanticModel, cancellationToken))
                    {
                        return IsInjectedCore(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe);
                    }
                }
            }

            using (var sources = AssignedValueWalker.Create(value, semanticModel, cancellationToken))
            {
                using (var recursive = RecursiveValues.Create(sources.Item, semanticModel, cancellationToken))
                {
                    return IsInjectedCore(recursive, semanticModel, cancellationToken).IsEither(Result.Yes, Result.Maybe);
                }
            }
        }

        private static Result IsInjectedCore(RecursiveValues values, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (values.Count == 0)
            {
                return Result.No;
            }

            values.Reset();
            while (values.MoveNext())
            {
                var symbol = semanticModel.GetSymbolSafe(values.Current, cancellationToken);
                if (IsInjectedCore(symbol).IsEither(Result.Yes, Result.Maybe))
                {
                    return Result.Yes;
                }
            }

            return Result.No;
        }

        private static Result IsInjectedCore(ISymbol symbol)
        {
            if (symbol == null)
            {
                return Result.Unknown;
            }

            if (symbol is ILocalSymbol)
            {
                return Result.Unknown;
            }

            if (symbol is IParameterSymbol)
            {
                return Result.Yes;
            }

            var field = symbol as IFieldSymbol;
            if (field != null)
            {
                if (field.IsStatic)
                {
                    return Result.Yes;
                }

                if (field.IsReadOnly)
                {
                    return Result.No;
                }

                return field.DeclaredAccessibility != Accessibility.Private
                           ? Result.Maybe
                           : Result.No;
            }

            var property = symbol as IPropertySymbol;
            if (property != null)
            {
                if (property.IsStatic)
                {
                    return Result.Yes;
                }

                if (property.IsReadOnly ||
                    property.SetMethod == null)
                {
                    return Result.No;
                }

                return property.DeclaredAccessibility != Accessibility.Private &&
                       property.SetMethod.DeclaredAccessibility != Accessibility.Private
                           ? Result.Maybe
                           : Result.No;
            }

            return Result.No;
        }
    }
}