using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Performance
{
    /// <summary>
    /// Analyzes initialization of variables for performance issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InitializationPerformanceAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor
            DoubleInitializationShouldBeAvoidedDescriptor
            = new DiagnosticDescriptor(
                "CT1803", "Double initialization should be avoided.", "Double initialization should be avoided.",
                "CodeTiger.Performance", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(DoubleInitializationShouldBeAvoidedDescriptor);
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

            context.RegisterSemanticModelAction(AnalyzeVariablesForDoubleInitialization);
        }

        private void AnalyzeVariablesForDoubleInitialization(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            foreach (var variableDeclarator in root.DescendantNodes().OfType<VariableDeclaratorSyntax>()
                .Where(x => x.Initializer != null))
            {
                var variableSymbol = context.SemanticModel.GetSymbolInfo(variableDeclarator).Symbol;
                var containingCodeElement = GetContainingCodeElement(variableDeclarator);
                
                var dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(
                    containingCodeElement.DescendantNodes().SkipWhile(x => x != variableDeclarator).Skip(1).OfType<StatementSyntax>().First(),
                    containingCodeElement.DescendantNodes().OfType<StatementSyntax>().Last());

                // If the variable is not always assigned after the declaration, skip it to be safe.
                if (!dataFlowAnalysis.AlwaysAssigned.Any(x => x == variableSymbol))
                {
                    continue;
                }


                foreach (var assignment in containingCodeElement.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                    .Where(x => x.Left.Kind() == SyntaxKind.IdentifierName
                        && context.SemanticModel.GetSymbolInfo(x.Left).Symbol == variableSymbol))
                {
                    if (assignment.Right.DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Any(x => context.SemanticModel.GetSymbolInfo(x).Symbol == variableSymbol))
                    {
                        break;
                    }

                    
                }
            }
        }

        private static SyntaxNode GetContainingCodeElement(SyntaxNode node)
        {
            var parent = node.Parent;

            while (parent != null)
            {
                switch (parent.Kind())
                {
                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKind.ConstructorDeclaration:
                    case SyntaxKind.GetAccessorDeclaration:
                    case SyntaxKind.SetAccessorDeclaration:
                    case SyntaxKind.AddAccessorDeclaration:
                    case SyntaxKind.RemoveAccessorDeclaration:
                        return parent;
                }

                parent = parent.Parent;
            }

            return null;
        }
    }
}