using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Naming
{
    /// <summary>
    /// Analyzes symbol names.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SymbolNamingAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor TypeNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1702", "Type names should use pascal casing.",
                "Type names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(TypeNamesShouldUsePascalCasingDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeTypeName, SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration);
        }

        private void AnalyzeTypeName(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken typeIdentifier;

            switch (context.Node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    typeIdentifier = ((ClassDeclarationSyntax)context.Node).Identifier;
                    break;
                case SyntaxKind.StructDeclaration:
                    typeIdentifier = ((StructDeclarationSyntax)context.Node).Identifier;
                    break;
                case SyntaxKind.InterfaceDeclaration:
                    typeIdentifier = ((StructDeclarationSyntax)context.Node).Identifier;
                    break;
                case SyntaxKind.EnumDeclaration:
                    typeIdentifier = ((StructDeclarationSyntax)context.Node).Identifier;
                    break;
                default:
                    return;
            }

            if (!IsProbablyPascalCased(typeIdentifier.Text)
                && !(context.Node.Kind() == SyntaxKind.InterfaceDeclaration
                    && typeIdentifier.Text.StartsWith("I")
                    && IsProbablyPascalCased(typeIdentifier.Text.Substring(1))))
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeNamesShouldUsePascalCasingDescriptor,
                    typeIdentifier.GetLocation()));
            }
        }

        private bool IsProbablyPascalCased(string value)
        {
            char[] valueCharacters = value.ToCharArray();

            return valueCharacters.Length > 1
                && char.IsUpper(value[0])
                && valueCharacters.All(char.IsLetterOrDigit)
                && valueCharacters.Any(char.IsLower);
        }
    }
}