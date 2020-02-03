using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability
{
    /// <summary>
    /// Analyzes the use of <c>dynamic</c> for potential reliability issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DynamicReliabilityAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor DynamicShouldNotBeUsedDescriptor
            = new DiagnosticDescriptor("CT2010", "Dynamic should not be used.", "Dynamic should not be used.",
                "CodeTiger.Reliability", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DynamicShouldNotBeUsedDescriptor);

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

            context.RegisterSyntaxNodeAction(AnalyzeDynamicReliability, SyntaxKind.IdentifierName);
        }

        private static void AnalyzeDynamicReliability(SyntaxNodeAnalysisContext context)
        {
            var identifierName = (IdentifierNameSyntax)context.Node;
            
            if (identifierName.Identifier.ValueText == "dynamic")
            {
                var identifierSymbol = context.SemanticModel
                    .GetSymbolInfo(identifierName, context.CancellationToken).Symbol;
                var typeSymbol = identifierSymbol as ITypeSymbol;

                if (typeSymbol?.TypeKind == TypeKind.Dynamic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DynamicShouldNotBeUsedDescriptor,
                        identifierName.Identifier.GetLocation()));
                }
            }
        }
    }
}