using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability;

/// <summary>
/// Analyzes assignments for potential reliability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssignmentReliabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor
        AssignmentResultsShouldNotBeUsedDescriptor = new DiagnosticDescriptor("CT2008",
            "The result of an assignment should not be used", "The result of an assignment should not be used",
            "CodeTiger.Reliability", DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(AssignmentResultsShouldNotBeUsedDescriptor);

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

        context.RegisterSemanticModelAction(AnalyzeAssignmentReliability);
    }

    private static void AnalyzeAssignmentReliability(SemanticModelAnalysisContext context)
    {
        var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

        foreach (var node in root.DescendantNodes())
        {
            AnalyzeNodeForAssignmentReliability(context, node);
        }
    }

    private static void AnalyzeNodeForAssignmentReliability(SemanticModelAnalysisContext context,
        SyntaxNode node)
    {
        switch (node.Kind())
        {
            case SyntaxKind.FieldDeclaration:
                AnalyzeNodeForAssignmentReliability(context, (FieldDeclarationSyntax)node);
                break;
            case SyntaxKind.VariableDeclarator:
                AnalyzeNodeForAssignmentReliability(context, (VariableDeclaratorSyntax)node);
                break;
            case SyntaxKind.IfStatement:
                AnalyzeNodeForAssignmentReliability(context, (IfStatementSyntax)node);
                break;
            case SyntaxKind.SwitchStatement:
                AnalyzeNodeForAssignmentReliability(context, (SwitchStatementSyntax)node);
                break;
            case SyntaxKind.AddAssignmentExpression:
            case SyntaxKind.AndAssignmentExpression:
            case SyntaxKind.DivideAssignmentExpression:
            case SyntaxKind.ExclusiveOrAssignmentExpression:
            case SyntaxKind.LeftShiftAssignmentExpression:
            case SyntaxKind.ModuloAssignmentExpression:
            case SyntaxKind.MultiplyAssignmentExpression:
            case SyntaxKind.OrAssignmentExpression:
            case SyntaxKind.RightShiftAssignmentExpression:
            case SyntaxKind.SimpleAssignmentExpression:
            case SyntaxKind.SubtractAssignmentExpression:
                AnalyzeNodeForAssignmentReliability(context, (AssignmentExpressionSyntax)node);
                break;
        }
    }

    private static void AnalyzeNodeForAssignmentReliability(SemanticModelAnalysisContext context,
        FieldDeclarationSyntax fieldDeclaration)
    {
        var variables = fieldDeclaration.Declaration?.Variables;
        if (variables != null)
        {
            foreach (var variable in variables)
            {
                AnalyzeNodeForAssignmentReliability(context, variable);
            }
        }
    }

    private static void AnalyzeNodeForAssignmentReliability(SemanticModelAnalysisContext context,
        VariableDeclaratorSyntax variableDeclarator)
    {
        var initializer = variableDeclarator.Initializer;
        if (initializer?.Value != null)
        {
            AnalyzeExpressionForAssignmentReliability(context, initializer.Value,
                initializer.EqualsToken.GetLocation());
        }
    }

    private static void AnalyzeNodeForAssignmentReliability(SemanticModelAnalysisContext context,
        IfStatementSyntax ifStatement)
    {
        AnalyzeExpressionForAssignmentReliability(context, ifStatement.Condition,
            ifStatement.Condition.GetLocation());
    }

    private static void AnalyzeNodeForAssignmentReliability(SemanticModelAnalysisContext context,
        SwitchStatementSyntax switchStatement)
    {
        AnalyzeExpressionForAssignmentReliability(context, switchStatement.Expression,
            switchStatement.Expression.GetLocation());
    }

    private static void AnalyzeNodeForAssignmentReliability(SemanticModelAnalysisContext context,
        AssignmentExpressionSyntax assignmentExpression)
    {
        AnalyzeExpressionForAssignmentReliability(context, assignmentExpression.Right,
            assignmentExpression.OperatorToken.GetLocation());
    }

    private static void AnalyzeExpressionForAssignmentReliability(SemanticModelAnalysisContext context,
        ExpressionSyntax expression, Location usageLocation)
    {
        switch (expression.Kind())
        {
            case SyntaxKind.AddAssignmentExpression:
            case SyntaxKind.AndAssignmentExpression:
            case SyntaxKind.DivideAssignmentExpression:
            case SyntaxKind.ExclusiveOrAssignmentExpression:
            case SyntaxKind.LeftShiftAssignmentExpression:
            case SyntaxKind.ModuloAssignmentExpression:
            case SyntaxKind.MultiplyAssignmentExpression:
            case SyntaxKind.OrAssignmentExpression:
            case SyntaxKind.RightShiftAssignmentExpression:
            case SyntaxKind.SimpleAssignmentExpression:
            case SyntaxKind.SubtractAssignmentExpression:
                context.ReportDiagnostic(Diagnostic.Create(AssignmentResultsShouldNotBeUsedDescriptor,
                    usageLocation));
                break;
            case SyntaxKind.ParenthesizedExpression:
                AnalyzeExpressionForAssignmentReliability(context,
                    ((ParenthesizedExpressionSyntax)expression).Expression, usageLocation);
                break;
        }
    }
}
