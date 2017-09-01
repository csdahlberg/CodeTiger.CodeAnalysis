using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout
{
    /// <summary>
    /// Analyzes attributes for layout issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeLayoutAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor AttributesShouldBeDeclaredSeparatelyDescriptor
            = new DiagnosticDescriptor("CT3526", "Attributes should be declared separately.",
                "Attributes should be declared separately.", "CodeTiger.Layout", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create<DiagnosticDescriptor>(AttributesShouldBeDeclaredSeparatelyDescriptor);
            }
        }

        /// <summary>
        /// Registers actions in an analysis context.
        /// </summary>
        /// <param name="context">The context to register actions in.</param>
        /// <remarks>This method should only be called once, at the start of a session.</remarks>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttributeList, SyntaxKind.AttributeList);
        }

        private void AnalyzeAttributeList(SyntaxNodeAnalysisContext context)
        {
            var node = (AttributeListSyntax)context.Node;

            if (node.Attributes.Count > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(AttributesShouldBeDeclaredSeparatelyDescriptor,
                    node.Attributes[0].GetLocation(), node.Attributes.Skip(1).Select(x => x.GetLocation())));
            }
        }
    }
}
