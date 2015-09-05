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
        internal static readonly DiagnosticDescriptor ConstantFieldNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1703", "Constant field names should use pascal casing.",
                "Constant field names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning,
                true);
        internal static readonly DiagnosticDescriptor ParameterNamesShouldUseCamcelCasingDescriptor
            = new DiagnosticDescriptor("CT1712", "Parameter names should use camel casing.",
                "Parameter names should use camel casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(TypeNamesShouldUsePascalCasingDescriptor,
                    ConstantFieldNamesShouldUsePascalCasingDescriptor,
                    ParameterNamesShouldUseCamcelCasingDescriptor);
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
            context.RegisterSyntaxNodeAction(AnalyzeConstantFieldName, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeParameterName, SyntaxKind.Parameter);
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
                    && typeIdentifier.Text[0] == 'I'
                    && IsProbablyPascalCased(typeIdentifier.Text.Substring(1))))
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeNamesShouldUsePascalCasingDescriptor,
                    typeIdentifier.GetLocation()));
            }
        }

        private void AnalyzeConstantFieldName(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclarationNode = (FieldDeclarationSyntax)context.Node;
            if (!fieldDeclarationNode.Modifiers.Any(x => x.Kind() == SyntaxKind.ConstKeyword))
            {
                return;
            }

            foreach (var fieldDeclaration in fieldDeclarationNode.Declaration.Variables)
            {
                if (!IsProbablyPascalCased(fieldDeclaration.Identifier.Text))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ConstantFieldNamesShouldUsePascalCasingDescriptor,
                        fieldDeclaration.Identifier.GetLocation()));
                }
            }
        }

        private void AnalyzeParameterName(SyntaxNodeAnalysisContext context)
        {
            var parameterNode = (ParameterSyntax)context.Node;
            
            if (!IsProbablyCamelCased(parameterNode.Identifier.Text))
            {
                context.ReportDiagnostic(Diagnostic.Create(ParameterNamesShouldUseCamcelCasingDescriptor,
                    parameterNode.Identifier.GetLocation()));
            }
        }

        private static bool IsProbablyPascalCased(string value)
        {
            char[] valueCharacters = value.ToCharArray();

            return char.IsUpper(value[0])
                && valueCharacters.All(char.IsLetterOrDigit)
                && (value.Length == 1 || valueCharacters.Any(char.IsLower));
        }

        private static bool IsProbablyCamelCased(string value)
        {
            char[] valueCharacters = value.ToCharArray();

            return char.IsLower(value[0])
                && valueCharacters.All(char.IsLetterOrDigit);
        }
    }
}