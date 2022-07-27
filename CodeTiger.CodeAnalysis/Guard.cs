using System;

namespace CodeTiger.CodeAnalysis;

/// <summary>
/// Contains methods for ensuring method calls and arguments passed in to them are valid.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Ensures that an argument is not null, throwing an exception if it is null.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="name">The name of the argument.</param>
    /// <param name="value">The value of the argument.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void ArgumentIsNotNull<T>(string name, [ValidatedNotNull] T value)
        where T : class
    {
        if (value == null)
        {
            throw new ArgumentNullException(name);
        }
    }

    /// <summary>
    /// Ensures that the value of a <see cref="int"/> argument is not negative, throwing an exception if it is
    /// negative.
    /// </summary>
    /// <param name="name">The name of the argument.</param>
    /// <param name="value">The value of the argument.</param>
    /// <returns><paramref name="value"/> if it is not negative.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static int ArgumentIsNotNegative(string name, int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(name);
        }

        return value;
    }
}
