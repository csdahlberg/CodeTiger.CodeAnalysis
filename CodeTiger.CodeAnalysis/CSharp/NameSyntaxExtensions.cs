using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeTiger.CodeAnalysis.CSharp
{
    internal static class NameSyntaxExtensions
    {
        public static SimpleNameSyntax GetUnqualifiedName(this NameSyntax name)
        {
            if (name is SimpleNameSyntax simpleNameSyntax)
            {
                return simpleNameSyntax;
            }

            if (name is QualifiedNameSyntax qualifiedNameSyntax)
            {
                return qualifiedNameSyntax.Right;
            }

            if (name is AliasQualifiedNameSyntax aliasQualifiedNameSyntax)
            {
                return aliasQualifiedNameSyntax.Name;
            }

            return null;
        }
    }
}
