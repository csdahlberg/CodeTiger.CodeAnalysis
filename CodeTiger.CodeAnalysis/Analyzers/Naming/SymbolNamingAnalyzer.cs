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
        internal static readonly DiagnosticDescriptor EventNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1705", "Event names should use pascal casing.",
                "Event names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor DelegateNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1706", "Delegate names should use pascal casing.",
                "Delegate names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor PropertyNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1707", "Property names should use pacal casing.",
                "Property names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor MethodNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1708", "Method names should use pascal casing.",
                "Method names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor EnumerationMemberNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1709", "Enumeration member names should use pascal casing.",
                "Enumeration member names should use pascal casing.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor VariableNamesShouldUseCamelCasingDescriptor
            = new DiagnosticDescriptor("CT1710", "Variable names should use camel casing.",
                "Variable names should use camel casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor InterfaceNamesShouldUsePascalCasingPrefixedWithIDescriptor
            = new DiagnosticDescriptor("CT1711", "Interface names should use pascal casing prefixed with 'I'.",
                "Interface names should use pascal casing prefixed with 'I'.", "CodeTiger.Naming",
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
                    EventNamesShouldUsePascalCasingDescriptor,
                    DelegateNamesShouldUsePascalCasingDescriptor,
                    PropertyNamesShouldUsePascalCasingDescriptor,
                    MethodNamesShouldUsePascalCasingDescriptor,
                    EnumerationMemberNamesShouldUsePascalCasingDescriptor,
                    VariableNamesShouldUseCamelCasingDescriptor,
                    InterfaceNamesShouldUsePascalCasingPrefixedWithIDescriptor,
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
                SyntaxKind.StructDeclaration, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeFieldName, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeEventFieldName, SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeDelegateName, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyName, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodName, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeEnumerationMemberName, SyntaxKind.EnumMemberDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLocalVariableNames, SyntaxKind.LocalDeclarationStatement);
            context.RegisterSyntaxNodeAction(AnalyzeInterfaceName, SyntaxKind.InterfaceDeclaration);
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
                case SyntaxKind.EnumDeclaration:
                    typeIdentifier = ((EnumDeclarationSyntax)context.Node).Identifier;
                    break;
                default:
                    return;
            }

            if (NamingUtility.IsProbablyPascalCased(typeIdentifier.Text) == false
                || NamingUtility.IsProbablyPascalCased(typeIdentifier.Text, 'I') == true)
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

        private void AnalyzeEventFieldName(SyntaxNodeAnalysisContext context)
        {
            var eventFieldDeclarationNode = (EventFieldDeclarationSyntax)context.Node;
            foreach (var eventFieldDeclaration in eventFieldDeclarationNode.Declaration.Variables)
            {
                if (NamingUtility.IsProbablyPascalCased(eventFieldDeclaration.Identifier.Text) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(EventNamesShouldUsePascalCasingDescriptor,
                        eventFieldDeclaration.Identifier.GetLocation()));
                }
            }
        }

        private void AnalyzeDelegateName(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclarationNode = (DelegateDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(delegateDeclarationNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(DelegateNamesShouldUsePascalCasingDescriptor,
                    delegateDeclarationNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzePropertyName(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclarationNode = (PropertyDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(propertyDeclarationNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyNamesShouldUsePascalCasingDescriptor,
                    propertyDeclarationNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzeMethodName(SyntaxNodeAnalysisContext context)
        {
            var methodDeclarationNode = (MethodDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(methodDeclarationNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(MethodNamesShouldUsePascalCasingDescriptor,
                    methodDeclarationNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzeEnumerationMemberName(SyntaxNodeAnalysisContext context)
        {
            var enumMemberDeclarationNode = (EnumMemberDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(enumMemberDeclarationNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(EnumerationMemberNamesShouldUsePascalCasingDescriptor,
                    enumMemberDeclarationNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzeLocalVariableNames(SyntaxNodeAnalysisContext context)
        {
            var localDeclarationSyntax = (LocalDeclarationStatementSyntax)context.Node;
            
            foreach (var variableDeclaratorSyntax in localDeclarationSyntax.Declaration.Variables)
            {
                if (NamingUtility.IsProbablyCamelCased(variableDeclaratorSyntax.Identifier.Text) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(VariableNamesShouldUseCamelCasingDescriptor,
                        variableDeclaratorSyntax.Identifier.GetLocation()));
                }
            }
        }

        private void AnalyzeInterfaceName(SyntaxNodeAnalysisContext context)
        {
            var interfaceDeclarationNode = (InterfaceDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(interfaceDeclarationNode.Identifier.Text, 'I') == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InterfaceNamesShouldUsePascalCasingPrefixedWithIDescriptor,
                    interfaceDeclarationNode.Identifier.GetLocation()));
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