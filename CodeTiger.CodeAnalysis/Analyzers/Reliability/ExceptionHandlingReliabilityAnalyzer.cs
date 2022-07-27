using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability;

/// <summary>
/// Analyzes exception handling for potential reliability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExceptionHandlingReliabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor EmptyCatchBlocksShouldNotBeUsedDescriptor
        = new DiagnosticDescriptor("CT2007", "Empty catch blocks should not be used.",
            "Empty catch blocks should not be used.", "CodeTiger.Reliability", DiagnosticSeverity.Warning,
            true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(EmptyCatchBlocksShouldNotBeUsedDescriptor);

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

        context.RegisterCodeBlockAction(AnalyzeCatchBlocks);
    }

    private static void AnalyzeCatchBlocks(CodeBlockAnalysisContext context)
    {
        foreach (var catchClause in context.CodeBlock.DescendantNodes().OfType<CatchClauseSyntax>())
        {
            if (!catchClause.Block.DescendantNodesAndSelf()
                .Any(x => x.Kind() != SyntaxKind.EmptyStatement && x.Kind() != SyntaxKind.Block))
            {
                context.ReportDiagnostic(Diagnostic.Create(EmptyCatchBlocksShouldNotBeUsedDescriptor,
                    catchClause.CatchKeyword.GetLocation()));
            }
        }
    }
}
