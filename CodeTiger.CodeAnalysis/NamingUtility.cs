using System.Linq;

namespace CodeTiger.CodeAnalysis
{
    internal static class NamingUtility
    {
        public static bool? IsProbablyPascalCased(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            char[] valueCharacters = value.ToCharArray();

            return char.IsUpper(valueCharacters[0])
                && valueCharacters.All(char.IsLetterOrDigit)
                && (valueCharacters.Length == 1 || valueCharacters.Any(char.IsLower));
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
