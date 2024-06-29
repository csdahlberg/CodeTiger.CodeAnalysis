using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout;

/// <summary>
/// Analyzes braces for layout issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BraceLayoutAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor BracesForMultiLineElementsShouldBeOnANewLineDescriptor
        = new DiagnosticDescriptor("CT3501", "Braces for multi-line elements should be on a new line",
            "Braces for multi-line elements should be on a new line", "CodeTiger.Layout",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor BracesShouldNotBeOmittedFromCodeBlocksDescriptor
        = new DiagnosticDescriptor("CT3525", "Braces should not be omitted from code blocks",
            "Braces should not be omitted from code blocks", "CodeTiger.Layout", DiagnosticSeverity.Warning,
            true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(BracesForMultiLineElementsShouldBeOnANewLineDescriptor,
                BracesShouldNotBeOmittedFromCodeBlocksDescriptor);
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

        context.RegisterSyntaxNodeAction(AnalyzeNamespaceDeclaration,
            SyntaxKind.NamespaceDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeComplexElementInitializerExpression,
            SyntaxKind.ComplexElementInitializerExpression);
        context.RegisterSyntaxNodeAction(AnalyzeArrayCreationExpression,
            SyntaxKind.ArrayCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreationExpression,
            SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAnonymousObjectCreationExpression,
            SyntaxKind.AnonymousObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
        context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeAccessorList, SyntaxKind.AccessorList);
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        context.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);
        context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
        context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
        context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
        context.RegisterSyntaxNodeAction(AnalyzeDoStatement, SyntaxKind.DoStatement);
        context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
    }

    private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var node = (NamespaceDeclarationSyntax)context.Node;

        var nodeLineSpan = node.GetLocation().GetLineSpan();
        if (nodeLineSpan.StartLinePosition.Line != nodeLineSpan.EndLinePosition.Line)
        {
            AnalyzeBraces(nodeLineSpan, node.OpenBraceToken, node.CloseBraceToken, context);
        }
    }

    private static void AnalyzeArrayCreationExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (ArrayCreationExpressionSyntax)context.Node;

        if (node.Initializer != null)
        {
            AnalyzeBraces(node.GetLocation().GetLineSpan(), node.Initializer.OpenBraceToken,
                node.Initializer.CloseBraceToken, context);
        }
    }

    private static void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (ObjectCreationExpressionSyntax)context.Node;

        if (node.Initializer != null)
        {
            AnalyzeBraces(node.GetLocation().GetLineSpan(), node.Initializer.OpenBraceToken,
                node.Initializer.CloseBraceToken, context,
                isForArgument: node.Parent.IsKind(SyntaxKind.Argument));
        }
    }

    private static void AnalyzeComplexElementInitializerExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (InitializerExpressionSyntax)context.Node;

        AnalyzeBraces(node.GetLocation().GetLineSpan(), node.OpenBraceToken, node.CloseBraceToken, context);
    }

    private static void AnalyzeAnonymousObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
        var node = (AnonymousObjectCreationExpressionSyntax)context.Node;

        AnalyzeBraces(node.GetLocation().GetLineSpan(), node.OpenBraceToken, node.CloseBraceToken, context);
    }

    private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
    {
        var node = (BlockSyntax)context.Node;

        FileLinePositionSpan nodeLinePositionSpan;
        switch (node.Parent?.Kind())
        {
            case SyntaxKind.IfStatement:
            case SyntaxKind.ElseClause:
                nodeLinePositionSpan = node.Parent.GetLocation().GetLineSpan();
                break;
            case SyntaxKind.AddAccessorDeclaration:
            case SyntaxKind.RemoveAccessorDeclaration:
            case SyntaxKind.GetAccessorDeclaration:
            case SyntaxKind.SetAccessorDeclaration:
                {
                    var accessorDeclarationNode = (AccessorDeclarationSyntax)node.Parent;
                    var accessorDeclarationStartSpan = accessorDeclarationNode.Modifiers.Count > 0
                        ? accessorDeclarationNode.Modifiers.Span
                        : accessorDeclarationNode.Keyword.Span;
                    var spanWithoutAttributes = TextSpan.FromBounds(accessorDeclarationStartSpan.Start,
                        accessorDeclarationNode.Span.End);
                    nodeLinePositionSpan = Location.Create(node.SyntaxTree, spanWithoutAttributes)
                        .GetLineSpan();
                    break;
                }
            default:
                nodeLinePositionSpan = node.GetLocation().GetLineSpan();
                break;
        }

        bool isForDoStatement = node.Parent.IsKind(SyntaxKind.DoStatement);
        bool isForArgument = node.Parent is ArgumentSyntax || node.Parent?.Parent is ArgumentSyntax;

        AnalyzeBraces(nodeLinePositionSpan, node.OpenBraceToken, node.CloseBraceToken, context,
            isForDoStatement, isForArgument);
    }

    private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
    {
        var node = (SwitchStatementSyntax)context.Node;

        AnalyzeBraces(node.GetLocation().GetLineSpan(), node.OpenBraceToken, node.CloseBraceToken, context);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        var node = (BaseTypeDeclarationSyntax)context.Node;

        var nodeLineSpan = node.GetLocation().GetLineSpan();
        if (nodeLineSpan.StartLinePosition.Line != nodeLineSpan.EndLinePosition.Line)
        {
            AnalyzeBraces(nodeLineSpan, node.OpenBraceToken, node.CloseBraceToken, context);
        }
    }

    private static void AnalyzeAccessorList(SyntaxNodeAnalysisContext context)
    {
        var node = (AccessorListSyntax)context.Node;

        AnalyzeBraces(node.GetLocation().GetLineSpan(), node.OpenBraceToken, node.CloseBraceToken, context);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var node = (IfStatementSyntax)context.Node;

        if (node.Statement != null && node.Statement.Kind() != SyntaxKind.Block)
        {
            context.ReportDiagnostic(Diagnostic.Create(BracesShouldNotBeOmittedFromCodeBlocksDescriptor,
                node.IfKeyword.GetLocation()));
        }
    }

    private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
    {
        var node = (ElseClauseSyntax)context.Node;

        if (node.Statement != null
            && node.Statement.Kind() != SyntaxKind.Block
            && node.Statement.Kind() != SyntaxKind.IfStatement)
        {
            context.ReportDiagnostic(Diagnostic.Create(BracesShouldNotBeOmittedFromCodeBlocksDescriptor,
                node.ElseKeyword.GetLocation()));
        }
    }

    private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
    {
        var node = (ForStatementSyntax)context.Node;

        if (node.Statement != null && node.Statement.Kind() != SyntaxKind.Block)
        {
            context.ReportDiagnostic(Diagnostic.Create(BracesShouldNotBeOmittedFromCodeBlocksDescriptor,
                node.ForKeyword.GetLocation()));
        }
    }

    private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
    {
        var node = (ForEachStatementSyntax)context.Node;

        if (node.Statement != null && node.Statement.Kind() != SyntaxKind.Block)
        {
            context.ReportDiagnostic(Diagnostic.Create(BracesShouldNotBeOmittedFromCodeBlocksDescriptor,
                node.ForEachKeyword.GetLocation()));
        }
    }

    private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
    {
        var node = (WhileStatementSyntax)context.Node;

        if (node.Statement != null && node.Statement.Kind() != SyntaxKind.Block)
        {
            context.ReportDiagnostic(Diagnostic.Create(BracesShouldNotBeOmittedFromCodeBlocksDescriptor,
                node.WhileKeyword.GetLocation()));
        }
    }

    private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
    {
        var node = (DoStatementSyntax)context.Node;

        if (node.Statement != null && node.Statement.Kind() != SyntaxKind.Block)
        {
            context.ReportDiagnostic(Diagnostic.Create(BracesShouldNotBeOmittedFromCodeBlocksDescriptor,
                node.DoKeyword.GetLocation()));
        }
    }

    private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
    {
        var node = (UsingStatementSyntax)context.Node;

        if (node.Statement == null)
        {
            return;
        }

        if (node.Statement.Kind() != SyntaxKind.Block && node.Statement.Kind() != SyntaxKind.UsingStatement)
        {
            context.ReportDiagnostic(Diagnostic.Create(BracesShouldNotBeOmittedFromCodeBlocksDescriptor,
                node.UsingKeyword.GetLocation()));
        }
    }

    private static void AnalyzeBraces(FileLinePositionSpan nodeLineSpan, SyntaxToken openBraceToken,
        SyntaxToken closeBraceToken, SyntaxNodeAnalysisContext context, bool isForDoStatement = false,
        bool isForArgument = false)
    {
        if (nodeLineSpan.StartLinePosition.Line == nodeLineSpan.EndLinePosition.Line)
        {
            return;
        }

        if (!IsOnOwnLine(openBraceToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(BracesForMultiLineElementsShouldBeOnANewLineDescriptor,
                openBraceToken.GetLocation()));
        }

        if (isForDoStatement || isForArgument)
        {
            if (!IsFirstTokenOnLine(closeBraceToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    BracesForMultiLineElementsShouldBeOnANewLineDescriptor, closeBraceToken.GetLocation()));
            }
        }
        else
        {
            if (!IsFirstTokenOnLine(closeBraceToken)
                || (!isForArgument && !IsLastTokenOnLine(closeBraceToken)))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    BracesForMultiLineElementsShouldBeOnANewLineDescriptor, closeBraceToken.GetLocation()));
            }
        }
    }

    private static bool IsOnOwnLine(SyntaxToken braceToken)
    {
        return IsFirstTokenOnLine(braceToken) && IsLastTokenOnLine(braceToken);
    }

    private static bool IsFirstTokenOnLine(SyntaxToken braceToken)
    {
        var braceTokenLineSpan = braceToken.GetLocation().GetLineSpan();

        var previousToken = braceToken.GetPreviousToken();
        while (previousToken != default(SyntaxToken))
        {
            var previousTokenLineSpan = previousToken.GetLocation().GetLineSpan();
            if (previousTokenLineSpan.EndLinePosition.Line != braceTokenLineSpan.StartLinePosition.Line)
            {
                return true;
            }

            if (!SyntaxFacts.IsTrivia(previousToken.Kind())
                && !previousToken.IsKind(SyntaxKind.SemicolonToken)
                && !previousToken.IsKind(SyntaxKind.ColonToken))
            {
                return false;
            }

            previousToken = previousToken.GetPreviousToken();
        }

        return true;
    }

    private static bool IsLastTokenOnLine(SyntaxToken braceToken)
    {
        var braceTokenLineSpan = braceToken.GetLocation().GetLineSpan();

        var nextToken = braceToken.GetNextToken();
        while (nextToken != default(SyntaxToken))
        {
            var nextTokenLineSpan = nextToken.GetLocation().GetLineSpan();
            if (nextTokenLineSpan.StartLinePosition.Line != braceTokenLineSpan.EndLinePosition.Line)
            {
                return true;
            }

            if (!SyntaxFacts.IsTrivia(nextToken.Kind()) && !nextToken.IsKind(SyntaxKind.SemicolonToken)
                && !nextToken.IsKind(SyntaxKind.CommaToken) && !nextToken.IsKind(SyntaxKind.CloseParenToken))
            {
                return false;
            }

            nextToken = nextToken.GetNextToken();
        }

        return true;
    }
}
