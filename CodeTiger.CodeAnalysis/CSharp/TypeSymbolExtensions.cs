using Microsoft.CodeAnalysis;

namespace CodeTiger.CodeAnalysis.CSharp
{
    internal static class TypeSymbolExtensions
    {
        public static bool IsSubclassOf(this ITypeSymbol type, ITypeSymbol otherType)
        {
            var baseType = otherType.BaseType;

            while (baseType != null)
            {
                if (type == baseType)
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}
