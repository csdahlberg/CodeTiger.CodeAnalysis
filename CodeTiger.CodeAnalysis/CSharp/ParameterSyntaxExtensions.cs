#if !ROSLYN3_8_OR_HIGHER
using System.Diagnostics.CodeAnalysis;
#endif
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeTiger.CodeAnalysis.CSharp;

internal static class ParameterSyntaxExtensions
{
    public static bool IsParams(this ParameterSyntax node)
    {
        return node.Modifiers.Any(SyntaxKind.ParamsKeyword);
    }

#if !ROSLYN3_8_OR_HIGHER
    [SuppressMessage("CodeTiger.Design", "CT1020:Extension methods should use the 'this' parameter",
        Justification = "Records did not exist in older versions of Roslyn. Returning 'false' is preferred.")]
#endif
    public static bool IsInRecordDeclaration(this ParameterSyntax node)
    {
#if ROSLYN3_8_OR_HIGHER
        return node.Parent?.Parent?.Kind() == SyntaxKind.RecordDeclaration;
#else
        return false;
#endif
    }
}
