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
        internal static readonly DiagnosticDescriptor AttributesShouldBeDeclaredOnSeparateLinesDescriptor
            = new DiagnosticDescriptor("CT3527", "Attributes should be declared on separate lines.",
                "Attributes should be declared on separate lines.", "CodeTiger.Layout", DiagnosticSeverity.Warning,
                true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(AttributesShouldBeDeclaredSeparatelyDescriptor,
                    AttributesShouldBeDeclaredOnSeparateLinesDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeAttributeList, SyntaxKind.AttributeList);
        }

        private static void AnalyzeAttributeList(SyntaxNodeAnalysisContext context)
        {
            var node = (AttributeListSyntax)context.Node;

            // Allow parameters to have multiple attributes on the same line or in the same attribute list.
            if (node.Parent?.Kind() == SyntaxKind.Parameter)
            {
                return;
            }

            if (node.Attributes.Count > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(AttributesShouldBeDeclaredSeparatelyDescriptor,
                    node.Attributes[0].GetLocation(), node.Attributes.Skip(1).Select(x => x.GetLocation())));
            }

            var attributeListsForSameParent = node.Parent.ChildNodes().OfType<AttributeListSyntax>().ToList();
            if (attributeListsForSameParent.Count > 1)
            {
                int nodeStartLine = node.GetLocation().GetLineSpan().StartLinePosition.Line;
                if (attributeListsForSameParent.Any(x => x.SpanStart < node.SpanStart
                    && nodeStartLine == x.GetLocation().GetLineSpan().StartLinePosition.Line))
                {
                    context.ReportDiagnostic(Diagnostic.Create(AttributesShouldBeDeclaredOnSeparateLinesDescriptor,
                        node.OpenBracketToken.GetLocation()));
                }
            }
        }
    }
}
