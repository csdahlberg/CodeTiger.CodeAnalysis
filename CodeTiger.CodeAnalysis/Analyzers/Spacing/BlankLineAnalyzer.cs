using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeTiger.CodeAnalysis.Analyzers.Spacing;

/// <summary>
/// Analyzes files for style issues related to blank lines.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BlankLineAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor FilesShouldNotBeginWithBlankLinesDescriptor
        = new DiagnosticDescriptor("CT3000", "Files should not begin with blank lines",
            "Files should not begin with blank lines", "CodeTiger.Spacing", DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor FilesShouldEndWithABlankLineDescriptor
        = new DiagnosticDescriptor("CT3001", "Files should end with a blank line",
            "Files should end with a blank line", "CodeTiger.Spacing", DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor CodeShouldNotContainConsecutiveBlankLinesDescriptor
        = new DiagnosticDescriptor("CT3002", "Code should not contain consecutive blank lines",
            "Code should not contain consecutive blank lines", "CodeTiger.Spacing", DiagnosticSeverity.Warning,
            true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        FilesShouldNotBeginWithBlankLinesDescriptor,
        FilesShouldEndWithABlankLineDescriptor,
        CodeShouldNotContainConsecutiveBlankLinesDescriptor);

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

        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTreeForBlankLines);
    }

    private static void AnalyzeSyntaxTreeForBlankLines(SyntaxTreeAnalysisContext context)
    {
        var lines = context.Tree.GetText(context.CancellationToken).Lines;
        if (lines.Count > 0)
        {
            AnalyzeForStartingBlankLines(context, lines);
            AnalyzeForEndingBlankLine(context, lines);
            AnalyzeForConsecutiveBlankLines(context, lines);
        }
    }

    private static void AnalyzeForStartingBlankLines(SyntaxTreeAnalysisContext context, TextLineCollection lines)
    {
        int length = 0;
        for (int i = 0; i < lines.Count && string.IsNullOrWhiteSpace(lines[i].ToString()); i += 1)
        {
            length += lines[i].SpanIncludingLineBreak.Length;
        }

        if (length > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(FilesShouldNotBeginWithBlankLinesDescriptor,
                Location.Create(context.Tree, new TextSpan(0, length))));
        }
    }

    private static void AnalyzeForEndingBlankLine(SyntaxTreeAnalysisContext context, TextLineCollection lines)
    {
        var lastLine = lines[^1];

        if (!string.IsNullOrWhiteSpace(lastLine.ToString()))
        {
            context.ReportDiagnostic(Diagnostic.Create(FilesShouldEndWithABlankLineDescriptor,
                Location.Create(context.Tree, lastLine.Span)));
        }
    }

    private static void AnalyzeForConsecutiveBlankLines(SyntaxTreeAnalysisContext context,
        TextLineCollection lines)
    {
        for (int i = 1; i < lines.Count; i += 1)
        {
            if (string.IsNullOrWhiteSpace(lines[i - 1].ToString())
                && string.IsNullOrWhiteSpace(lines[i].ToString()))
            {
                var containingNode = context.Tree.GetRoot(context.CancellationToken).FindToken(lines[i].Start);
                if (!IsStringLiteral(containingNode))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        CodeShouldNotContainConsecutiveBlankLinesDescriptor,
                        Location.Create(context.Tree, lines[i].Span)));
                }
            }
        }
    }

    private static bool IsStringLiteral(SyntaxToken token)
    {
        switch (token.Kind())
        {
            case SyntaxKind.InterpolatedStringTextToken:
            case SyntaxKind.InterpolatedStringToken:
            case SyntaxKind.StringLiteralToken:
#if ROSLYN4_4_OR_HIGHER
            case SyntaxKind.MultiLineRawStringLiteralToken:
            case SyntaxKind.SingleLineRawStringLiteralToken:
            case SyntaxKind.Utf8MultiLineRawStringLiteralToken:
            case SyntaxKind.Utf8SingleLineRawStringLiteralToken:
#endif
                return true;
            default:
                return false;
        }
    }
}
