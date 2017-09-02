using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout
{
    /// <summary>
    /// Analyzes the layout of the dot operator.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DotOperatorLayoutAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor
            MultiLineExpressionsShouldNotBeSplitAfterADotOrMemberAccessTokenDescriptor
            = new DiagnosticDescriptor("CT3530",
                "Multi-line expressions should not be split after a dot or member access token.",
                "Multi-line expressions should not be split after a dot or member access token.",
                "CodeTiger.Layout", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    MultiLineExpressionsShouldNotBeSplitAfterADotOrMemberAccessTokenDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeMemberAccessExpression,
                SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.PointerMemberAccessExpression);
            context.RegisterSyntaxNodeAction(AnalyzeQualifiedNameSyntax, SyntaxKind.QualifiedName);
        }

        private static void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (MemberAccessExpressionSyntax)context.Node;

            if (IsLineSplitAfterToken(node.OperatorToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MultiLineExpressionsShouldNotBeSplitAfterADotOrMemberAccessTokenDescriptor,
                    node.OperatorToken.GetLocation()));
            }
        }

        private static void AnalyzeQualifiedNameSyntax(SyntaxNodeAnalysisContext context)
        {
            var node = (QualifiedNameSyntax)context.Node;

            if (IsLineSplitAfterToken(node.DotToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MultiLineExpressionsShouldNotBeSplitAfterADotOrMemberAccessTokenDescriptor,
                    node.DotToken.GetLocation()));
            }
        }

        private static bool IsLineSplitAfterToken(SyntaxToken token)
        {
            var tokenLineSpan = token.GetLocation().GetLineSpan();

            var nextToken = token.GetNextToken();
            while (nextToken != default(SyntaxToken))
            {
                var nextTokenLineSpan = nextToken.GetLocation().GetLineSpan();
                if (nextTokenLineSpan.StartLinePosition.Line != tokenLineSpan.EndLinePosition.Line)
                {
                    break;
                }

                if (!SyntaxFacts.IsTrivia(nextToken.Kind()))
                {
                    return false;
                }

                nextToken = nextToken.GetNextToken();
            }

            return true;
        }
    }
}
