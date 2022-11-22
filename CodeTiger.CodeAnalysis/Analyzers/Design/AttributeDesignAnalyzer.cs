using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design;

/// <summary>
/// Analyzes the design of <see cref="Attribute"/> subclasses.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeDesignAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor
        AttributeClassesShouldIncludeAnAttributeUsageAttributeDescriptor = new DiagnosticDescriptor("CT1012",
            "Attribute classes should include an AttributeUsage attribute",
            "Attribute classes should include an AttributeUsage attribute", "CodeTiger.Design",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(AttributeClassesShouldIncludeAnAttributeUsageAttributeDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        var node = (ClassDeclarationSyntax)context.Node;

        var attributeUsageType = context.Compilation
            .GetTypeByMetadataName(typeof(AttributeUsageAttribute).FullName);

        if (!IsSubclassOfAttribute(context, node))
        {
            return;
        }

        bool hasAttributeUsageAttribute = false;

        foreach (var customAttribute in node.AttributeLists.SelectMany(x => x.Attributes))
        {
            var customAttributeType = context.SemanticModel
                .GetTypeInfo(customAttribute.Name, context.CancellationToken);
            
            if (customAttributeType.Type == attributeUsageType)
            {
                hasAttributeUsageAttribute = true;
                break;
            }
        }

        if (!hasAttributeUsageAttribute)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                AttributeClassesShouldIncludeAnAttributeUsageAttributeDescriptor,
                node.Identifier.GetLocation()));
        }
    }

    private static bool IsSubclassOfAttribute(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax node)
    {
        if (node.BaseList?.Types == null)
        {
            return false;
        }

        var attributeType = context.Compilation.GetTypeByMetadataName(typeof(Attribute).FullName);

        foreach (var baseTypeNode in node.BaseList.Types)
        {
            var baseType = context.SemanticModel.GetTypeInfo(baseTypeNode.Type, context.CancellationToken);

            if (baseType.Type == attributeType)
            {
                return true;
            }
        }

        return false;
    }
}
