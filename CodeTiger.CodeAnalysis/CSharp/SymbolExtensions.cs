using Microsoft.CodeAnalysis;

namespace CodeTiger.CodeAnalysis.CSharp;

internal static class SymbolExtensions
{
    public static string GetFullName(this ISymbol symbol)
    {
        if (symbol.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace)
        {
            return string.Concat(GetFullName(symbol.ContainingNamespace), ".", symbol.Name);
        }

        return symbol.Name;
    }

    public static bool IsExternallyAccessible(this ISymbol symbol)
    {
        if (symbol.ContainingType != null && !IsExternallyAccessible(symbol.ContainingType))
        {
            return false;
        }

        switch (symbol.DeclaredAccessibility)
        {
            case Accessibility.Public:
            case Accessibility.Protected:
            case Accessibility.ProtectedOrInternal:
                return true;
            case Accessibility.Internal:
            case Accessibility.Private:
            case Accessibility.ProtectedAndInternal:
            default:
                return false;
        }
    }
}
