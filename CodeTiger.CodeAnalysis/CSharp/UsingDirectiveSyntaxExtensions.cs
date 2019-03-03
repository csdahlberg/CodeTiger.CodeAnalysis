using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeTiger.CodeAnalysis.CSharp
{
    internal static class UsingDirectiveSyntaxExtensions
    {
        public static bool IsStaticUsingDirective(this UsingDirectiveSyntax node)
        {
            return node.StaticKeyword != default(SyntaxToken);
        }
    }
}
