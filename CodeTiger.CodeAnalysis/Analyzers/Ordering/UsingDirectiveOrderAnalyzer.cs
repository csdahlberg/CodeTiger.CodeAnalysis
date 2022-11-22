using System;
using System.Collections.Immutable;
using System.Linq;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeTiger.CodeAnalysis.Analyzers.Ordering;

/// <summary>
/// Analyzes the order of using directives in source code.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UsingDirectiveOrderAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor UsingDirectivesShouldBeBeforeNamespaceDeclarationsDescriptor
        = new DiagnosticDescriptor("CT3210", "Using directives should be before namespace declarations",
            "Using directives should be before namespace declarations", "CodeTiger.Ordering",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor
        UsingNamespaceDirectivesForSystemNamespacesShouldBeBeforeOtherNamespacesDescriptor
        = new DiagnosticDescriptor("CT3211",
            "Using directives for System namespaces should be before other namespaces",
            "Using directives for System namespaces should be before other namespaces", "CodeTiger.Ordering",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor
        UsingNamespaceDirectivesShouldBeBeforeUsingAliasDirectivesDescriptor = new DiagnosticDescriptor(
            "CT3212", "Using namespace directives should be before using alias directives",
            "Using namespace directives should be before using alias directives", "CodeTiger.Ordering",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor
        UsingDirectivesShouldBeOrderedAlphabeticallyWithinGroupsDescriptor = new DiagnosticDescriptor("CT3213",
            "Using directives should be ordered alphabetically",
            "Using directives should be ordered alphabetically", "CodeTiger.Ordering",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor UsingDirectivesShouldNotBeSeparatedByAnyLinesDescriptor
        = new DiagnosticDescriptor("CT3214", "Using directives should not be separated by any lines",
            "Using directives should not be separated by any lines", "CodeTiger.Ordering",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor
        NonStaticUsingDirectivesShouldBeBeforeStaticUsingDirectivesDescriptor = new DiagnosticDescriptor(
            "CT3218", "Non-static using directives should be before static using directives",
            "Non-static using directives should be before static using directives", "CodeTiger.Ordering",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(UsingDirectivesShouldBeBeforeNamespaceDeclarationsDescriptor,
                UsingNamespaceDirectivesForSystemNamespacesShouldBeBeforeOtherNamespacesDescriptor,
                UsingNamespaceDirectivesShouldBeBeforeUsingAliasDirectivesDescriptor,
                UsingDirectivesShouldBeOrderedAlphabeticallyWithinGroupsDescriptor,
                UsingDirectivesShouldNotBeSeparatedByAnyLinesDescriptor,
                NonStaticUsingDirectivesShouldBeBeforeStaticUsingDirectivesDescriptor);
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

        context.RegisterSyntaxTreeAction(AnalyzeUsingDirectiveLocation);
        context.RegisterSyntaxTreeAction(AnalyzeUsingDirectiveOrder);
    }

    private static void AnalyzeUsingDirectiveLocation(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        bool wasNamespaceEncountered = false;

        foreach (var node in root.ChildNodes())
        {
            switch (node.Kind())
            {
                case SyntaxKind.UsingDirective:
                    if (wasNamespaceEncountered)
                    {
                        // The using directive appears after a namespace declaration
                        context.ReportDiagnostic(Diagnostic.Create(
                            UsingDirectivesShouldBeBeforeNamespaceDeclarationsDescriptor, node.GetLocation()));
                    }
                    break;
                case SyntaxKind.NamespaceDeclaration:
                    foreach (var nestedUsingDirective in node.DescendantNodes().OfType<UsingDirectiveSyntax>())
                    {
                        // The using directive appears within a namespace declaration
                        context.ReportDiagnostic(Diagnostic.Create(
                            UsingDirectivesShouldBeBeforeNamespaceDeclarationsDescriptor,
                            nestedUsingDirective.GetLocation()));
                    }
                    wasNamespaceEncountered = true;
                    break;
            }
        }
    }

    private static void AnalyzeUsingDirectiveOrder(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        // Analyze using directives outside of any namespace declarations
        AnalyzeUsingDirectiveOrder(root, context);

        // Analyze using directives within namespace declarations
        foreach (var namespaceDeclaration in root.DescendantNodes().OfType<NamespaceDeclarationSyntax>())
        {
            AnalyzeUsingDirectiveOrder(namespaceDeclaration, context);
        }
    }

    private static void AnalyzeUsingDirectiveOrder(SyntaxNode root, SyntaxTreeAnalysisContext context)
    {
        bool wasNonSystemNamespaceEncountered = false;
        bool wasAliasDirectiveEncountered = false;
        bool wasStaticUsingDirectiveEncountered = false;

        UsingDirectiveSyntax previousUsingDirectiveNode = null;

        foreach (var node in root.ChildNodes().OfType<UsingDirectiveSyntax>())
        {
            if (previousUsingDirectiveNode != null)
            {
                AnalyzeUsingDirectiveForOrder(context, wasNonSystemNamespaceEncountered,
                    wasAliasDirectiveEncountered, wasStaticUsingDirectiveEncountered,
                    previousUsingDirectiveNode, node);
            }

            if (node.Alias != null)
            {
                wasAliasDirectiveEncountered = true;
            }
            else if (node.IsStaticUsingDirective())
            {
                wasStaticUsingDirectiveEncountered = true;
            }
            else if (node.Alias == null)
            {
                if (!StartsWithSystemNamespace(node.Name))
                {
                    wasNonSystemNamespaceEncountered = true;
                }
            }

            previousUsingDirectiveNode = node;
        }
    }

    private static void AnalyzeUsingDirectiveForOrder(SyntaxTreeAnalysisContext context,
        bool wasNonSystemNamespaceEncountered, bool wasAliasDirectiveEncountered,
        bool wasStaticUsingDirectiveEncountered, UsingDirectiveSyntax previousNode, UsingDirectiveSyntax node)
    {
        // Check for using directives separated by any lines
        if (GetLocationForSorting(node).GetLineSpan().StartLinePosition.Line
            > GetLocationForSorting(previousNode).GetLineSpan().EndLinePosition.Line + 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                UsingDirectivesShouldNotBeSeparatedByAnyLinesDescriptor, node.GetLocation()));
        }

        if (node.Alias == null && !node.IsStaticUsingDirective())
        {
            if (StartsWithSystemNamespace(node.Name))
            {
                // Check for using namespace directives for System.* after other namespaces
                if (wasNonSystemNamespaceEncountered)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        UsingNamespaceDirectivesForSystemNamespacesShouldBeBeforeOtherNamespacesDescriptor,
                        node.GetLocation()));
                }
            }

            // Check for using namespace directives after using alias directives
            if (wasAliasDirectiveEncountered)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    UsingNamespaceDirectivesShouldBeBeforeUsingAliasDirectivesDescriptor,
                    node.GetLocation()));
            }
        }

        // Check for non-static using directives after static using directives
        if (!node.IsStaticUsingDirective() && wasStaticUsingDirectiveEncountered)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                NonStaticUsingDirectivesShouldBeBeforeStaticUsingDirectivesDescriptor,
                node.GetLocation()));
        }

        AnalyzeUsingDirectiveForAlphabeticalOrder(context, node, previousNode);
    }

    private static void AnalyzeUsingDirectiveForAlphabeticalOrder(SyntaxTreeAnalysisContext context,
        UsingDirectiveSyntax node, UsingDirectiveSyntax previousNode)
    {
        if (node.Alias != null)
        {
            if (previousNode.Alias != null
                && CompareCaseInsensitive(previousNode.Alias.Name.ToString(), node.Alias.Name.ToString()) > 0)
            {
                // Both node and previousNode are aliases, and node should be before previousNode.
                context.ReportDiagnostic(Diagnostic.Create(
                    UsingDirectivesShouldBeOrderedAlphabeticallyWithinGroupsDescriptor, node.GetLocation()));
            }
        }
        else if (node.IsStaticUsingDirective() == previousNode.IsStaticUsingDirective()
            && CompareCaseInsensitive(GetTextForSorting(previousNode), GetTextForSorting(node)) > 0)
        {
            if (StartsWithSystemNamespace(node.Name))
            {
                // Both node and previousNode are for System namespaces, and node should be before
                // previousNode.
                context.ReportDiagnostic(Diagnostic.Create(
                    UsingDirectivesShouldBeOrderedAlphabeticallyWithinGroupsDescriptor, node.GetLocation()));
            }
            else if (!StartsWithSystemNamespace(previousNode.Name))
            {
                // Both node and previousNode are for non-System namespaces, and node should be
                // before previousNode.
                context.ReportDiagnostic(Diagnostic.Create(
                    UsingDirectivesShouldBeOrderedAlphabeticallyWithinGroupsDescriptor, node.GetLocation()));
            }
        }

        string GetTextForSorting(UsingDirectiveSyntax usingDirectiveNode)
            => usingDirectiveNode?.Alias?.ToString() + usingDirectiveNode.Name.ToString();
        int CompareCaseInsensitive(string first, string second)
            => string.Compare(first, second, StringComparison.OrdinalIgnoreCase);
    }

    private static Location GetLocationForSorting(UsingDirectiveSyntax node)
    {
        if (!node.HasLeadingTrivia)
        {
            return node.GetLocation();
        }

        var spanToUseForStart = node.Span;

        foreach (var leadingTrivia in node.GetLeadingTrivia().Reverse())
        {
            switch (leadingTrivia.Kind())
            {
                case SyntaxKind.BadDirectiveTrivia:
                case SyntaxKind.DefineDirectiveTrivia:
                case SyntaxKind.DisabledTextTrivia:
                case SyntaxKind.DocumentationCommentExteriorTrivia:
                case SyntaxKind.ElifDirectiveTrivia:
                case SyntaxKind.ElseDirectiveTrivia:
                case SyntaxKind.EndIfDirectiveTrivia:
                case SyntaxKind.EndRegionDirectiveTrivia:
                case SyntaxKind.ErrorDirectiveTrivia:
                case SyntaxKind.IfDirectiveTrivia:
                case SyntaxKind.LineDirectiveTrivia:
                case SyntaxKind.LoadDirectiveTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                case SyntaxKind.PragmaChecksumDirectiveTrivia:
                case SyntaxKind.PragmaWarningDirectiveTrivia:
                case SyntaxKind.PreprocessingMessageTrivia:
                case SyntaxKind.ReferenceDirectiveTrivia:
                case SyntaxKind.RegionDirectiveTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.SkippedTokensTrivia:
                case SyntaxKind.UndefDirectiveTrivia:
                case SyntaxKind.WarningDirectiveTrivia:
                case SyntaxKind.WhitespaceTrivia:
                    spanToUseForStart = leadingTrivia.Span;
                    break;
                case SyntaxKind.EndOfLineTrivia:
                default:
                    return node.SyntaxTree.GetLocation(
                        TextSpan.FromBounds(spanToUseForStart.Start, node.Span.End));
            }
        }

        return node.SyntaxTree.GetLocation(TextSpan.FromBounds(spanToUseForStart.Start, node.Span.End));
    }

    private static bool StartsWithSystemNamespace(NameSyntax node)
    {
        var nameNode = node as NameSyntax;
        while (nameNode != null && nameNode.Kind() == SyntaxKind.QualifiedName)
        {
            nameNode = ((QualifiedNameSyntax)nameNode).Left;
        }

        var simpleNameNode = nameNode as SimpleNameSyntax;

        return simpleNameNode?.Identifier.ValueText == "System";
    }
}
