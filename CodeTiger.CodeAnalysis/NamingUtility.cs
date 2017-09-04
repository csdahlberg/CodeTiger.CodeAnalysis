using System.Linq;

namespace CodeTiger.CodeAnalysis
{
    internal static class NamingUtility
    {
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

            if (isGenericTypeArityAllowed)
            {
                var valueParts = value.Split('`');
                if (valueParts.Length == 2 && int.TryParse(valueParts[1], out int arity))
                {
                    char[] valuePartCharacters = valueParts[0].ToCharArray();
                    return valuePartCharacters.All(char.IsLetterOrDigit)
                        && valuePartCharacters.Any(char.IsLower);
                }
            }

            char[] valueCharacters = value.ToCharArray();

            return valueCharacters.All(char.IsLetterOrDigit)
                && valueCharacters.Any(char.IsLower);
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
