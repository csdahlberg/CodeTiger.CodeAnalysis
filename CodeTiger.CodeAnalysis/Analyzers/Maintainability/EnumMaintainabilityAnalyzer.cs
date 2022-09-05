using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Maintainability;

/// <summary>
/// Analyzes parmeters for maintainability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnumMaintainabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor
        CompositeValuesInFlagsEnumerationsShouldShouldEqualACombinationOfOtherValuesDescriptor
        = new DiagnosticDescriptor("CT1505",
            "Composite values in Flags enumerations should equal a combination of other values",
            "Composite values in Flags enumerations should equal a combination of other values",
            "CodeTiger.Maintainability", DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            CompositeValuesInFlagsEnumerationsShouldShouldEqualACombinationOfOtherValuesDescriptor);

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

        context.RegisterSyntaxNodeAction(AnalyzeEnumForCompositeFlagsValues, SyntaxKind.EnumDeclaration);
    }

    private static void AnalyzeEnumForCompositeFlagsValues(SyntaxNodeAnalysisContext context)
    {
        var node = (EnumDeclarationSyntax)context.Node;

        var enumDeclaration = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);

        if (!HasFlagsAttribute(context, enumDeclaration))
        {
            return;
        }

        var enumValues = enumDeclaration.GetMembers().OfType<IFieldSymbol>();

        foreach (var enumValue in enumValues)
        {
            if (IsProbablyInvalidValue(enumValue.ConstantValue, enumValues))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    CompositeValuesInFlagsEnumerationsShouldShouldEqualACombinationOfOtherValuesDescriptor,
                    enumValue.Locations.First()));
            }
        }
    }

    private static bool HasFlagsAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumDeclaration)
    {
        var flagsAttributeType = context.SemanticModel.Compilation
            .GetTypeByMetadataName(typeof(FlagsAttribute).FullName);

        return enumDeclaration.GetAttributes()
            .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, flagsAttributeType));
    }

    private static bool IsProbablyInvalidValue(object value, IEnumerable<IFieldSymbol> enumValues)
    {
        if (value.GetType() == typeof(long)
            || value.GetType() == typeof(int)
            || value.GetType() == typeof(short)
            || value.GetType() == typeof(sbyte))
        {
            long typedValue = Convert.ToInt64(value, CultureInfo.InvariantCulture);

            return HasMoreThanOneBitSet(typedValue)
                && !IsCombinationOfOtherValues(typedValue, enumValues);
        }

        if (value.GetType() == typeof(ulong)
            || value.GetType() == typeof(uint)
            || value.GetType() == typeof(ushort)
            || value.GetType() == typeof(byte))
        {
            ulong typedValue = Convert.ToUInt64(value, CultureInfo.InvariantCulture);

            return HasMoreThanOneBitSet(typedValue)
                && !IsCombinationOfOtherValues(typedValue, enumValues);
        }

        // This would only happen if an enum has something other than a byte/short/int/long base type
        return false;
    }

    private static bool HasMoreThanOneBitSet(long value)
    {
        if (value == 0)
        {
            return false;
        }

        value &= value - 1;

        return value != 0;
    }

    private static bool HasMoreThanOneBitSet(ulong value)
    {
        if (value == 0)
        {
            return false;
        }

        value &= value - 1;

        return value != 0;
    }

    private static bool IsCombinationOfOtherValues(long value, IEnumerable<IFieldSymbol> enumValues)
    {
        long combination = 0;

        foreach (var enumValue in enumValues)
        {
            long constValue = Convert.ToInt64(enumValue.ConstantValue, CultureInfo.InvariantCulture);
            if (constValue != value)
            {
                combination |= constValue;
            }
        }

        return (value & combination) == value;
    }

    private static bool IsCombinationOfOtherValues(ulong value, IEnumerable<IFieldSymbol> enumValues)
    {
        ulong combination = 0;

        foreach (var enumValue in enumValues)
        {
            ulong constValue = Convert.ToUInt64(enumValue.ConstantValue, CultureInfo.InvariantCulture);
            if (constValue != value)
            {
                combination |= constValue;
            }
        }

        return (value & combination) == value;
    }
}
