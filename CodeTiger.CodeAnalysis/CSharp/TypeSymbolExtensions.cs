using Microsoft.CodeAnalysis;

namespace CodeTiger.CodeAnalysis.CSharp
{
    internal static class TypeSymbolExtensions
    {
        public static bool IsSubclassOf(this ITypeSymbol type, ITypeSymbol otherType)
        {
            return IsSameOrSubclassOf(type.BaseType, otherType);
        }

        public static bool IsSameOrSubclassOf(this ITypeSymbol type, ITypeSymbol otherType)
        {
            while (type != null)
            {
                if (type.Equals(otherType))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
