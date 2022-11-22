using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Ordering;

/// <summary>
/// Analyzes the order of property and event accessors.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AccessorOrderAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor GetPropertyAccessorsShouldBeBeforeSetPropertyAccessorsDescriptor
        = new DiagnosticDescriptor("CT3215", "Get property accessors should be before set property accessors",
            "Get property accessors should be before set property accessors", "CodeTiger.Ordering",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor AddEventAccessorsShouldBeBeforeRemoveEventAccessorsDescriptor
        = new DiagnosticDescriptor("CT3216", "Add event accessors should be before remove event accessors",
            "Add event accessors should be before remove event accessors", "CodeTiger.Ordering",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(GetPropertyAccessorsShouldBeBeforeSetPropertyAccessorsDescriptor,
                AddEventAccessorsShouldBeBeforeRemoveEventAccessorsDescriptor);
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

        context.RegisterSyntaxNodeAction(AnalyzePropertyAccessorOrder, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeEventAccessorOrder, SyntaxKind.EventDeclaration);
    }

    private static void AnalyzePropertyAccessorOrder(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node as PropertyDeclarationSyntax;
        if (node?.AccessorList?.Accessors.Count > 1)
        {
            bool wasSetEncountered = false;
            foreach (var accessor in node.AccessorList.Accessors)
            {
                if (wasSetEncountered && accessor.Kind() == SyntaxKind.GetAccessorDeclaration)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        GetPropertyAccessorsShouldBeBeforeSetPropertyAccessorsDescriptor,
                        accessor.GetLocation()));
                }

                if (accessor.Kind() == SyntaxKind.SetAccessorDeclaration)
                {
                    wasSetEncountered = true;
                }
            }
        }
    }

    private static void AnalyzeEventAccessorOrder(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node as EventDeclarationSyntax;
        if (node?.AccessorList?.Accessors.Count > 1)
        {
            bool wasRemoveEncountered = false;
            foreach (var accessor in node.AccessorList.Accessors)
            {
                if (wasRemoveEncountered && accessor.Kind() == SyntaxKind.AddAccessorDeclaration)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        AddEventAccessorsShouldBeBeforeRemoveEventAccessorsDescriptor,
                        accessor.GetLocation()));
                }

                if (accessor.Kind() == SyntaxKind.RemoveAccessorDeclaration)
                {
                    wasRemoveEncountered = true;
                }
            }
        }
    }
}
