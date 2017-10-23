﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout
{
    /// <summary>
    /// Analyzes parenthesis for layout issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParenthesisLayoutAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor
            OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor = new DiagnosticDescriptor(
                "CT3532", "Opening parenthesis should be on the same line as the preceding keyword.",
                "Opening parenthesis should be on the same line as the preceding keyword.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor = new DiagnosticDescriptor(
                "CT3533", "Opening parenthesis should be on the same line as the preceding identifier.",
                "Opening parenthesis should be on the same line as the preceding identifier.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor = new DiagnosticDescriptor(
                "CT3534", "Closing parenthesis should be on the same line as the preceding argument.",
                "Closing parenthesis should be on the same line as the preceding argument.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeConstructorInitializer, SyntaxKind.BaseConstructorInitializer,
                SyntaxKind.ThisConstructorInitializer);
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
            context.RegisterSyntaxNodeAction(AnalyzeDefaultExpression, SyntaxKind.DefaultExpression);
            context.RegisterSyntaxNodeAction(AnalyzeFixedStatement, SyntaxKind.FixedStatement);
            context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeLockStatement, SyntaxKind.LockStatement);
            context.RegisterSyntaxNodeAction(AnalyzeSizeOfExpression, SyntaxKind.SizeOfExpression);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(AnalyzeTypeOfExpression, SyntaxKind.TypeOfExpression);
            context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
            context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreationExpression, SyntaxKind.ObjectCreationExpression);
        }

        private void AnalyzeConstructorInitializer(SyntaxNodeAnalysisContext context)
        {
            var node = (ConstructorInitializerSyntax)context.Node;

            if (node.ArgumentList?.OpenParenToken != null)
            {
                AnalyzeParenthesis(context, node.ThisOrBaseKeyword.GetLocation(),
                    node.ArgumentList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
            }

            if (node.ArgumentList?.CloseParenToken != null && node.ArgumentList.Arguments.Any())
            {
                AnalyzeParenthesis(context, node.ArgumentList.Arguments.Last().GetLocation(),
                    node.ArgumentList.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
            }
        }

        private void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var node = (CatchClauseSyntax)context.Node;

            if (node.Declaration?.OpenParenToken == null)
            {
                return;
            }

            AnalyzeParenthesis(context, node.CatchKeyword.GetLocation(), node.Declaration.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
        }

        private void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (DefaultExpressionSyntax)context.Node;

            AnalyzeParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            AnalyzeParenthesis(context, node.Type.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
        }

        private void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (FixedStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.FixedKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
        }

        private void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.ForKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
        }

        private void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForEachStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.ForEachKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (IfStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.IfKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
        }

        private void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (LockStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.LockKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            AnalyzeParenthesis(context, node.Expression.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
        }

        private void AnalyzeSizeOfExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (SizeOfExpressionSyntax)context.Node;

            AnalyzeParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            AnalyzeParenthesis(context, node.Type.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (SwitchStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.SwitchKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            AnalyzeParenthesis(context, node.Expression.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
        }

        private void AnalyzeTypeOfExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (TypeOfExpressionSyntax)context.Node;

            AnalyzeParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            AnalyzeParenthesis(context, node.Type.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
        }

        private void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (UsingStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.UsingKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
        }

        private void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (WhileStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.WhileKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            AnalyzeParenthesis(context, node.Condition.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
        }

        private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (InvocationExpressionSyntax)context.Node;

            if (node.ArgumentList?.OpenParenToken != null)
            {
                var targetExpression = node.Expression as IdentifierNameSyntax;
                DiagnosticDescriptor descriptor;
                if (string.Equals(targetExpression?.Identifier.Text, "nameof", StringComparison.OrdinalIgnoreCase))
                {
                    descriptor = OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor;
                }
                else
                {
                    descriptor = OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor;
                }

                AnalyzeParenthesis(context, node.Expression.GetLocation(), node.ArgumentList.OpenParenToken,
                    descriptor);
            }

            if (node.ArgumentList?.CloseParenToken != null && node.ArgumentList.Arguments.Any())
            {
                AnalyzeParenthesis(context, node.ArgumentList.Arguments.Last().GetLocation(),
                    node.ArgumentList.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
            }
        }

        private void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (ObjectCreationExpressionSyntax)context.Node;

            if (node.ArgumentList?.OpenParenToken != null)
            {
                AnalyzeParenthesis(context, node.Type.GetLocation(), node.ArgumentList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor);
            }

            if (node.ArgumentList?.CloseParenToken != null && node.ArgumentList.Arguments.Any())
            {
                AnalyzeParenthesis(context, node.ArgumentList.Arguments.Last().GetLocation(),
                    node.ArgumentList.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
            }
        }

        private static void AnalyzeParenthesis(SyntaxNodeAnalysisContext context,
            Location precedingElementLocation, SyntaxToken parenthesisToken, DiagnosticDescriptor descriptor)
        {
            if (parenthesisToken.IsMissing)
            {
                return;
            }

            var parenthesisLocation = parenthesisToken.GetLocation();

            if (parenthesisLocation.GetLineSpan().StartLinePosition.Line
                != precedingElementLocation.GetLineSpan().EndLinePosition.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, parenthesisLocation));
            }
        }
    }
}
