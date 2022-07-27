using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Readability;

/// <summary>
/// Analyzes readability issues caused by the use of regions.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RegionReadabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor RegionsShouldNotBeUsedDescriptor = new DiagnosticDescriptor(
        "CT3109", "Regions should not be used.", "Regions should not be used.", "CodeTiger.Readability",
        DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(RegionsShouldNotBeUsedDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeRegions, SyntaxKind.RegionDirectiveTrivia);
    }

    private static void AnalyzeRegions(SyntaxNodeAnalysisContext context)
    {
        var node = (RegionDirectiveTriviaSyntax)context.Node;

        context.ReportDiagnostic(Diagnostic.Create(RegionsShouldNotBeUsedDescriptor,
            node.RegionKeyword.GetLocation()));
    }
}
