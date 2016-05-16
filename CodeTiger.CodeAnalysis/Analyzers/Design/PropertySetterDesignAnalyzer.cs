using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design
{
    /// <summary>
    /// Analyzes the design of property setters.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PropertySetterDesignAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor PropertySettersShouldNotModifyMultipleValuesDescriptor
            = new DiagnosticDescriptor("CT1006", "Property setters should not modify multiple values.",
                "Property setters should not modify multiple values.", "CodeTiger.Design",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(PropertySettersShouldNotModifyMultipleValuesDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzePropertySetter, SyntaxKind.SetAccessorDeclaration);
        }

        private void AnalyzePropertySetter(SyntaxNodeAnalysisContext context)
        {
            var setAccessorDeclaration = (AccessorDeclarationSyntax)context.Node;

            if (setAccessorDeclaration.Body == null)
            {
                return;
            }

            var assignedExpressions = new List<ExpressionSyntax>();
            var assignedSymbols = new List<ISymbol>();

            var assignments = setAccessorDeclaration.DescendantNodes(ShouldIncludeInAnalysis)
                .OfType<AssignmentExpressionSyntax>();
            foreach (var assignment in assignments)
            {
                assignedExpressions.Add(assignment.Left);

                var assignedSymbol = context.SemanticModel
                    .GetSymbolInfo(assignment.Left, context.CancellationToken)
                    .Symbol;
                if (assignedSymbol == null || assignedSymbol.Kind == SymbolKind.Local)
                {
                    continue;
                }

                if (!assignedSymbols.Contains(assignedSymbol))
                {
                    assignedSymbols.Add(assignedSymbol);
                }
            }

            if (assignedSymbols.Count > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertySettersShouldNotModifyMultipleValuesDescriptor,
                    assignedExpressions.First().GetLocation(),
                    assignedExpressions.Skip(1).Select(x => x.GetLocation()).ToList()));
            }
        }

        private bool ShouldIncludeInAnalysis(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ParenthesizedLambdaExpression:
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.AnonymousMethodExpression:
                case SyntaxKind.AnonymousObjectCreationExpression:
                case SyntaxKind.AnonymousObjectMemberDeclarator:
                    return false;
                default:
                    return true;
            }
        }
    }
}
