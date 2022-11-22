using System.Collections.Immutable;
using System.Linq;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design;

/// <summary>
/// Analyzes design issues related to parameters.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterDesignAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor MethodsShouldNotExceedSevenParametersDescriptor
        = new DiagnosticDescriptor("CT1003", "Methods should not exceed seven parameters",
            "Methods should not exceed seven parameters", "CodeTiger.Design", DiagnosticSeverity.Warning,
            true);
    internal static readonly DiagnosticDescriptor ExtensionMethodsShouldUseTheThisParameterDescriptor
        = new DiagnosticDescriptor("CT1020", "Extension methods should use the 'this' parameter",
            "Extension methods should use the 'this' parameter", "CodeTiger.Design", DiagnosticSeverity.Warning,
            true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            MethodsShouldNotExceedSevenParametersDescriptor,
            ExtensionMethodsShouldUseTheThisParameterDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeParameterCount, SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration, SyntaxKind.DelegateDeclaration,
            SyntaxKind.IndexerDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.OperatorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMethodForThisParameter, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeParameterCount(SyntaxNodeAnalysisContext context)
    {
        if (GetParameterCount(context.Node) > 7)
        {
            context.ReportDiagnostic(Diagnostic.Create(MethodsShouldNotExceedSevenParametersDescriptor,
                context.Node.GetIdentifierLocation()));
        }
    }

    private static void AnalyzeMethodForThisParameter(SyntaxNodeAnalysisContext context)
    {
        var node = (MethodDeclarationSyntax)context.Node;

        if (node.Body == null && node.ExpressionBody == null)
        {
            return;
        }

        var methodDeclaration = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);

        if (methodDeclaration.IsExtensionMethod)
        {
            var thisParameterNode = node.ParameterList.Parameters
                .Single(x => x.Modifiers.Any(SyntaxKind.ThisKeyword));
            var thisParameter = context.SemanticModel
                .GetDeclaredSymbol(thisParameterNode, context.CancellationToken);

            var dataFlow = node.Body != null
                ? context.SemanticModel.AnalyzeDataFlow(node.Body)
                : context.SemanticModel.AnalyzeDataFlow(node.ExpressionBody);

            if (!dataFlow.ReadInside.Contains(thisParameter)
                && !dataFlow.WrittenInside.Contains(thisParameter))
            {
                context.ReportDiagnostic(Diagnostic.Create(ExtensionMethodsShouldUseTheThisParameterDescriptor,
                    node.Identifier.GetLocation()));
            }
        }
    }

    private static int GetParameterCount(SyntaxNode node)
    {
        BaseParameterListSyntax parameterList;

        switch (node.Kind())
        {
            case SyntaxKind.ConstructorDeclaration:
                parameterList = ((ConstructorDeclarationSyntax)node).ParameterList;
                break;
            case SyntaxKind.ConversionOperatorDeclaration:
                parameterList = ((ConversionOperatorDeclarationSyntax)node).ParameterList;
                break;
            case SyntaxKind.DelegateDeclaration:
                parameterList = ((DelegateDeclarationSyntax)node).ParameterList;
                break;
            case SyntaxKind.IndexerDeclaration:
                parameterList = ((IndexerDeclarationSyntax)node).ParameterList;
                break;
            case SyntaxKind.MethodDeclaration:
                parameterList = ((MethodDeclarationSyntax)node).ParameterList;
                break;
            case SyntaxKind.OperatorDeclaration:
                parameterList = ((OperatorDeclarationSyntax)node).ParameterList;
                break;
            default:
                return 0;
        }

        return parameterList?.Parameters.Count ?? 0;
    }
}
