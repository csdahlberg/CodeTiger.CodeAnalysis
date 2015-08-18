using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

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
                var variableSymbol = context.SemanticModel
                    .GetDeclaredSymbol(variableDeclarator, context.CancellationToken);
                var containingNode = GetContainingCodeBlockNode(context, variableSymbol);
                if (containingNode != null)
                {
                    // Get the statement that contains the declarator
                    var statementContainingDeclarator = containingNode.ChildNodes()
                        .OfType<StatementSyntax>()
                        .Single(x => x.DescendantNodes().Contains(variableDeclarator));

                    // Get the first statement after the declarator that include the variable
                    var statementIncludingVariable = containingNode.ChildNodes()
                        .OfType<StatementSyntax>()
                        .SkipWhile(x => x != statementContainingDeclarator)
                        .Skip(1)
                        .Where(x => x.DescendantNodes()
                            .OfType<IdentifierNameSyntax>()
                            .Any(y => context.SemanticModel.GetSymbolInfo(y, context.CancellationToken)
                                .Symbol == variableSymbol))
                        .FirstOrDefault();

                    if (statementIncludingVariable != null)
                    {
                        var currentStatementDataFlowAnalysis = context.SemanticModel
                            .AnalyzeDataFlow(statementIncludingVariable);
                        if (currentStatementDataFlowAnalysis.WrittenInside.Contains(variableSymbol)
                            && !currentStatementDataFlowAnalysis.ReadInside.Contains(variableSymbol))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                DoubleInitializationShouldBeAvoidedDescriptor,
                                variableDeclarator.Initializer.GetLocation()));
                        }
                    }
                }
            }
        }

        private static SyntaxNode GetContainingCodeBlockNode(SemanticModelAnalysisContext context,
            ISymbol variableSymbol)
        {
            var containingNode = variableSymbol?.ContainingSymbol?.DeclaringSyntaxReferences
                .SingleOrDefault()?.GetSyntax(context.CancellationToken);
            if (containingNode == null)
            {
                return null;
            }

            switch (containingNode.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                    return ((BaseMethodDeclarationSyntax)containingNode).Body;
                case SyntaxKind.SimpleLambdaExpression:
                    return ((SimpleLambdaExpressionSyntax)containingNode).Body;
                case SyntaxKind.ParenthesizedLambdaExpression:
                    return ((ParenthesizedLambdaExpressionSyntax)containingNode).Body;
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.UnknownAccessorDeclaration:
                    return ((AccessorDeclarationSyntax)containingNode).Body;
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                    return null;
                default:
                    return containingNode;
            }
        }
    }
}