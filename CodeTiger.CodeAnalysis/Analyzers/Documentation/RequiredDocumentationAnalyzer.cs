﻿using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Documentation
{
    /// <summary>
    /// Analyzes elements for the presence of required documentation comments.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RequiredDocumentationAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor
            ParametersOfExternallyAccessibleMembersShouldBeDocumentedDescriptor = new DiagnosticDescriptor(
                "CT3600", "Parameters of externally accessible members should be documented.",
                "Parameters of externally accessible members should be documented.", "CodeTiger.Documentation",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            ReturnValuesOfExternallyAccessibleMembersShouldBeDocumentedDescriptor = new DiagnosticDescriptor(
                "CT3601", "Return values of externally accessible members should be documented.",
                "Return values of externally accessible members should be documented.", "CodeTiger.Documentation",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(ParametersOfExternallyAccessibleMembersShouldBeDocumentedDescriptor,
                    ReturnValuesOfExternallyAccessibleMembersShouldBeDocumentedDescriptor);
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

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeParameterDocumentation, SyntaxKind.ConstructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration, SyntaxKind.DelegateDeclaration,
                SyntaxKind.IndexerDeclaration, SyntaxKind.MethodDeclaration,
                SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeReturnValueDocumentation,
                SyntaxKind.ConversionOperatorDeclaration, SyntaxKind.DelegateDeclaration,
                SyntaxKind.IndexerDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.OperatorDeclaration);
        }

        private static void AnalyzeParameterDocumentation(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.Options.DocumentationMode >= DocumentationMode.Diagnose
                && IsExternallyAccessible(context)
                && HasParameters(context.Node)
                && IsMissingParameterDocumentation(context))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ParametersOfExternallyAccessibleMembersShouldBeDocumentedDescriptor,
                    GetIdentifierLocation(context.Node)));
            }
        }

        private static void AnalyzeReturnValueDocumentation(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.Options.DocumentationMode >= DocumentationMode.Diagnose
                && IsExternallyAccessible(context)
                && HasNonVoidReturnType(context)
                && IsMissingReturnValueDocumentation(context))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ReturnValuesOfExternallyAccessibleMembersShouldBeDocumentedDescriptor,
                    GetIdentifierLocation(context.Node)));
            }
        }

        private static bool IsExternallyAccessible(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken);

            return symbol.IsExternallyAccessible();
        }

        private static bool HasParameters(SyntaxNode node)
        {
            BaseParameterListSyntax parameterList;

            switch (node.Kind())
            {
                case SyntaxKind.ConstructorDeclaration:
                    parameterList = ((ConstructorDeclarationSyntax)node).ParameterList;
                    break;
                case SyntaxKind.ConversionOperatorDeclaration:
                    parameterList = ((ConversionOperatorDeclarationSyntax)node).ParameterList;
                    break;
                case SyntaxKind.DelegateDeclaration:
                    parameterList = ((DelegateDeclarationSyntax)node).ParameterList;
                    break;
                case SyntaxKind.IndexerDeclaration:
                    parameterList = ((IndexerDeclarationSyntax)node).ParameterList;
                    break;
                case SyntaxKind.MethodDeclaration:
                    parameterList = ((MethodDeclarationSyntax)node).ParameterList;
                    break;
                case SyntaxKind.OperatorDeclaration:
                    parameterList = ((OperatorDeclarationSyntax)node).ParameterList;
                    break;
                default:
                    return false;
            }

            return parameterList?.Parameters.Any() ?? false;
        }

        private static bool HasNonVoidReturnType(SyntaxNodeAnalysisContext context)
        {
            TypeSyntax returnType;

            switch (context.Node.Kind())
            {
                case SyntaxKind.ConversionOperatorDeclaration:
                    returnType = ((ConversionOperatorDeclarationSyntax)context.Node).Type;
                    break;
                case SyntaxKind.DelegateDeclaration:
                    returnType = ((DelegateDeclarationSyntax)context.Node).ReturnType;
                    break;
                case SyntaxKind.IndexerDeclaration:
                    returnType = ((IndexerDeclarationSyntax)context.Node).Type;
                    break;
                case SyntaxKind.MethodDeclaration:
                    returnType = ((MethodDeclarationSyntax)context.Node).ReturnType;
                    break;
                case SyntaxKind.OperatorDeclaration:
                    returnType = ((OperatorDeclarationSyntax)context.Node).ReturnType;
                    break;
                default:
                    return false;
            }

            var type = context.SemanticModel.GetTypeInfo(returnType, context.CancellationToken).Type;

            return type?.SpecialType != SpecialType.System_Void;
        }

        private static bool IsMissingParameterDocumentation(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken);

            try
            {
                var documentationXml = symbol.GetDocumentationCommentXml(null, true, context.CancellationToken);

                // If the documentation does not exists, ignore it for this diagnostic.
                if (string.IsNullOrWhiteSpace(documentationXml))
                {
                    return false;
                }

                var documentation = XDocument.Parse(documentationXml);

                return documentation.Root.Elements().Any()
                    && !documentation.Root.Elements("param").Any();
            }
            catch (XmlException)
            {
                // If the documentation contains invalid XML, ignore it for this diagnostic.
                return false;
            }
        }

        private static bool IsMissingReturnValueDocumentation(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken);

            try
            {
                var documentationXml = symbol.GetDocumentationCommentXml(null, true, context.CancellationToken);

                // If the documentation does not exists, ignore it for this diagnostic.
                if (string.IsNullOrWhiteSpace(documentationXml))
                {
                    return false;
                }

                var documentation = XDocument.Parse(documentationXml);

                return documentation.Root.Elements().Any()
                    && !documentation.Root.Elements("returns").Any();
            }
            catch (XmlException)
            {
                // If the documentation contains invalid XML, ignore it for this diagnostic.
                return false;
            }
        }

        private static Location GetIdentifierLocation(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ConstructorDeclaration:
                    return ((ConstructorDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.ConversionOperatorDeclaration:
                    return ((ConversionOperatorDeclarationSyntax)node).Type.GetLocation();
                case SyntaxKind.DelegateDeclaration:
                    return ((DelegateDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.IndexerDeclaration:
                    return ((IndexerDeclarationSyntax)node).ThisKeyword.GetLocation();
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).Identifier.GetLocation();
                case SyntaxKind.OperatorDeclaration:
                    return ((OperatorDeclarationSyntax)node).OperatorToken.GetLocation();
                default:
                    return null;
            }
        }
    }
}