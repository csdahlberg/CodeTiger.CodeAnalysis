using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeTiger.CodeAnalysis.Analyzers.Performance
{
    /// <summary>
    /// Analyzes LINQ queries and extension methods for performance issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LinqPerformanceAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor UnnecessaryWhereClausesShouldBeSimplifiedDescriptor
            = new DiagnosticDescriptor("CT1802", "Unnecessary where clauses should be simplified.",
                "Unnecessary where clauses should be simplified.", "CodeTiger.Performance",
                DiagnosticSeverity.Warning, true);

        private static readonly string[] _linqMethodsWithPredicates = new[]
        {
            nameof(Enumerable.Any), nameof(Enumerable.Count), nameof(Enumerable.First),
            nameof(Enumerable.FirstOrDefault), nameof(Enumerable.Last), nameof(Enumerable.LastOrDefault),
            nameof(Enumerable.LongCount), nameof(Enumerable.Single), nameof(Enumerable.SingleOrDefault)
        };

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(UnnecessaryWhereClausesShouldBeSimplifiedDescriptor);

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

            context.RegisterSemanticModelAction(AnalyzeForUnnecessaryWhereClauses);
        }

        private void AnalyzeForUnnecessaryWhereClauses(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            foreach (var invocationExpression in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocationExpression.ArgumentList?.Arguments.Count > 0)
                {
                    continue;
                }

                if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
                    || !_linqMethodsWithPredicates.Contains(memberAccessExpression.Name.Identifier.ValueText))
                {
                    continue;
                }

                if (!(memberAccessExpression.Expression is InvocationExpressionSyntax parentExpression)
                    || !(parentExpression.Expression is MemberAccessExpressionSyntax parentMemberAccessExpression))
                {
                    continue;
                }

                if (parentMemberAccessExpression.Name.Identifier.ValueText == nameof(Enumerable.Where))
                {
                    int spanStart = parentMemberAccessExpression.Name.SpanStart;
                    var textSpan = new TextSpan(spanStart, invocationExpression.Span.End - spanStart);
                    context.ReportDiagnostic(Diagnostic.Create(UnnecessaryWhereClausesShouldBeSimplifiedDescriptor,
                        Location.Create(context.SemanticModel.SyntaxTree, textSpan)));
                }
            }
        }
    }
}
