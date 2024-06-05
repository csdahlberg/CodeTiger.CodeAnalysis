using System.Linq;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.CSharp;

public static class ParameterSyntaxExtensionsTests
{
    public class IsParams
    {
        [Fact]
        public void ReturnsFalseWhenModifiersAreEmpty()
        {
            var parameterNode = SyntaxFactory.Parameter(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(),
                SyntaxFactory.ArrayType(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
                SyntaxFactory.Identifier("input"),
                null);

            bool actual = parameterNode.IsParams();

            Assert.False(actual);
        }

        [Fact]
        public void ReturnsFalseWhenModifiersDoNotContainParams()
        {
            var parameterNode = SyntaxFactory.Parameter(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.OutKeyword)),
                SyntaxFactory.ArrayType(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
                SyntaxFactory.Identifier("input"),
                null);

            bool actual = parameterNode.IsParams();

            Assert.False(actual);
        }

        [Fact]
        public void ReturnsTrueWhenModifiersContainParams()
        {
            var parameterNode = SyntaxFactory.Parameter(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)),
                SyntaxFactory.ArrayType(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
                SyntaxFactory.Identifier("input"),
                null);

            bool actual = parameterNode.IsParams();

            Assert.True(actual);
        }
    }

    public class IsInRecordDeclaration
    {
        [Fact]
        public void ReturnsFalseWhenParentIsNull()
        {
            var parameterNode = SyntaxFactory.Parameter(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(),
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                SyntaxFactory.Identifier("input"),
                null);

            bool actual = parameterNode.IsInRecordDeclaration();

            Assert.False(actual);
        }

        [Fact]
        public void ReturnsFalseWhenParentOfParentIsAMethod()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(
                @"class Thing { Process(string input) { } }");
            var parameterNode = syntaxTree.GetRoot().DescendantNodes().OfType<ParameterSyntax>().Single();

            bool actual = parameterNode.IsInRecordDeclaration();

            Assert.False(actual);
        }

#if ROSLYN3_8_OR_HIGHER
        [Fact]
        public void ReturnsTrueWhenParentOfParentIsARecord()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(
                @"record Thing(string Value);");
            var parameterNode = syntaxTree.GetRoot().DescendantNodes().OfType<ParameterSyntax>().Single();

            bool actual = parameterNode.IsInRecordDeclaration();

            Assert.True(actual);
        }
#endif
    }
}
