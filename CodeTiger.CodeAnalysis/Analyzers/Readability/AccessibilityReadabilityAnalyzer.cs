using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Readability
{
    /// <summary>
    /// Analyzes readability issues related to accessibility modifiers.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AccessibilityReadabilityAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor TypesShouldHaveAccessModifierSpecifiedDescriptor
            = new DiagnosticDescriptor("CT3103", "Types should have an access modifier specified.",
                "Types should have an access modifier specified.", "CodeTiger.Readability",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor MembersShouldHaveAccessModifierSpecifiedDescriptor
            = new DiagnosticDescriptor("CT3104", "Members should have an access modifier specified.",
                "Members should have an access modifier specified.", "CodeTiger.Readability",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(TypesShouldHaveAccessModifierSpecifiedDescriptor,
                    MembersShouldHaveAccessModifierSpecifiedDescriptor);
            }
        }

        /// <summary>
        /// Registers actions in an analysis context.
        /// </summary>
        /// <param name="context">The context to register actions in.</param>
        /// <remarks>This method should only be called once, at the start of a session.</remarks>
        public override void Initialize(AnalysisContext context)
        {
            Guard.ArgumentIsNotNull(nameof(context), context);

            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration, SyntaxKind.EnumDeclaration, SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMemberDeclaration, SyntaxKind.ConstructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration, SyntaxKind.DelegateDeclaration,
                SyntaxKind.EventDeclaration, SyntaxKind.EventFieldDeclaration, SyntaxKind.FieldDeclaration,
                SyntaxKind.IndexerDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.OperatorDeclaration,
                SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var modifiers = GetModifiers(context.Node);

            var identifierLocation = GetIdentifierLocation(context.Node);

            if (!modifiers.Any(x => SyntaxFacts.IsAccessibilityModifier(x.Kind())))
            {
                context.ReportDiagnostic(Diagnostic.Create(TypesShouldHaveAccessModifierSpecifiedDescriptor,
                    identifierLocation));
            }
        }

        private void AnalyzeMemberDeclaration(SyntaxNodeAnalysisContext context)
        {
            var modifiers = GetModifiers(context.Node);

            if (!modifiers.Any(x => SyntaxFacts.IsAccessibilityModifier(x.Kind()))
                && !IsExplicitInterfaceImplementation(context.Node))
            {
                var identifierLocation = GetIdentifierLocation(context.Node);

                context.ReportDiagnostic(Diagnostic.Create(MembersShouldHaveAccessModifierSpecifiedDescriptor,
                    identifierLocation));
            }
        }

        private static SyntaxTokenList GetModifiers(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                    return ((TypeDeclarationSyntax)node).Modifiers;
                case SyntaxKind.EnumDeclaration:
                    return ((EnumDeclarationSyntax)node).Modifiers;
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.OperatorDeclaration:
                    return ((BaseMethodDeclarationSyntax)node).Modifiers;
                case SyntaxKind.DelegateDeclaration:
                    return ((DelegateDeclarationSyntax)node).Modifiers;
                case SyntaxKind.EventDeclaration:
                case SyntaxKind.IndexerDeclaration:
                case SyntaxKind.PropertyDeclaration:
                    return ((BasePropertyDeclarationSyntax)node).Modifiers;
                case SyntaxKind.EventFieldDeclaration:
                case SyntaxKind.FieldDeclaration:
                    return ((BaseFieldDeclarationSyntax)node).Modifiers;
                default:
                    return new SyntaxTokenList();
            }
        }

        private static Location GetIdentifierLocation(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.EnumDeclaration:
                    return ((BaseTypeDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.ConstructorDeclaration:
                    return ((ConstructorDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.ConversionOperatorDeclaration:
                    return ((ConversionOperatorDeclarationSyntax)node).Type.GetLocation();
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.OperatorDeclaration:
                    return ((OperatorDeclarationSyntax)node).OperatorToken.GetLocation();
                case SyntaxKind.DelegateDeclaration:
                    return ((DelegateDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.EventDeclaration:
                    return ((EventDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.IndexerDeclaration:
                    return ((IndexerDeclarationSyntax)node).ThisKeyword.GetLocation();
                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.EventFieldDeclaration:
                    return ((EventFieldDeclarationSyntax)node).Declaration.Variables.First().Identifier
                        .GetLocation();
                case SyntaxKind.FieldDeclaration:
                    return ((FieldDeclarationSyntax)node).Declaration.Variables.First().Identifier.GetLocation();
                default:
                    return null;
            }
        }

        private static bool IsExplicitInterfaceImplementation(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).ExplicitInterfaceSpecifier != null;
                case SyntaxKind.EventDeclaration:
                    return ((EventDeclarationSyntax)node).ExplicitInterfaceSpecifier != null;
                case SyntaxKind.IndexerDeclaration:
                    return ((IndexerDeclarationSyntax)node).ExplicitInterfaceSpecifier != null;
                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)node).ExplicitInterfaceSpecifier != null;
                default:
                    return false;
            }
        }
    }
}