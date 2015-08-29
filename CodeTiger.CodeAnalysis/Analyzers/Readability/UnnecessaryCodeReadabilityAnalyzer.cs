using System.Collections.Immutable;
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

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(PositiveSignsShouldNotBeUsedDescriptor);
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
        }

        private void AnalyzeUnaryPlusUsage(SyntaxNodeAnalysisContext context)
        {
            var node = (PrefixUnaryExpressionSyntax)context.Node;

            context.ReportDiagnostic(Diagnostic.Create(PositiveSignsShouldNotBeUsedDescriptor,
                node.OperatorToken.GetLocation()));
        }
    }
}