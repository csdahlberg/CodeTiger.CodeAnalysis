using System;
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

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
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
        }

        private void AnalyzeConstructorInitializer(SyntaxNodeAnalysisContext context)
        {
            var node = (ConstructorInitializerSyntax)context.Node;

            if (node.ArgumentList?.OpenParenToken == null)
            {
                return;
            }

            AnalyzeOpenParenthesis(context, node.ThisOrBaseKeyword.GetLocation(),
                node.ArgumentList.OpenParenToken);
        }

        private void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var node = (CatchClauseSyntax)context.Node;

            if (node.Declaration?.OpenParenToken == null)
            {
                return;
            }

            AnalyzeOpenParenthesis(context, node.CatchKeyword.GetLocation(), node.Declaration.OpenParenToken);
        }

        private void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (DefaultExpressionSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (FixedStatementSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.FixedKeyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForStatementSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.ForKeyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForEachStatementSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.ForEachKeyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (IfStatementSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.IfKeyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (LockStatementSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.LockKeyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeSizeOfExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (SizeOfExpressionSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (SwitchStatementSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.SwitchKeyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeTypeOfExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (TypeOfExpressionSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (UsingStatementSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.UsingKeyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (WhileStatementSyntax)context.Node;

            AnalyzeOpenParenthesis(context, node.WhileKeyword.GetLocation(), node.OpenParenToken);
        }

        private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (InvocationExpressionSyntax)context.Node;

            if (node.ArgumentList?.OpenParenToken == null)
            {
                return;
            }

            var targetExpression = node.Expression as IdentifierNameSyntax;
            if (string.Equals(targetExpression?.Identifier.Text, "nameof", StringComparison.OrdinalIgnoreCase))
            {
                AnalyzeOpenParenthesis(context, node.Expression.GetLocation(),
                    node.ArgumentList.OpenParenToken);
            }
        }

        private static void AnalyzeOpenParenthesis(SyntaxNodeAnalysisContext context,
            Location precedingKeywordOrIdentifierLocation, SyntaxToken openParenthesisToken)
        {
            if (openParenthesisToken.IsMissing)
            {
                return;
            }

            if (openParenthesisToken.GetLocation().GetLineSpan().StartLinePosition.Line
                != precedingKeywordOrIdentifierLocation.GetLineSpan().EndLinePosition.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor,
                    openParenthesisToken.GetLocation()));
            }
        }
    }
}
