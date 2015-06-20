using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Usage
{
    /// <summary>
    /// Analyzes usage of the <see cref="Debug"/> class.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DebugUsageAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor CallsToDebugAssertShouldIncludeAMessageDescriptor
            = new DiagnosticDescriptor("CT2202", "Calls to the Debug.Assert method should include a message.",
                "Calls to the Debug.Assert method should include a message.", "CodeTiger.Usage",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(CallsToDebugAssertShouldIncludeAMessageDescriptor);
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

            context.RegisterSemanticModelAction(AnalyzeDebugUsage);
        }

        private void AnalyzeDebugUsage(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);
            var debugType = context.SemanticModel.Compilation
                .GetTypeByMetadataName("System.Diagnostics.Debug");

            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations
                .Where(x => x.Expression.Kind() == SyntaxKind.SimpleMemberAccessExpression))
            {
                var memberAccessExpression = (MemberAccessExpressionSyntax)invocation.Expression;

                var targetType = context.SemanticModel.GetTypeInfo(memberAccessExpression.Expression);
                if (targetType.Type == debugType && memberAccessExpression?.Name?.Identifier.Text == "Assert")
                {
                    // NOTE: This assumes that the message is always the second argument.
                    if (invocation.ArgumentList.Arguments.Count < 2)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            CallsToDebugAssertShouldIncludeAMessageDescriptor, invocation.GetLocation()));
                    }
                }
            }
        }
    }
}