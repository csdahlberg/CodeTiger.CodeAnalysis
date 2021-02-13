using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Usage
{
    /// <summary>
    /// Analyzes usages of <see cref="string"/> and related types.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StringUsageAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor InterpolatedStringsRequireLeadingDollarSignDescriptor
            = new DiagnosticDescriptor("CT2208", "Interpolated strings require a leading '$' character.",
                "Interpolated strings require a leading '$' character.", "CodeTiger.Usage",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(InterpolatedStringsRequireLeadingDollarSignDescriptor);

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

            context.RegisterSyntaxNodeAction(AnalyzeStringLiteral, SyntaxKind.StringLiteralExpression);
        }

        private static void AnalyzeStringLiteral(SyntaxNodeAnalysisContext context)
        {
            var node = (LiteralExpressionSyntax)context.Node;

            string value = (string)node.Token.Text;

            if (value.Contains("{") && value.Contains("}"))
            {
                var newNode = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_"),
                        SyntaxFactory.Token(SyntaxKind.EqualsToken),
                        SyntaxFactory.ParseExpression("$" + value)));
                if (context.SemanticModel
                    .TryGetSpeculativeSemanticModel(node.SpanStart, newNode, out var speculativeModel))
                {
                    AnalyzeInterpolatedStringFromStringLiteral(context, node, speculativeModel);
                }
            }
        }

        private static void AnalyzeInterpolatedStringFromStringLiteral(SyntaxNodeAnalysisContext context,
            LiteralExpressionSyntax node, SemanticModel speculativeModel)
        {
            var speculativeRoot = speculativeModel.SyntaxTree.GetRoot(context.CancellationToken);
            var expressionStatementNode = speculativeRoot as ExpressionStatementSyntax;
            var assignmentExpressionNode = expressionStatementNode?.Expression as AssignmentExpressionSyntax;
            var interpolatedStringExpressionNode = assignmentExpressionNode?.Right
                as InterpolatedStringExpressionSyntax;

            if (interpolatedStringExpressionNode == null)
            {
                return;
            }

            for (int i = 0; i < interpolatedStringExpressionNode.Contents.Count; i += 1)
            {
                if (interpolatedStringExpressionNode.Contents[i] is InterpolationSyntax interpolationNode
                    && IsInterpolationProbablyValid(context, speculativeModel, interpolationNode))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        InterpolatedStringsRequireLeadingDollarSignDescriptor,
                        node.GetLocation()));
                    return;
                }
            }
        }

        private static bool IsInterpolationProbablyValid(SyntaxNodeAnalysisContext context,
            SemanticModel speculativeModel, InterpolationSyntax interpolationNode)
        {
            if (interpolationNode.Expression is IdentifierNameSyntax identifierNode)
            {
                var identifier = speculativeModel
                    .GetSymbolInfo(identifierNode, context.CancellationToken).Symbol;

                return identifier != null;
            }
            else if (interpolationNode.Expression is MemberAccessExpressionSyntax memberAccessExpressionNode)
            {
                var identifier = speculativeModel
                    .GetSymbolInfo(memberAccessExpressionNode, context.CancellationToken).Symbol;

                return identifier != null;
            }
            else if (interpolationNode.Expression is InvocationExpressionSyntax invocationExpressionNode)
            {
                var identifier = speculativeModel
                    .GetSymbolInfo(invocationExpressionNode.Expression, context.CancellationToken).Symbol;
                if (identifier != null)
                {
                    return true;
                }

                // Some compile-time expressions like nameof expressions do not have a resolvable symbol, but do
                // have a constant value.
                return speculativeModel.GetConstantValue(invocationExpressionNode, context.CancellationToken)
                    .HasValue;
            }

            return false;
        }
    }
}
