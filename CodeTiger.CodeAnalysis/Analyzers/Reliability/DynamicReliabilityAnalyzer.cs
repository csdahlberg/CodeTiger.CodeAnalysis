using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability;

/// <summary>
/// Analyzes the use of <c>dynamic</c> for potential reliability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DynamicReliabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor DynamicShouldNotBeUsedDescriptor
        = new DiagnosticDescriptor("CT2010", "Dynamic should not be used", "Dynamic should not be used",
            "CodeTiger.Reliability", DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(DynamicShouldNotBeUsedDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeDynamicReliability, SyntaxKind.IdentifierName);
    }

    private static void AnalyzeDynamicReliability(SyntaxNodeAnalysisContext context)
    {
        var identifierName = (IdentifierNameSyntax)context.Node;
        
        if (identifierName.Identifier.ValueText == "dynamic")
        {
            var identifierSymbol = context.SemanticModel
                .GetSymbolInfo(identifierName, context.CancellationToken).Symbol;
            var typeSymbol = identifierSymbol as ITypeSymbol;

            if (typeSymbol?.TypeKind == TypeKind.Dynamic && !IsInherited(context, identifierName.Parent))
            {
                context.ReportDiagnostic(Diagnostic.Create(DynamicShouldNotBeUsedDescriptor,
                    identifierName.Identifier.GetLocation()));
            }
        }
    }

    private static bool IsInherited(SyntaxNodeAnalysisContext context, SyntaxNode? parentNode)
    {
        if (parentNode is null)
        {
            return false;
        }

        // For parameters, try to get the containing method or property
        while ((parentNode.IsKind(SyntaxKind.Parameter) || parentNode.IsKind(SyntaxKind.ParameterList))
            && parentNode.Parent is not null)
        {
            parentNode = parentNode.Parent;
        }

        if (parentNode.IsKind(SyntaxKind.MethodDeclaration))
        {
            var parentMethodNode = (MethodDeclarationSyntax)parentNode;
            var parentMethod = context.SemanticModel
                .GetDeclaredSymbol(parentMethodNode, context.CancellationToken);
            if (parentMethod is null)
            {
                return false;
            }

            if (IsInherited(parentMethod, parentMethod.ContainingType.BaseType)
                || parentMethod.ContainingType.AllInterfaces.Any(x => IsInherited(parentMethod, x)))
            {
                return true;
            }
        }
        else if (parentNode.IsKind(SyntaxKind.PropertyDeclaration))
        {
            var parentPropertyNode = (PropertyDeclarationSyntax)parentNode;
            var parentProperty = context.SemanticModel
                .GetDeclaredSymbol(parentPropertyNode, context.CancellationToken);
            if (parentProperty is null)
            {
                return false;
            }

            if (IsInherited(parentProperty, parentProperty.ContainingType.BaseType)
                || parentProperty.ContainingType.AllInterfaces.Any(x => IsInherited(parentProperty, x)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInherited(IMethodSymbol method, ITypeSymbol? baseTypeOrInterface)
    {
        var baseType = baseTypeOrInterface;

        while (baseType != null)
        {
            var matchingBaseMethods = baseType.GetMembers(method.Name)
                .Where(x => x.Kind == SymbolKind.Method)
                .OfType<IMethodSymbol>()
                .Where(x => AreParameterTypesEqual(method.Parameters, x.Parameters))
                .ToList();

            if (matchingBaseMethods.Count >= 1)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool IsInherited(IPropertySymbol property, ITypeSymbol? baseTypeOrInterface)
    {
        var baseType = baseTypeOrInterface;

        while (baseType != null)
        {
            var matchingBaseProperties = baseType.GetMembers(property.Name)
                .Where(x => x.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(x => AreParameterTypesEqual(property.Parameters, x.Parameters))
                .ToList();

            if (matchingBaseProperties.Count >= 1)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool AreParameterTypesEqual(ImmutableArray<IParameterSymbol> firstParameters,
        ImmutableArray<IParameterSymbol> secondParameters)
    {
        if (firstParameters.Length != secondParameters.Length)
        {
            return false;
        }

        for (int i = 0; i < firstParameters.Length; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(firstParameters[i].Type, secondParameters[i].Type))
            {
                return false;
            }
        }

        return true;
    }
}
