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

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(TypesShouldHaveAccessModifierSpecifiedDescriptor);
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
        }

        private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var baseTypeDeclaration = (BaseTypeDeclarationSyntax)context.Node;

            SyntaxTokenList modifiers;

            switch (baseTypeDeclaration.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                    modifiers = ((TypeDeclarationSyntax)baseTypeDeclaration).Modifiers;
                    break;
                case SyntaxKind.EnumDeclaration:
                    modifiers = ((EnumDeclarationSyntax)baseTypeDeclaration).Modifiers;
                    break;
                default:
                    modifiers = new SyntaxTokenList();
                    break;
            }

            if (!modifiers.Any(x => SyntaxFacts.IsAccessibilityModifier(x.Kind())))
            {
                context.ReportDiagnostic(Diagnostic.Create(TypesShouldHaveAccessModifierSpecifiedDescriptor,
                    baseTypeDeclaration.Identifier.GetLocation()));
            }
        }
    }
}