﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Readability
{
    /// <summary>
    /// Analyzes readability issues caused by the presence of unnecessary code.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnnecessaryCodeReadabilityAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor PositiveSignsShouldNotBeUsedDescriptor
            = new DiagnosticDescriptor("CT3105", "Positive signs should not be used.",
                "Positive signs should not be used.", "CodeTiger.Readability", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor StatementsShouldNotUseUnnecessaryParenthesisDescriptor
            = new DiagnosticDescriptor("CT3106", "Statements should not use unnecessary parenthesis.",
                "Statements should not use unnecessary parenthesis.", "CodeTiger.Readability",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            AnonymousMethodsWithoutParametersShouldNotIncludeParenthesisDescriptor = new DiagnosticDescriptor(
                "CT3107", "Anonymous methods without parameters should not include parenthesis.",
                "Anonymous methods without parameters should not include parenthesis.", "CodeTiger.Readability",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(PositiveSignsShouldNotBeUsedDescriptor,
                    StatementsShouldNotUseUnnecessaryParenthesisDescriptor,
                    AnonymousMethodsWithoutParametersShouldNotIncludeParenthesisDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeUnaryPlusUsage, SyntaxKind.UnaryPlusExpression);
            context.RegisterSyntaxNodeAction(AnalyzeUnnecessaryParenthesisUsage,
                SyntaxKind.ParenthesizedExpression);
            context.RegisterSyntaxNodeAction(AnalyzeAnonymousMethods, SyntaxKind.AnonymousMethodExpression);
        }

        private void AnalyzeUnaryPlusUsage(SyntaxNodeAnalysisContext context)
        {
            var node = (PrefixUnaryExpressionSyntax)context.Node;

            context.ReportDiagnostic(Diagnostic.Create(PositiveSignsShouldNotBeUsedDescriptor,
                node.OperatorToken.GetLocation()));
        }

        private void AnalyzeUnnecessaryParenthesisUsage(SyntaxNodeAnalysisContext context)
        {
            var node = (ParenthesizedExpressionSyntax)context.Node;

            // TODO: Expand to include cases where parenthesized expressions are used in other types of expressions
            // unnecessarily (for example, as the right side of assignments).

            if (node.Expression.Kind() == SyntaxKind.ParenthesizedExpression)
            {
                context.ReportDiagnostic(Diagnostic.Create(StatementsShouldNotUseUnnecessaryParenthesisDescriptor,
                    node.OpenParenToken.GetLocation()));
            }
        }

        private void AnalyzeAnonymousMethods(SyntaxNodeAnalysisContext context)
        {
            var node = (AnonymousMethodExpressionSyntax)context.Node;

            if (node.ParameterList != null && !node.ParameterList.Parameters.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    AnonymousMethodsWithoutParametersShouldNotIncludeParenthesisDescriptor,
                    node.ParameterList.GetLocation()));
            }
        }
    }
}