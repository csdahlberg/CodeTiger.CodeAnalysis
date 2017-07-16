using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeTiger.CodeAnalysis.CSharp
{
    internal static class MemberDeclarationSyntaxExtensions
    {
        public static Location GetIdentifierLocation(this MemberDeclarationSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.DelegateDeclaration:
                    return ((DelegateDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.NamespaceDeclaration:
                    return ((NamespaceDeclarationSyntax)node).Name.GetLocation();
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                    return ((TypeDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.EnumDeclaration:
                    return ((EnumDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.EnumMemberDeclaration:
                    return ((EnumMemberDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.FieldDeclaration:
                    return ((FieldDeclarationSyntax)node).Declaration.Variables.FirstOrDefault()?.GetLocation()
                        ?? node.GetLocation();
                case SyntaxKind.EventFieldDeclaration:
                    return ((EventFieldDeclarationSyntax)node).Declaration.Variables.FirstOrDefault()
                        ?.GetLocation() ?? node.GetLocation();
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                    return ((OperatorDeclarationSyntax)node).OperatorToken.GetLocation();
                case SyntaxKind.ConstructorDeclaration:
                    return ((ConstructorDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.DestructorDeclaration:
                    return ((DestructorDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.IndexerDeclaration:
                    return ((IndexerDeclarationSyntax)node).ThisKeyword.GetLocation();
                case SyntaxKind.EventDeclaration:
                    return ((EventDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.GlobalStatement:
                case SyntaxKind.IncompleteMember:
                default:
                    return node.GetLocation();
            }
        }
    }
}
