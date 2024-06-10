using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Performance;

/// <summary>
/// Analyzes initialization of variables for performance issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InitializationPerformanceAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor
        DoubleInitializationShouldBeAvoidedDescriptor
        = new DiagnosticDescriptor(
            "CT1803", "Double initialization should be avoided", "Double initialization should be avoided",
            "CodeTiger.Performance", DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(DoubleInitializationShouldBeAvoidedDescriptor);

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

        context.RegisterSemanticModelAction(AnalyzeVariablesForDoubleInitialization);
    }

    private static void AnalyzeVariablesForDoubleInitialization(SemanticModelAnalysisContext context)
    {
        var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

        foreach (var variableDeclarator in root.DescendantNodes().OfType<VariableDeclaratorSyntax>()
            .Where(x => x.Initializer != null))
        {
            var variableSymbol = context.SemanticModel
                .GetDeclaredSymbol(variableDeclarator, context.CancellationToken);
            if (variableSymbol is null)
            {
                continue;
            }

            var containingNode = GetContainingCodeBlockNode(context, variableSymbol);
            if (containingNode != null)
            {
                // Get the statement that contains the declarator
                var statementContainingDeclarator = containingNode.ChildNodes()
                    .OfType<StatementSyntax>()
                    .Single(x => x.DescendantNodes().Contains(variableDeclarator));

                // Get the first statement after the declarator that include the variable
                StatementSyntax? statementIncludingVariable = null;
                foreach (var statementNode in containingNode.ChildNodes()
                    .OfType<StatementSyntax>()
                    .SkipWhile(x => x != statementContainingDeclarator)
                    .Skip(1))
                {
                    foreach (var identifierNode in statementNode.DescendantNodes()
                        .OfType<IdentifierNameSyntax>())
                    {
                        var identifierSymbolInfo = context.SemanticModel
                            .GetSymbolInfo(identifierNode, context.CancellationToken).Symbol;
                        if (SymbolEqualityComparer.Default.Equals(identifierSymbolInfo, variableSymbol))
                        {
                            statementIncludingVariable = statementNode;
                        }
                    }
                }

                if (statementIncludingVariable != null)
                {
                    var currentStatementDataFlowAnalysis = context.SemanticModel
                        .AnalyzeDataFlow(statementIncludingVariable);
                    
                    if (currentStatementDataFlowAnalysis?.AlwaysAssigned.Contains(variableSymbol) == true
                        && !currentStatementDataFlowAnalysis.ReadInside.Contains(variableSymbol))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DoubleInitializationShouldBeAvoidedDescriptor,
                            variableDeclarator.Initializer?.GetLocation()));
                    }
                }
            }
        }
    }

    private static SyntaxNode? GetContainingCodeBlockNode(SemanticModelAnalysisContext context,
        ISymbol? variableSymbol)
    {
        var containingNodes = variableSymbol?.ContainingSymbol?.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax(context.CancellationToken));
        if (containingNodes is null)
        {
            return null;
        }

        foreach (var containingNode in containingNodes)
        {
            if (containingNode == null)
            {
                continue;
            }

            switch (containingNode.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                    {
                        var baseMethodDeclarationNode = (BaseMethodDeclarationSyntax)containingNode;
                        if (baseMethodDeclarationNode.Body != null)
                        {
                            return baseMethodDeclarationNode.Body;
                        }

                        break;
                    }
                case SyntaxKind.SimpleLambdaExpression:
                    {
                        var simpleLambdaExpressionNode = (SimpleLambdaExpressionSyntax)containingNode;
                        if (simpleLambdaExpressionNode != null)
                        {
                            return simpleLambdaExpressionNode.Body;
                        }

                        break;
                    }
                case SyntaxKind.ParenthesizedLambdaExpression:
                    {
                        var parenthesizedLambdaExpressionNode
                            = (ParenthesizedLambdaExpressionSyntax)containingNode;
                        if (parenthesizedLambdaExpressionNode != null)
                        {
                            return parenthesizedLambdaExpressionNode.Body;
                        }

                        break;
                    }
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.UnknownAccessorDeclaration:
                    {
                        var accessorDeclarationNode = (AccessorDeclarationSyntax)containingNode;
                        if (accessorDeclarationNode != null)
                        {
                            return accessorDeclarationNode.Body;
                        }

                        break;
                    }
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                    return null;
            }
        }

        return null;
    }
}
