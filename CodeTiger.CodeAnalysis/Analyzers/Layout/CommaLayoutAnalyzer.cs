using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout;

/// <summary>
/// Analyzes commas for layout issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CommaLayoutAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor CommasShouldBeOnTheSameLineAsThePrecedingElementDescriptor
        = new DiagnosticDescriptor("CT3537", "Commas should be on the same line as the preceding element",
            "Commas should be on the same line as the preceding element", "CodeTiger.Layout",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(CommasShouldBeOnTheSameLineAsThePrecedingElementDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeParameterList, SyntaxKind.ParameterList);
        context.RegisterSyntaxNodeAction(AnalyzeArgumentList, SyntaxKind.ArgumentList);

        context.RegisterSyntaxNodeAction(AnalyzeTypeParameterList, SyntaxKind.TypeParameterList);
        context.RegisterSyntaxNodeAction(AnalyzeTypeParameterConstraintClause,
            SyntaxKind.TypeParameterConstraintClause);
        context.RegisterSyntaxNodeAction(AnalyzeTypeArgumentList, SyntaxKind.TypeArgumentList);

        context.RegisterSyntaxNodeAction(AnalyzeEnumDeclaration, SyntaxKind.EnumDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeInitializerExpression, SyntaxKind.ArrayInitializerExpression,
            SyntaxKind.CollectionInitializerExpression, SyntaxKind.ObjectInitializerExpression,
            SyntaxKind.ComplexElementInitializerExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAnonymousObjectCreationExpression,
            SyntaxKind.AnonymousObjectCreationExpression);
    }

    private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
    {
        var node = (ParameterListSyntax)context.Node;

        for (int i = 0; i < node.Parameters.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Parameters[i].GetLocation(), node.Parameters.GetSeparator(i));
        }
    }

    private static void AnalyzeArgumentList(SyntaxNodeAnalysisContext context)
    {
        var node = (ArgumentListSyntax)context.Node;

        for (int i = 0; i < node.Arguments.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Arguments[i].GetLocation(), node.Arguments.GetSeparator(i));
        }
    }

    private static void AnalyzeTypeParameterList(SyntaxNodeAnalysisContext context)
    {
        var node = (TypeParameterListSyntax)context.Node;

        for (int i = 0; i < node.Parameters.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Parameters[i].GetLocation(), node.Parameters.GetSeparator(i));
        }
    }

    private static void AnalyzeTypeParameterConstraintClause(SyntaxNodeAnalysisContext context)
    {
        var node = (TypeParameterConstraintClauseSyntax)context.Node;

        for (int i = 0; i < node.Constraints.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Constraints[i].GetLocation(), node.Constraints.GetSeparator(i));
        }
    }

    private static void AnalyzeTypeArgumentList(SyntaxNodeAnalysisContext context)
    {
        var node = (TypeArgumentListSyntax)context.Node;

        for (int i = 0; i < node.Arguments.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Arguments[i].GetLocation(), node.Arguments.GetSeparator(i));
        }
    }

    private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
    {
        var node = (EnumDeclarationSyntax)context.Node;

        for (int i = 0; i < node.Members.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Members[i].GetLocation(), node.Members.GetSeparator(i));
        }
    }

    private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var node = (VariableDeclarationSyntax)context.Node;

        for (int i = 0; i < node.Variables.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Variables[i].GetLocation(), node.Variables.GetSeparator(i));
        }
    }

    private static void AnalyzeInitializerExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (InitializerExpressionSyntax)context.Node;

        for (int i = 0; i < node.Expressions.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Expressions[i].GetLocation(), node.Expressions.GetSeparator(i));
        }
    }

    private static void AnalyzeAnonymousObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (AnonymousObjectCreationExpressionSyntax)context.Node;

        for (int i = 0; i < node.Initializers.SeparatorCount; i += 1)
        {
            AnalyzeComma(context, node.Initializers[i].GetLocation(), node.Initializers.GetSeparator(i));
        }
    }

    private static void AnalyzeComma(SyntaxNodeAnalysisContext context,
        Location precedingElementLocation, SyntaxToken commaToken)
    {
        if (commaToken.IsMissing)
        {
            return;
        }

        var commaLocation = commaToken.GetLocation();

        if (commaLocation.GetLineSpan().StartLinePosition.Line
            != precedingElementLocation.GetLineSpan().EndLinePosition.Line)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(CommasShouldBeOnTheSameLineAsThePrecedingElementDescriptor, commaLocation));
        }
    }
}
