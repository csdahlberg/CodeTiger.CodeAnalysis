﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Usage;

/// <summary>
/// Analyzes usage of the <see cref="SuppressMessageAttribute"/> class.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SuppressMessageAttributeUsageAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor SuppressMessageAttributeShouldIncludeJustificationDescriptor
        = new DiagnosticDescriptor("CT2201",
            "Use of the SuppressMessage attribute should include justification",
            "Use of the SuppressMessage attribute should include justification",
            "CodeTiger.Usage", DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(SuppressMessageAttributeShouldIncludeJustificationDescriptor);

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

        context.RegisterSemanticModelAction(AnalyzeSuppressMessageAttributeUsage);
    }

    private static void AnalyzeSuppressMessageAttributeUsage(SemanticModelAnalysisContext context)
    {
        var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);
        var suppressMessageAttributeType = context.SemanticModel.Compilation
            .GetTypeByMetadataName(typeof(SuppressMessageAttribute).FullName);

        var attributeUsages = root.DescendantNodes().OfType<AttributeSyntax>();

        foreach (var attributeUsage in attributeUsages)
        {
            var attributeType = context.SemanticModel.GetTypeInfo(attributeUsage, context.CancellationToken);
            if (SymbolEqualityComparer.Default.Equals(attributeType.Type, suppressMessageAttributeType)
                && !AttributeIncludesJustificationArgument(attributeUsage))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    SuppressMessageAttributeShouldIncludeJustificationDescriptor,
                    attributeUsage.GetLocation()));
            }
        }
    }

    private static bool AttributeIncludesJustificationArgument(AttributeSyntax attributeUsage)
    {
        return attributeUsage?.ArgumentList?.Arguments
            .Any(arg => arg.NameEquals?.Name?.Identifier.ValueText == "Justification")
            ?? false;
    }
}
