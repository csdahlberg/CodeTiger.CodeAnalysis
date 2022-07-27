using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout;

/// <summary>
/// Analyzes the length of lines of code.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LineLengthLayoutAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor LinesShouldNotExceedTheMaximumLengthDescriptor
        = new DiagnosticDescriptor("CT3531", "Lines should not exceed the maximum length of 115.",
            "Lines should not exceed the maximum length of 115.", "CodeTiger.Layout", DiagnosticSeverity.Warning,
            true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(LinesShouldNotExceedTheMaximumLengthDescriptor);

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

        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var text = context.Tree.GetText(context.CancellationToken);

        foreach (var line in text.Lines)
        {
            int lineColumns = 0;
            int firstCharacterExceedingMaxLength = -1;
            for (int i = line.Span.Start; i < line.Span.End; i += 1)
            {
                if (line.Text[i] == '\t')
                {
                    lineColumns += 4;
                }
                else
                {
                    lineColumns += 1;
                }

                if (lineColumns > 115)
                {
                    firstCharacterExceedingMaxLength = i;
                    break;
                }
            }

            if (firstCharacterExceedingMaxLength > 0)
            {
                var lineSpanExceedingMaxLength = TextSpan.FromBounds(firstCharacterExceedingMaxLength, line.End);
                context.ReportDiagnostic(Diagnostic.Create(LinesShouldNotExceedTheMaximumLengthDescriptor,
                    Location.Create(context.Tree, lineSpanExceedingMaxLength)));
            }
        }
    }
}
