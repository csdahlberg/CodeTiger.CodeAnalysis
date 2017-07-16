using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability
{
    /// <summary>
    /// Analyzes enumeration of collections for potential reliability issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnumerationReliabilityAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor CollectionsBeingIteratedOverShouldNotBeModifiedDescriptor
            = new DiagnosticDescriptor("CT2000", "Collections being iterated over should not be modified.",
                "Collections being iterated over should not be modified.", "CodeTiger.Reliability",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(CollectionsBeingIteratedOverShouldNotBeModifiedDescriptor);
            }
        }

        /// <summary>
        /// Registers actions in an analysis context.
        /// </summary>
        /// <param name="context">The context to register actions in.</param>
        /// <remarks>This method should only be called once, at the start of a session.</remarks>
        public override void Initialize(AnalysisContext context)
        {
            Guard.ArgumentIsNotNull(nameof(context), context);

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSemanticModelAction(AnalyzeEnumerations);
        }

        private static void AnalyzeEnumerations(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            foreach (var forEachStatement in root.DescendantNodes().OfType<ForEachStatementSyntax>())
            {
                AnalyzeEnumeration(context, forEachStatement.Expression, forEachStatement.Statement);
            }
        }

        private static void AnalyzeEnumeration(SemanticModelAnalysisContext context,
            ExpressionSyntax enumeratedExpression, StatementSyntax enumerationStatement)
        {
            switch (enumeratedExpression.Kind())
            {
                case SyntaxKind.SimpleAssignmentExpression:
                    AnalyzeEnumeration(context, ((AssignmentExpressionSyntax)enumeratedExpression).Right,
                        enumerationStatement);
                    break;
                case SyntaxKind.IdentifierName:
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.PointerMemberAccessExpression:
                    AnalyzeEnumeratedExpression(context, enumeratedExpression, enumerationStatement);
                    break;
            }
        }

        private static void AnalyzeEnumeratedExpression(SemanticModelAnalysisContext context,
            ExpressionSyntax enumeratedExpression, StatementSyntax enumerationStatement)
        {
            foreach (var invocationExpression in enumerationStatement.DescendantNodesAndSelf()
                .OfType<InvocationExpressionSyntax>())
            {
                if (DoesInvocationProbablyModifyCollection(context, enumeratedExpression, invocationExpression))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        CollectionsBeingIteratedOverShouldNotBeModifiedDescriptor,
                        invocationExpression.Expression.GetLocation()));
                }
            }
        }

        private static bool DoesInvocationProbablyModifyCollection(SemanticModelAnalysisContext context,
            ExpressionSyntax collectionExpression, InvocationExpressionSyntax invocationExpression)
        {
            if (invocationExpression.Expression.Kind() != SyntaxKind.PointerMemberAccessExpression
                && invocationExpression.Expression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
            {
                return false;
            }

            var invocationMemberAccess = (MemberAccessExpressionSyntax)invocationExpression.Expression;
            var invocationSymbolInfo = context.SemanticModel.GetSymbolInfo(invocationMemberAccess.Expression,
                context.CancellationToken);

            var collectionExpressionSymbolInfo = context.SemanticModel.GetSymbolInfo(collectionExpression,
                context.CancellationToken);

            if (collectionExpressionSymbolInfo.Symbol != invocationSymbolInfo.Symbol)
            {
                return false;
            }

            return IsProbablyACollectionModifyingMethod(invocationMemberAccess.Name);
        }

        private static bool IsProbablyACollectionModifyingMethod(SimpleNameSyntax methodName)
        {
            switch (methodName.Identifier.Text)
            {
                case "Add":
                case "AddRange":
                case "Clear":
                case "Insert":
                case "InsertRange":
                case "Remove":
                case "RemoveAll":
                case "RemoveAt":
                case "RemoveRange":
                    return true;
                default:
                    return false;
            }
        }
    }
}