using Microsoft.CodeAnalysis;

namespace CodeTiger.CodeAnalysis.CSharp
{
    internal static class SymbolExtensions
    {
        public static bool IsExternallyAccessible(this ISymbol symbol)
        {
            if (symbol.ContainingType?.ContainingType != null
                && !IsExternallyAccessible(symbol.ContainingType?.ContainingType))
            {
                return false;
            }

            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Public:
                case Accessibility.Protected:
                case Accessibility.ProtectedAndInternal:
                case Accessibility.ProtectedOrInternal:
                    return true;
                case Accessibility.Internal:
                case Accessibility.Private:
                default:
                    return false;
            }
        }
    }
}
