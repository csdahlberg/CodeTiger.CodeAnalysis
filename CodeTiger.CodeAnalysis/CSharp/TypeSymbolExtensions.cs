using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeTiger.CodeAnalysis.CSharp;

internal static class TypeSymbolExtensions
{
    public static bool IsSubclassOf(this ITypeSymbol type, ITypeSymbol otherType)
    {
        // Consider all other types a subclass of System.Object
        if (otherType.SpecialType == SpecialType.System_Object)
        {
            return true;
        }

        var baseType = type.BaseType;

        return baseType is not null && IsSameOrSubclassOf(baseType, otherType);
    }

    public static bool IsSameOrSubclassOf(this ITypeSymbol type, ITypeSymbol otherType)
    {
        // Consider all other types a subclass of System.Object
        if (otherType.SpecialType == SpecialType.System_Object)
        {
            return true;
        }

        var currentType = type;

        while (currentType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentType, otherType))
            {
                return true;
            }

            if (otherType.Kind == SymbolKind.TypeParameter)
            {
                var otherTypeParameter = (ITypeParameterSymbol)otherType;
                if (otherTypeParameter.ConstraintTypes.Any(currentType.Equals))
                {
                    return true;
                }
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    public static bool IsConstrainedTo(this ITypeSymbol type, ITypeSymbol otherType)
    {
        if (type is ITypeParameterSymbol typeParameter)
        {
            return typeParameter.ConstraintTypes.Any(x => x.IsSameOrSubclassOf(otherType));
        }

        return false;
    }
}
