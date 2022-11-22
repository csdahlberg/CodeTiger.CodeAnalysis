using System;
using System.Collections.Immutable;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design;

/// <summary>
/// Analyzes types used for design issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DataTypeDesignAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor ExternallyAccessibleMembersShouldNotUseTuplesDescriptor
        = new DiagnosticDescriptor("CT1014", "Externally-accessible members should not use tuples",
            "Externally-accessible members should not use tuples", "CodeTiger.Design",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(ExternallyAccessibleMembersShouldNotUseTuplesDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclarationForTupleTypes, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclarationForTupleTypes, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclarationForTupleTypes, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeMethodDeclarationForTupleTypes(SyntaxNodeAnalysisContext context)
    {
        var node = (MethodDeclarationSyntax)context.Node;

        var methodDeclaration = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);
        if (methodDeclaration?.IsExternallyAccessible() == true)
        {
            AnalyzeExternallyAccessibleTypeForTuples(context, node.ReturnType);

            if (node.ParameterList != null)
            {
                foreach (var parameterNode in node.ParameterList.Parameters)
                {
                    AnalyzeExternallyAccessibleTypeForTuples(context, parameterNode.Type);
                }
            }
        }
    }

    private static void AnalyzePropertyDeclarationForTupleTypes(SyntaxNodeAnalysisContext context)
    {
        var node = (PropertyDeclarationSyntax)context.Node;

        var propertyDeclaration = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);
        if (propertyDeclaration?.IsExternallyAccessible() == true)
        {
            AnalyzeExternallyAccessibleTypeForTuples(context, node.Type);
        }
    }

    private static void AnalyzeFieldDeclarationForTupleTypes(SyntaxNodeAnalysisContext context)
    {
        var node = (FieldDeclarationSyntax)context.Node;

        var fieldDeclaration = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);
        if (fieldDeclaration?.IsExternallyAccessible() == true)
        {
            AnalyzeExternallyAccessibleTypeForTuples(context, node.Declaration.Type);
        }
    }

    private static void AnalyzeExternallyAccessibleTypeForTuples(SyntaxNodeAnalysisContext context,
        TypeSyntax typeNode)
    {
        if (typeNode == null)
        {
            return;
        }

        var type = context.SemanticModel.GetTypeInfo(typeNode);

        if (type.Type == null)
        {
            return;
        }

        if (type.Type.IsTupleType || type.Type.GetFullName() == typeof(Tuple).FullName)
        {
            context.ReportDiagnostic(Diagnostic.Create(ExternallyAccessibleMembersShouldNotUseTuplesDescriptor,
                typeNode.GetLocation()));
        }
    }
}
