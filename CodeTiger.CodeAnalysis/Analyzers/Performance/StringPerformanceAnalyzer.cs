using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeTiger.CodeAnalysis.Analyzers.Performance
{
    /// <summary>
    /// Analyzes string operations for performance issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringPerformanceAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor
            StringEqualsShouldBeUsedForCaseInsensitiveEqualityChecksDescriptor
            = new DiagnosticDescriptor(
                "CT1800", "String.Equals should be used for case insensitive equality checks.",
                "String.Equals should be used for case insensitive equality checks.", "CodeTiger.Performance",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            StringBuilderShouldBeUsedInsteadOfMultipleStringConcatenationsDescriptor
            = new DiagnosticDescriptor(
                "CT1801", "StringBuilder should be used instead of multiple string concatenations.",
                "StringBuilder should be used instead of multiple string concatenations.",
                "CodeTiger.Performance", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(StringEqualsShouldBeUsedForCaseInsensitiveEqualityChecksDescriptor,
                    StringBuilderShouldBeUsedInsteadOfMultipleStringConcatenationsDescriptor);
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

            context.RegisterSemanticModelAction(AnalyzeStringComparisons);
            context.RegisterSemanticModelAction(AnalyzeStringConcatenation);
        }

        private void AnalyzeStringComparisons(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            foreach (var node in root.DescendantNodes())
            {
                switch (node.Kind())
                {
                    case SyntaxKind.EqualsExpression:
                    case SyntaxKind.NotEqualsExpression:
                        AnalyzeBinaryExpressionForStringComparisons(context, (BinaryExpressionSyntax)node);
                        break;
                }
            }
        }

        private static void AnalyzeStringConcatenation(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            foreach (var assignmentExpression in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
            {
                if (IsDefinitelyStringConcatenation(context, assignmentExpression)
                    && IsWithinLoop(assignmentExpression))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        StringBuilderShouldBeUsedInsteadOfMultipleStringConcatenationsDescriptor,
                        assignmentExpression.GetLocation()));
                }
            }

            // TODO: Improve this to identify cases where concatenation is done many times without any loops.
        }

        private static bool IsDefinitelyStringConcatenation(SemanticModelAnalysisContext context,
            AssignmentExpressionSyntax assignmentExpression)
        {
            var typeInfo = context.SemanticModel.GetTypeInfo(assignmentExpression.Left);
            if (typeInfo.Type?.SpecialType != SpecialType.System_String)
            {
                return false;
            }

            if (assignmentExpression.Kind() == SyntaxKind.AddAssignmentExpression)
            {
                return true;
            }

            var identifier = assignmentExpression.Left as IdentifierNameSyntax;
            if (identifier == null)
            {
                return false;
            }

            return assignmentExpression.Kind() == SyntaxKind.SimpleAssignmentExpression
                && assignmentExpression.Right.Kind() == SyntaxKind.AddExpression
                && assignmentExpression.Left.Kind() == SyntaxKind.IdentifierName
                && ((IdentifierNameSyntax)assignmentExpression.Left) == identifier;
        }

        private static bool IsWithinLoop(ExpressionSyntax expression)
        {
            var parent = expression.Parent;
            while (parent != null)
            {
                switch (parent.Kind())
                {
                    case SyntaxKind.ForEachStatement:
                    case SyntaxKind.ForStatement:
                        return true;
                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKind.ConstructorDeclaration:
                    case SyntaxKind.GetAccessorDeclaration:
                    case SyntaxKind.SetAccessorDeclaration:
                    case SyntaxKind.AddAccessorDeclaration:
                    case SyntaxKind.RemoveAccessorDeclaration:
                        return false;
                }

                parent = parent.Parent;
            }

            return false;
        }

        private static void AnalyzeBinaryExpressionForStringComparisons(SemanticModelAnalysisContext context,
            BinaryExpressionSyntax node)
        {
            if (IsResultOfCaseConversion(context, node.Left) || IsResultOfCaseConversion(context, node.Right))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    StringEqualsShouldBeUsedForCaseInsensitiveEqualityChecksDescriptor, node.GetLocation()));
            }
        }

        private static bool IsResultOfCaseConversion(SemanticModelAnalysisContext context,
            ExpressionSyntax expression)
        {
            if (expression.Kind() == SyntaxKind.InvocationExpression)
            {
                var invocationExpression = (InvocationExpressionSyntax)expression;
                var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
                if (memberAccessExpression != null && IsCallToCaseConversionMethod(context, memberAccessExpression))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCallToCaseConversionMethod(SemanticModelAnalysisContext context,
            MemberAccessExpressionSyntax memberAccessExpression)
        {
            var expressionType = context.SemanticModel.GetTypeInfo(memberAccessExpression.Expression);
            if (expressionType.Type?.SpecialType == SpecialType.System_String)
            {
                switch (memberAccessExpression.Name.Identifier.Text)
                {
                    case "ToLower":
                    case "ToLowerInvariant":
                    case "ToUpper":
                    case "ToUpperInvariant":
                        return true;
                }
            }

            return false;
        }
    }
}