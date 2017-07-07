using System;
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
        internal static readonly DiagnosticDescriptor PrivateFieldNamesShouldUseCamelCasingDescriptor
            = new DiagnosticDescriptor("CT1704",
                "Private field names should use camel casing with a leading underscore.",
                "Private field names should use camel casing with a leading underscore.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ParameterNamesShouldUseCamelCasingDescriptor
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
                    PrivateFieldNamesShouldUseCamelCasingDescriptor,
                    ParameterNamesShouldUseCamelCasingDescriptor);
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
            context.RegisterSyntaxNodeAction(AnalyzeFieldName, SyntaxKind.FieldDeclaration);
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
                    typeIdentifier = ((InterfaceDeclarationSyntax)context.Node).Identifier;
                    break;
                case SyntaxKind.EnumDeclaration:
                    typeIdentifier = ((EnumDeclarationSyntax)context.Node).Identifier;
                    break;
                default:
                    return;
            }

            if ((NamingUtility.IsProbablyPascalCased(typeIdentifier.Text) == false)
                && !(context.Node.Kind() == SyntaxKind.InterfaceDeclaration
                    && typeIdentifier.Text[0] == 'I'
                    && (NamingUtility.IsProbablyPascalCased(typeIdentifier.Text.Substring(1)) == true)))
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeNamesShouldUsePascalCasingDescriptor,
                    typeIdentifier.GetLocation()));
            }
        }

        private void AnalyzeFieldName(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclarationNode = (FieldDeclarationSyntax)context.Node;
            if (fieldDeclarationNode.Modifiers.Any(x => x.Kind() == SyntaxKind.ConstKeyword))
            {
                foreach (var fieldDeclaration in fieldDeclarationNode.Declaration.Variables)
                {
                    if (NamingUtility.IsProbablyPascalCased(fieldDeclaration.Identifier.Text) == false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            ConstantFieldNamesShouldUsePascalCasingDescriptor,
                            fieldDeclaration.Identifier.GetLocation()));
                    }
                }
            }
            else if (fieldDeclarationNode.Modifiers.Any(x => x.Kind() == SyntaxKind.PrivateKeyword))
            {
                foreach (var fieldDeclaration in fieldDeclarationNode.Declaration.Variables)
                {
                    if (NamingUtility.IsProbablyCamelCased(fieldDeclaration.Identifier.Text, '_') == false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(PrivateFieldNamesShouldUseCamelCasingDescriptor,
                            fieldDeclaration.Identifier.GetLocation()));
                    }
                }
            }
        }

        private void AnalyzeParameterName(SyntaxNodeAnalysisContext context)
        {
            var parameterNode = (ParameterSyntax)context.Node;
            
            if (NamingUtility.IsProbablyCamelCased(parameterNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(ParameterNamesShouldUseCamelCasingDescriptor,
                    parameterNode.Identifier.GetLocation()));
            }
        }
    }
}