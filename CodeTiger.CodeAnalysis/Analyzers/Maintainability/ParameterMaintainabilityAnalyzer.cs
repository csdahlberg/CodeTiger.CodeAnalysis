using System;
using System.Collections.Immutable;
using System.Linq;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Maintainability;

/// <summary>
/// Analyzes parmeters for maintainability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterMaintainabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor
        ExternallyAccessibleParametersShouldNotHaveDefaultValuesDescriptor = new DiagnosticDescriptor(
            "CT1501", "Externally-accessible parameters should not have default values",
            "Externally-accessible parameters should not have default values", "CodeTiger.Maintainability",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            ExternallyAccessibleParametersShouldNotHaveDefaultValuesDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeMethodForDefaultParameterValues,
            SyntaxKind.ConstructorDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodForDefaultParameterValues(SyntaxNodeAnalysisContext context)
    {
        var node = (BaseMethodDeclarationSyntax)context.Node;

        var methodDeclaration = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);
        if (methodDeclaration?.IsExternallyAccessible() == true)
        {
            foreach (var parameter in methodDeclaration.Parameters.Where(x => x.HasExplicitDefaultValue))
            {
                if (!IsDefaultValueAllowed(parameter.ExplicitDefaultValue, parameter.Type))
                {
                    var parameterNode = parameter.DeclaringSyntaxReferences.First()
                        .GetSyntax(context.CancellationToken);

                    context.ReportDiagnostic(Diagnostic.Create(
                        ExternallyAccessibleParametersShouldNotHaveDefaultValuesDescriptor,
                        parameterNode.GetLocation()));
                }
            }
        }
    }

    private static bool IsDefaultValueAllowed(object? defaultValue, ITypeSymbol type)
    {
        if (defaultValue == null)
        {
            return true;
        }

        if (type.IsReferenceType || type.Kind == SymbolKind.TypeParameter)
        {
            return defaultValue == null;
        }
        
        if (type.IsValueType)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return default(bool).Equals(defaultValue);
                case SpecialType.System_Byte:
                    return default(byte).Equals(defaultValue);
                case SpecialType.System_Char:
                    return default(char).Equals(defaultValue);
                case SpecialType.System_Decimal:
                    return default(decimal).Equals(defaultValue);
                case SpecialType.System_Double:
                    return default(double).Equals(defaultValue);
                case SpecialType.System_Int16:
                    return default(short).Equals(defaultValue);
                case SpecialType.System_Int32:
                    return default(int).Equals(defaultValue);
                case SpecialType.System_Int64:
                    return default(long).Equals(defaultValue);
                case SpecialType.System_IntPtr:
                    return default(IntPtr).Equals(defaultValue);
                case SpecialType.System_SByte:
                    return default(sbyte).Equals(defaultValue);
                case SpecialType.System_Single:
                    return default(float).Equals(defaultValue);
                case SpecialType.System_UInt16:
                    return default(ushort).Equals(defaultValue);
                case SpecialType.System_UInt32:
                    return default(uint).Equals(defaultValue);
                case SpecialType.System_UInt64:
                    return default(ulong).Equals(defaultValue);
                case SpecialType.System_UIntPtr:
                    return default(UIntPtr).Equals(defaultValue);
            }

            if (type is INamedTypeSymbol namedType && namedType.EnumUnderlyingType != null)
            {
                return IsDefaultValueAllowed(defaultValue, namedType.EnumUnderlyingType);
            }
        }

        // Every parameter type should be a type parameter, a reference type, or a value type, so this should
        // never happen. 
        return false;
    }
}
