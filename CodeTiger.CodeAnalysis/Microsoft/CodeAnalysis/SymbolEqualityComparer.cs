using System.Collections.Generic;

namespace Microsoft.CodeAnalysis;

/// <summary>
/// A basic implementation of SymbolEqualityComparer to be used when targeting versions of Roslyn prior to 3.3.
/// </summary>
internal class SymbolEqualityComparer : IEqualityComparer<ISymbol>
{
    public static readonly SymbolEqualityComparer Default = new SymbolEqualityComparer();

    public bool Equals(ISymbol x, ISymbol y)
    {
        if (x is null)
        {
            return y is null;
        }

        return x.Equals(y);
    }

    public int GetHashCode(ISymbol obj)
    {
        return obj?.GetHashCode() ?? 0;
    }
}
