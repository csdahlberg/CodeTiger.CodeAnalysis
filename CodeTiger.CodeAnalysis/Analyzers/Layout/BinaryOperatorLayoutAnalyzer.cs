using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout;

/// <summary>
/// Analyzes the use of binary operators for layout issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BinaryOperatorLayoutAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor
        MultiLineExpressionsShouldNotBeSplitAfterADotOrMemberAccessTokenDescriptor
        = new DiagnosticDescriptor("CT3530",
            "Multi-line expressions should not be split after a dot or member access token",
            "Multi-line expressions should not be split after a dot or member access token", "CodeTiger.Layout",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor
        MultiLineExpressionsShouldNotBeSplitAfterABinaryOperatorDescriptor = new DiagnosticDescriptor("CT3540",
            "Multi-line expressions should not be split after a binary operator",
            "Multi-line expressions should not be split after a binary operator", "CodeTiger.Layout",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(
                MultiLineExpressionsShouldNotBeSplitAfterADotOrMemberAccessTokenDescriptor,
                MultiLineExpressionsShouldNotBeSplitAfterABinaryOperatorDescriptor);
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

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccessExpression,
            SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.PointerMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeQualifiedNameSyntax, SyntaxKind.QualifiedName);
        
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression, SyntaxKind.MultiplyExpression, SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression, SyntaxKind.LessThanExpression, SyntaxKind.GreaterThanExpression,
            SyntaxKind.LessThanOrEqualExpression, SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.IsExpression, SyntaxKind.AsExpression, SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression, SyntaxKind.LeftShiftExpression, SyntaxKind.RightShiftExpression,
            SyntaxKind.BitwiseAndExpression, SyntaxKind.BitwiseOrExpression, SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression, SyntaxKind.ExclusiveOrExpression, SyntaxKind.CoalesceExpression);

        context.RegisterSyntaxNodeAction(AnalyzeConditionalExpression, SyntaxKind.ConditionalExpression);

        context.RegisterSyntaxNodeAction(AnalyzeAssignmentExpression, SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression, SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression, SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression, SyntaxKind.AndAssignmentExpression,
            SyntaxKind.OrAssignmentExpression, SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression, SyntaxKind.RightShiftAssignmentExpression);

        context.RegisterSyntaxNodeAction(AnalyzeLambdaExpression, SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.ParenthesizedLambdaExpression);
    }

    private static void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (MemberAccessExpressionSyntax)context.Node;

        if (IsLineSplitAfterToken(node.OperatorToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MultiLineExpressionsShouldNotBeSplitAfterADotOrMemberAccessTokenDescriptor,
                node.OperatorToken.GetLocation()));
        }
    }

    private static void AnalyzeQualifiedNameSyntax(SyntaxNodeAnalysisContext context)
    {
        var node = (QualifiedNameSyntax)context.Node;

        if (IsLineSplitAfterToken(node.DotToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MultiLineExpressionsShouldNotBeSplitAfterADotOrMemberAccessTokenDescriptor,
                node.DotToken.GetLocation()));
        }
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (BinaryExpressionSyntax)context.Node;

        if (IsLineSplitAfterToken(node.OperatorToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MultiLineExpressionsShouldNotBeSplitAfterABinaryOperatorDescriptor,
                node.OperatorToken.GetLocation()));
        }
    }

    private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (ConditionalExpressionSyntax)context.Node;

        if (IsLineSplitAfterToken(node.QuestionToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MultiLineExpressionsShouldNotBeSplitAfterABinaryOperatorDescriptor,
                node.QuestionToken.GetLocation()));
        }

        if (IsLineSplitAfterToken(node.ColonToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MultiLineExpressionsShouldNotBeSplitAfterABinaryOperatorDescriptor,
                node.ColonToken.GetLocation()));
        }
    }

    private static void AnalyzeAssignmentExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (AssignmentExpressionSyntax)context.Node;

        if (IsLineSplitAfterToken(node.OperatorToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MultiLineExpressionsShouldNotBeSplitAfterABinaryOperatorDescriptor,
                node.OperatorToken.GetLocation()));
        }
    }

    private static void AnalyzeLambdaExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (LambdaExpressionSyntax)context.Node;

        if (IsLineSplitAfterToken(node.ArrowToken) && !IsTokenFollowedByOpenBrace(node.ArrowToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MultiLineExpressionsShouldNotBeSplitAfterABinaryOperatorDescriptor,
                node.ArrowToken.GetLocation()));
        }
    }

    private static bool IsLineSplitAfterToken(SyntaxToken token)
    {
        var tokenLineSpan = token.GetLocation().GetLineSpan();

        var nextToken = token.GetNextToken();
        while (nextToken != default(SyntaxToken))
        {
            var nextTokenLineSpan = nextToken.GetLocation().GetLineSpan();
            if (nextTokenLineSpan.StartLinePosition.Line != tokenLineSpan.EndLinePosition.Line)
            {
                break;
            }

            if (!SyntaxFacts.IsTrivia(nextToken.Kind()))
            {
                return false;
            }

            nextToken = nextToken.GetNextToken();
        }

        return true;
    }

    private static bool IsTokenFollowedByOpenBrace(SyntaxToken token)
    {
        var nextToken = token.GetNextToken();
        while (nextToken != default(SyntaxToken))
        {
            if (nextToken.Kind() == SyntaxKind.OpenBraceToken)
            {
                return true;
            }

            if (!SyntaxFacts.IsTrivia(nextToken.Kind()))
            {
                return nextToken.Kind() == SyntaxKind.OpenBraceToken;
            }

            nextToken = nextToken.GetNextToken();
        }

        return true;
    }
}
