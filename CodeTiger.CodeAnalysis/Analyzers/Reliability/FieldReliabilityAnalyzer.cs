using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability
{
    /// <summary>
    /// Analyzes fields for potential reliability issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FieldReliabilityAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor MutableFieldsShouldBePrivateDescriptor
            = new DiagnosticDescriptor("CT2009", "Mutable fields should be private.",
                "Mutable fields should be private.", "CodeTiger.Reliability", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(MutableFieldsShouldBePrivateDescriptor);
            }
        }

        /// <summary>
        /// Registers actions in an analysis context.
        /// </summary>
        /// <param name="context">The context to register actions in.</param>
        /// <remarks>This method should only be called once, at the start of a session.</remarks>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeFieldAccessibility, SymbolKind.Field);
        }

        private void AnalyzeFieldAccessibility(SymbolAnalysisContext context)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            if (!fieldSymbol.IsReadOnly)
            {
                switch (context.Symbol.DeclaredAccessibility)
                {
                    case Accessibility.Internal:
                    case Accessibility.Protected:
                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.ProtectedOrInternal:
                    case Accessibility.Public:
                        context.ReportDiagnostic(Diagnostic.Create(MutableFieldsShouldBePrivateDescriptor,
                            context.Symbol.Locations.First(), context.Symbol.Locations.Skip(1)));
                        break;
                }
            }
        }
    }
}