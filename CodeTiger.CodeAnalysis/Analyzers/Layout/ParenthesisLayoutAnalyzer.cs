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
        internal static readonly DiagnosticDescriptor
            ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor = new DiagnosticDescriptor(
                "CT3535", "Closing parenthesis should be on the same line as the preceding element.",
                "Closing parenthesis should be on the same line as the preceding element.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor EmptyParenthesesShouldBeOnTheSameLineDescriptor
            = new DiagnosticDescriptor("CT3536", "Empty parentheses should be on the same line.",
                "Empty parentheses should be on the same line.", "CodeTiger.Layout", DiagnosticSeverity.Warning,
                true);

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
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor,
                    EmptyParenthesesShouldBeOnTheSameLineDescriptor);
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
            context.RegisterSyntaxNodeAction(AnalyzeCastExpression, SyntaxKind.CastExpression);
            context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConversionOperatorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeDestructorDeclaration, SyntaxKind.DestructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeOperatorDeclaration, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeParenthesizedLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void AnalyzeConstructorInitializer(SyntaxNodeAnalysisContext context)
        {
            var node = (ConstructorInitializerSyntax)context.Node;

            if (node.ArgumentList != null && node.ArgumentList.OpenParenToken != default(SyntaxToken))
            {
                AnalyzeParenthesis(context, node.ThisOrBaseKeyword.GetLocation(),
                    node.ArgumentList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
            }

            if (node.ArgumentList != null && node.ArgumentList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ArgumentList.Arguments.Any())
                {
                    AnalyzeParenthesis(context, node.ArgumentList.Arguments.Last().GetLocation(),
                        node.ArgumentList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ArgumentList.OpenParenToken.GetLocation(),
                        node.ArgumentList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
            }
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var node = (CatchClauseSyntax)context.Node;

            if (node.Declaration != null && node.Declaration.OpenParenToken != default(SyntaxToken))
            {
                AnalyzeParenthesis(context, node.CatchKeyword.GetLocation(), node.Declaration.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);
            }

            if (node.Declaration != null && node.Declaration.CloseParenToken != default(SyntaxToken))
            {
                Location precedingTokenLocation;
                if (node.Declaration.Identifier != default(SyntaxToken))
                {
                    precedingTokenLocation = node.Declaration.Identifier.GetLocation();
                }
                else if (node.Declaration.Type != null)
                {
                    precedingTokenLocation = node.Declaration.Type.GetLocation();
                }
                else
                {
                    precedingTokenLocation = null;
                }

                if (precedingTokenLocation != null && precedingTokenLocation.Kind != LocationKind.None)
                {
                    AnalyzeParenthesis(context, precedingTokenLocation, node.Declaration.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
                }
            }
        }

        private static void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (DefaultExpressionSyntax)context.Node;

            AnalyzeParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            AnalyzeParenthesis(context, node.Type.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
        }

        private static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (FixedStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.FixedKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            AnalyzeParenthesis(context, node.Declaration?.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.ForKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            var lastTokenBeforeClosingParenthesis = node.Incrementors.Last()?.GetLastToken()
                ?? node.SecondSemicolonToken;
            AnalyzeParenthesis(context, lastTokenBeforeClosingParenthesis.GetLocation(), node.CloseParenToken,
                ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
        }

        private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForEachStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.ForEachKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            if (node.Expression != null)
            {
                AnalyzeParenthesis(context, node.Expression.GetLocation(), node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
            }
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (IfStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.IfKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            if (node.Condition != null)
            {
                AnalyzeParenthesis(context, node.Condition.GetLocation(), node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
            }
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (LockStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.LockKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            if (node.Expression != null)
            {
                AnalyzeParenthesis(context, node.Expression.GetLocation(), node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
            }
        }

        private static void AnalyzeSizeOfExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (SizeOfExpressionSyntax)context.Node;

            AnalyzeParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            if (node.Type != null)
            {
                AnalyzeParenthesis(context, node.Type.GetLocation(), node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
            }
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (SwitchStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.SwitchKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            if (node.Expression != null)
            {
                AnalyzeParenthesis(context, node.Expression.GetLocation(), node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
            }
        }

        private static void AnalyzeTypeOfExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (TypeOfExpressionSyntax)context.Node;

            AnalyzeParenthesis(context, node.Keyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            if (node.Type != null)
            {
                AnalyzeParenthesis(context, node.Type.GetLocation(), node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
            }
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (UsingStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.UsingKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            if (node.Declaration != null || node.Expression != null)
            {
                var declarationOrExpressionLocation = node.Declaration?.GetLocation()
                    ?? node.Expression.GetLocation();
                AnalyzeParenthesis(context, declarationOrExpressionLocation, node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
            }
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (WhileStatementSyntax)context.Node;

            AnalyzeParenthesis(context, node.WhileKeyword.GetLocation(), node.OpenParenToken,
                OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingKeywordDescriptor);

            if (node.Condition != null)
            {
                AnalyzeParenthesis(context, node.Condition.GetLocation(), node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
            }
        }

        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (InvocationExpressionSyntax)context.Node;

            if (node.ArgumentList != null && node.ArgumentList.OpenParenToken != default(SyntaxToken)
                && node.Expression != null)
            {
                DiagnosticDescriptor descriptor;
                if (node.Expression is SimpleNameSyntax simpleNameExpression 
                    && string.Equals(simpleNameExpression.Identifier.Text, "nameof", StringComparison.OrdinalIgnoreCase))
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

            if (node.ArgumentList != null && node.ArgumentList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ArgumentList.Arguments.Any())
                {
                    AnalyzeParenthesis(context, node.ArgumentList.Arguments.Last().GetLocation(),
                        node.ArgumentList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ArgumentList.OpenParenToken.GetLocation(),
                        node.ArgumentList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
            }
        }

        private static void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (ObjectCreationExpressionSyntax)context.Node;

            if (node.ArgumentList != null && node.ArgumentList.OpenParenToken != default(SyntaxToken))
            {
                AnalyzeParenthesis(context, node.Type?.GetLocation(), node.ArgumentList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor);
            }

            if (node.ArgumentList != null && node.ArgumentList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ArgumentList.Arguments.Any())
                {
                    AnalyzeParenthesis(context, node.ArgumentList.Arguments.Last().GetLocation(),
                        node.ArgumentList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingArgumentDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ArgumentList.OpenParenToken.GetLocation(),
                        node.ArgumentList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
            }
        }

        private static void AnalyzeCastExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (CastExpressionSyntax)context.Node;

            if (node.Type != null)
            {
                AnalyzeParenthesis(context, node.Type.GetLocation(), node.CloseParenToken,
                    ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
            }
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (ConstructorDeclarationSyntax)context.Node;

            if (node.ParameterList != null && node.ParameterList.OpenParenToken != default(SyntaxToken))
            {
                AnalyzeParenthesis(context, node.Identifier.GetLocation(), node.ParameterList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor);
            }

            if (node.ParameterList != null && node.ParameterList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ParameterList.Parameters.Any())
                {
                    AnalyzeParenthesis(context, node.ParameterList.Parameters.Last().GetLocation(),
                        node.ParameterList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ParameterList.OpenParenToken.GetLocation(),
                        node.ParameterList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
            }
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (ConversionOperatorDeclarationSyntax)context.Node;

            if (node.ParameterList != null && node.ParameterList.OpenParenToken != default(SyntaxToken)
                && node.Type != null)
            {
                AnalyzeParenthesis(context, node.Type.GetLocation(), node.ParameterList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor);
            }

            if (node.ParameterList != null && node.ParameterList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ParameterList.Parameters.Any())
                {
                    AnalyzeParenthesis(context, node.ParameterList.Parameters.Last().GetLocation(),
                        node.ParameterList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ParameterList.OpenParenToken.GetLocation(),
                        node.ParameterList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
            }
        }

        private static void AnalyzeDestructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (DestructorDeclarationSyntax)context.Node;

            if (node.ParameterList != null && node.ParameterList.OpenParenToken != default(SyntaxToken))
            {
                AnalyzeParenthesis(context, node.Identifier.GetLocation(), node.ParameterList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor);
            }

            if (node.ParameterList != null && node.ParameterList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ParameterList.Parameters.Any())
                {
                    AnalyzeParenthesis(context, node.ParameterList.Parameters.Last().GetLocation(),
                        node.ParameterList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ParameterList.OpenParenToken.GetLocation(),
                        node.ParameterList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
            }
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (MethodDeclarationSyntax)context.Node;

            if (node.ParameterList != null && node.ParameterList.OpenParenToken != default(SyntaxToken))
            {
                AnalyzeParenthesis(context, node.Identifier.GetLocation(), node.ParameterList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor);
            }

            if (node.ParameterList != null && node.ParameterList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ParameterList.Parameters.Any())
                {
                    AnalyzeParenthesis(context, node.ParameterList.Parameters.Last().GetLocation(),
                        node.ParameterList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ParameterList.OpenParenToken.GetLocation(),
                        node.ParameterList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
            }
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (OperatorDeclarationSyntax)context.Node;

            if (node.ParameterList != null && node.ParameterList.OpenParenToken != default(SyntaxToken)
                && node.ReturnType != null)
            {
                AnalyzeParenthesis(context, node.ReturnType.GetLocation(), node.ParameterList.OpenParenToken,
                    OpeningParenthesisShouldBeOnTheSameLineAsThePrecedingIdentifierDescriptor);
            }

            if (node.ParameterList != null && node.ParameterList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ParameterList.Parameters.Any())
                {
                    AnalyzeParenthesis(context, node.ParameterList.Parameters.Last().GetLocation(),
                        node.ParameterList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ParameterList.OpenParenToken.GetLocation(),
                        node.ParameterList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
            }
        }

        private static void AnalyzeParenthesizedLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (ParenthesizedLambdaExpressionSyntax)context.Node;

            if (node.ParameterList != null && node.ParameterList.CloseParenToken != default(SyntaxToken))
            {
                if (node.ParameterList.Parameters.Any())
                {
                    AnalyzeParenthesis(context, node.ParameterList.Parameters.Last().GetLocation(),
                        node.ParameterList.CloseParenToken,
                        ClosingParenthesisShouldBeOnTheSameLineAsThePrecedingElementDescriptor);
                }
                else
                {
                    AnalyzeParenthesis(context, node.ParameterList.OpenParenToken.GetLocation(),
                        node.ParameterList.CloseParenToken, EmptyParenthesesShouldBeOnTheSameLineDescriptor);
                }
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
