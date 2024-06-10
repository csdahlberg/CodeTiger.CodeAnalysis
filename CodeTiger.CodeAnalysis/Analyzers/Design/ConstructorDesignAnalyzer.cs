using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design;

/// <summary>
/// Analyzes constructors for design issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConstructorDesignAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor CopyConstructorsShouldNotBePublicDescriptor
        = new DiagnosticDescriptor("CT1001", "Copy constructors should not be public",
            "Copy constructors should not be public", "CodeTiger.Design", DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor ConstructorsForAbstractClassesShouldNotBePublicDescriptor
        = new DiagnosticDescriptor("CT1013", "Constructors for abstract classes should not be public",
            "Constructors for abstract classes should not be public", "CodeTiger.Design",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            CopyConstructorsShouldNotBePublicDescriptor,
            ConstructorsForAbstractClassesShouldNotBePublicDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeCopyConstructor, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeCopyConstructor(SyntaxNodeAnalysisContext context)
    {
        var node = (ConstructorDeclarationSyntax)context.Node;

        var constructor = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);
        if (constructor is null)
        {
            return;
        }

        if (IsProbablyCopyConstructor(context, node, constructor)
            && constructor.DeclaredAccessibility == Accessibility.Public)
        {
            context.ReportDiagnostic(Diagnostic.Create(CopyConstructorsShouldNotBePublicDescriptor,
                node.Identifier.GetLocation()));
        }

        if (constructor.ContainingType.IsAbstract
            && constructor.DeclaredAccessibility == Accessibility.Public)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                ConstructorsForAbstractClassesShouldNotBePublicDescriptor, node.Identifier.GetLocation()));
        }
    }

    private static bool IsProbablyCopyConstructor(SyntaxNodeAnalysisContext context,
        ConstructorDeclarationSyntax constructorDeclaration, IMethodSymbol constructorSymbol)
    {
        if (constructorSymbol.Parameters.Length != 1)
        {
            return false;
        }

        var parameterSymbol = constructorSymbol.Parameters.Single();
        if (!SymbolEqualityComparer.Default.Equals(parameterSymbol.Type, constructorSymbol.ContainingType))
        {
            return false;
        }

        return constructorDeclaration.Body?.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(x => IsAssigningToSameProperty(context, x)) ?? false;
    }

    private static bool IsAssigningToSameProperty(SyntaxNodeAnalysisContext context,
        AssignmentExpressionSyntax assignmentExpression)
    {
        var leftSymbolInfo = context.SemanticModel
            .GetSymbolInfo(assignmentExpression.Left, context.CancellationToken);
        var rightSymbolInfo = context.SemanticModel
            .GetSymbolInfo(assignmentExpression.Right, context.CancellationToken);

        return SymbolEqualityComparer.Default.Equals(leftSymbolInfo.Symbol, rightSymbolInfo.Symbol);
    }
}
