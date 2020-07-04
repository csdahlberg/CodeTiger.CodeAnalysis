using System;
using System.Linq;

namespace CodeTiger.CodeAnalysis
{
    internal static class NamingUtility
    {
        private static readonly char[] _genericTypeAritySeparators = new[] { '`', '_' };
        private static readonly string[] _hungarianNotationPrefixes = new[]
        {
            "b", "c", "d", "f", "i", "l", "li", "n", "o", "p", "s", "si", "str", "sz", "ui", "ul"
        };

        public static bool? IsProbablyHungarianNotation(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.StartsWith("_", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(1);
            }

            foreach (string prefix in _hungarianNotationPrefixes)
            {
                if (value.Length > prefix.Length
                    && value.StartsWith(prefix, StringComparison.Ordinal)
                    && char.IsUpper(value[prefix.Length]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool? IsProbablyPascalCased(string value, bool isGenericTypeArityAllowed = false)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (!char.IsUpper(value[0]))
            {
                return false;
            }

            if (value.Length == 1)
            {
                return null;
            }

            // For generic types, allow things like Thing`2, Thing_2, and Thing`2Tests
            if (isGenericTypeArityAllowed && _genericTypeAritySeparators.Any(value.Contains))
            {                
                string[] valueParts = value.Split(_genericTypeAritySeparators);
                if (valueParts.Length != 2
                    || !valueParts[0].All(char.IsLetterOrDigit)
                    || !valueParts[0].Any(char.IsLower)
                    || valueParts[1].Length == 0
                    || !char.IsDigit(valueParts[1][0]))
                {
                    return false;
                }

                // Make sure there is a valid number for the start of value[1] and any remainder is pascal cased
                char[] charactersAfterTypeAritySeparator = valueParts[1].SkipWhile(char.IsDigit).ToArray();

                if (charactersAfterTypeAritySeparator.Length == 0)
                {
                    return true;
                }

                return char.IsUpper(charactersAfterTypeAritySeparator[0])
                    && charactersAfterTypeAritySeparator.All(char.IsLetterOrDigit)
                    && charactersAfterTypeAritySeparator.Any(char.IsLower);
            }

            return value.All(char.IsLetterOrDigit) && value.Any(char.IsLower);
        }

        public static bool? IsProbablyPascalCased(string value, char leadingCharacter)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.Length < 2)
            {
                return false;
            }

            char[] valueCharacters = value.ToCharArray();

            return valueCharacters[0] == leadingCharacter
                && char.IsUpper(valueCharacters[1])
                && valueCharacters.Skip(1).All(char.IsLetterOrDigit)
                && (valueCharacters.Length == 2 || valueCharacters.Skip(1).Any(char.IsLower));
        }

        public static bool? IsProbablyCamelCased(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (!char.IsLower(value[0]))
            {
                return false;
            }

            char[] valueCharacters = value.ToCharArray();

            return char.IsLower(valueCharacters[0])
                && valueCharacters.All(char.IsLetterOrDigit);
        }

        public static bool? IsProbablyCamelCased(string value, char leadingCharacter)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.Length < 2)
            {
                return false;
            }

            char[] valueCharacters = value.ToCharArray();

            return valueCharacters[0] == leadingCharacter
                && char.IsLower(valueCharacters[1])
                && valueCharacters.Skip(1).All(char.IsLetterOrDigit);
        }
    }
}
