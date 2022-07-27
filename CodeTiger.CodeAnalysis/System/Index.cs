using CodeTiger.CodeAnalysis;

namespace System;

/// <summary>
/// A minimal implementation of <c>System.Index</c> required to allow range-based indexing in this project.
/// </summary>
internal readonly struct Index
{
    private readonly int _value;

    public Index(int value, bool isFromEnd)
    {
        Guard.ArgumentIsNotNegative(nameof(value), value);

        _value = isFromEnd ? ~value : value;
    }

    public int GetOffset(int length)
    {
        int offset = _value;

        return offset < 0
            ? offset + length + 1
            : offset;
    }

    public override int GetHashCode()
    {
        return _value;
    }
}
