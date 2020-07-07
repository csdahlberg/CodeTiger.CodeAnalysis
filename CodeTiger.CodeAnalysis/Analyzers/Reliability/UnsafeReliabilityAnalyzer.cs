using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability
{
    /// <summary>
    /// Analyzes unsafe code for potential reliability issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnsafeReliabilityAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor UnsafeCodeShouldNotBeUsedDescriptor
            = new DiagnosticDescriptor("CT2004", "Unsafe code should not be used.",
                "Unsafe code should not be used.", "CodeTiger.Reliability", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(UnsafeCodeShouldNotBeUsedDescriptor);

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

            context.RegisterSyntaxTreeAction(AnalyzeUnsafeReliability);
        }

        private static void AnalyzeUnsafeReliability(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);

            foreach (var unsafeToken in root.DescendantTokens().Where(x => x.Kind() == SyntaxKind.UnsafeKeyword))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnsafeCodeShouldNotBeUsedDescriptor,
                    unsafeToken.GetLocation()));
            }
        }
    }
}
