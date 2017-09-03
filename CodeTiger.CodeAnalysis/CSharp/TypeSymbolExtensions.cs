using System.Linq;
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
            // Consider all other types a subclass of System.Object
            if (otherType.SpecialType == SpecialType.System_Object)
            {
                return true;
            }

            while (type != null)
            {
                if (type.Equals(otherType))
                {
                    return true;
                }

                if (otherType.Kind == SymbolKind.TypeParameter)
                {
                    var otherTypeParameter = (ITypeParameterSymbol)otherType;
                    if (otherTypeParameter.ConstraintTypes.Any(type.Equals))
                    {
                        return true;
                    }
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
