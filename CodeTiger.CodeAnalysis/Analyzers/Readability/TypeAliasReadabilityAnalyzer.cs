using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Readability
{
    /// <summary>
    /// Analyzes declarations for the use of built-in type aliases.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypeAliasReadabilityAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor BuiltInTypeAliasesShouldBeUsedDescriptor
            = new DiagnosticDescriptor("CT3101", "Built-in type aliases should be used.",
                "Built-in type alises should be used.", "CodeTiger.Readability", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ShorthandShouldBeUsedForNullableTypesDescriptor
            = new DiagnosticDescriptor("CT3102", "Shorthand should be used for nullable types.",
                "Shorthand should be used for nullable types.", "CodeTiger.Readability",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(BuiltInTypeAliasesShouldBeUsedDescriptor,
                    ShorthandShouldBeUsedForNullableTypesDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.IdentifierName);
            context.RegisterSyntaxNodeAction(AnalyzeNullableShorthand, SyntaxKind.GenericName);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken).Symbol;
            if (symbol?.Kind != SymbolKind.NamedType)
            {
                return;
            }

            var namedTypeSymbol = (INamedTypeSymbol)symbol;

            switch (namedTypeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                    context.ReportDiagnostic(Diagnostic.Create(BuiltInTypeAliasesShouldBeUsedDescriptor,
                        context.Node.GetLocation()));
                    break;
            }
        }

        private void AnalyzeNullableShorthand(SyntaxNodeAnalysisContext context)
        {
            var genericName = (GenericNameSyntax)context.Node;
            
            var symbol = context.SemanticModel.GetSymbolInfo(genericName, context.CancellationToken).Symbol;
            if (symbol?.Kind != SymbolKind.NamedType)
            {
                return;
            }

            var namedTypeSymbol = (INamedTypeSymbol)symbol;
            if (namedTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShorthandShouldBeUsedForNullableTypesDescriptor,
                    context.Node.GetLocation()));
            }
        }
    }
}