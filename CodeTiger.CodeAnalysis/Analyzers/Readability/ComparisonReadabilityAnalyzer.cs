using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Readability;

/// <summary>
/// Analyzes comparisons for readability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ComparisonReadabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor
        LiteralsShouldNotBeOnTheLeftSideOfComparisonOperatorsDescriptor = new DiagnosticDescriptor("CT3113",
            "Literals should not be on the left side of comparison operators.",
            "Literals should not be on the left side of comparison operators.", "CodeTiger.Readability",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor
        ConstantsShouldNotBeOnTheLeftSideOfComparisonOperatorsDescriptor = new DiagnosticDescriptor("CT3114",
            "Constants should not be on the left side of comparison operators.",
            "Constants should not be on the left side of comparison operators.", "CodeTiger.Readability",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(
                LiteralsShouldNotBeOnTheLeftSideOfComparisonOperatorsDescriptor,
                ConstantsShouldNotBeOnTheLeftSideOfComparisonOperatorsDescriptor);
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

        context.RegisterSyntaxNodeAction(AnalyzeComparison, SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression, SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression, SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression);
    }

    private static void AnalyzeComparison(SyntaxNodeAnalysisContext context)
    {
        var node = (BinaryExpressionSyntax)context.Node;

        if (IsLiteral(node.Left))
        {
            if (!IsLiteral(node.Right))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    LiteralsShouldNotBeOnTheLeftSideOfComparisonOperatorsDescriptor,
                    node.Left.GetLocation()));
            }
        }
        else if (context.SemanticModel.GetConstantValue(node.Left, context.CancellationToken).HasValue)
        {
            if (!IsLiteral(node.Right)
                && !context.SemanticModel.GetConstantValue(node.Right, context.CancellationToken).HasValue)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ConstantsShouldNotBeOnTheLeftSideOfComparisonOperatorsDescriptor,
                    node.Left.GetLocation()));
            }
        }
    }

    private static bool IsLiteral(ExpressionSyntax node)
    {
        switch (node.Kind())
        {
            case SyntaxKind.CharacterLiteralExpression:
            case SyntaxKind.FalseLiteralExpression:
            case SyntaxKind.NullLiteralExpression:
            case SyntaxKind.NumericLiteralExpression:
            case SyntaxKind.StringLiteralExpression:
            case SyntaxKind.TrueLiteralExpression:
                return true;
            default:
                return false;
        }
    }
}
