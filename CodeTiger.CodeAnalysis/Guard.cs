using System;

namespace CodeTiger.CodeAnalysis
{
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
    }
}
