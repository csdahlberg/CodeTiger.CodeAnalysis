using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeTiger.CodeAnalysis.CSharp;

internal static class SyntaxNodeExtensions
{
    public static Location GetIdentifierLocation(this SyntaxNode node)
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
            case SyntaxKind.EnumDeclaration:
                return ((BaseTypeDeclarationSyntax)node).Identifier.GetLocation();
            case SyntaxKind.EnumMemberDeclaration:
                return ((EnumMemberDeclarationSyntax)node).Identifier.GetLocation();
            case SyntaxKind.FieldDeclaration:
            case SyntaxKind.EventFieldDeclaration:
                return ((BaseFieldDeclarationSyntax)node).Declaration.Variables.FirstOrDefault()
                    ?.Identifier.GetLocation() ?? node.GetLocation();
            case SyntaxKind.MethodDeclaration:
                return ((MethodDeclarationSyntax)node).Identifier.GetLocation();
            case SyntaxKind.OperatorDeclaration:
                return ((OperatorDeclarationSyntax)node).OperatorToken.GetLocation();
            case SyntaxKind.ConversionOperatorDeclaration:
                return ((ConversionOperatorDeclarationSyntax)node).Type.GetLocation();
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
