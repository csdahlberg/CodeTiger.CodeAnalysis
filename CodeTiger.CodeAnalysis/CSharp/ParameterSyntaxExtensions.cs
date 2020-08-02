using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeTiger.CodeAnalysis.CSharp
{
    internal static class ParameterSyntaxExtensions
    {
        public static bool IsParams(this ParameterSyntax node)
        {
            return node.Modifiers.Any(SyntaxKind.ParamsKeyword);
        }
    }
}
