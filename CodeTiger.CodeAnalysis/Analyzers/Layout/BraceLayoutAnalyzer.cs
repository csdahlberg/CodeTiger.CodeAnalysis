using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout
{
    /// <summary>
    /// Analyzes braces for layout issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BraceLayoutAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor NamespacesShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3500", "Namespaces should not be defined on a single line.",
                "Namespaces should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor BracesForMultiLineElementsShouldBeOnANewLineDescriptor
            = new DiagnosticDescriptor("CT3501", "Braces for multi-line elements should be on a new line.",
                "Braces for multi-line elements should be on a new line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor TypesShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3502", "Types should not be defined on a single line.",
                "Types should not be defined on a single line.", "CodeTiger.Layout", DiagnosticSeverity.Warning,
                true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(NamespacesShouldNotBeDefinedOnASingleLineDescriptor,
                    BracesForMultiLineElementsShouldBeOnANewLineDescriptor,
                    TypesShouldNotBeDefinedOnASingleLineDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeBracesForNamespaceDeclaration,
                SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeBracesForComplexElementInitializerExpression,
                SyntaxKind.ComplexElementInitializerExpression);
            context.RegisterSyntaxNodeAction(AnalyzeBracesForArrayCreationExpression,
                SyntaxKind.ArrayCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeBracesForObjectCreationExpression,
                SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeBracesForAnonymousObjectCreationExpression,
                SyntaxKind.AnonymousObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeBracesForBlock, SyntaxKind.Block);
            context.RegisterSyntaxNodeAction(AnalyzeBracesForSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(AnalyzeBracesForTypeDeclaration, SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeBracesForAccessorList, SyntaxKind.AccessorList);
        }

        private void AnalyzeBracesForNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (NamespaceDeclarationSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.StartLinePosition.Line == nodeLineSpan.EndLinePosition.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(NamespacesShouldNotBeDefinedOnASingleLineDescriptor,
                    node.Name.GetLocation()));
            }
            else
            {
                AnalyzeBraces(nodeLineSpan, node.OpenBraceToken, node.CloseBraceToken, context);
            }
        }

        private void AnalyzeBracesForArrayCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (ArrayCreationExpressionSyntax)context.Node;

            if (node.Initializer != null)
            {
                AnalyzeBraces(node.GetLocation().GetLineSpan(), node.Initializer.OpenBraceToken,
                    node.Initializer.CloseBraceToken, context);
            }
        }

        private void AnalyzeBracesForObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (ObjectCreationExpressionSyntax)context.Node;

            if (node.Initializer != null)
            {
                AnalyzeBraces(node.GetLocation().GetLineSpan(), node.Initializer.OpenBraceToken,
                    node.Initializer.CloseBraceToken, context);
            }
        }

        private void AnalyzeBracesForComplexElementInitializerExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (InitializerExpressionSyntax)context.Node;

            AnalyzeBraces(node.GetLocation().GetLineSpan(), node.OpenBraceToken, node.CloseBraceToken, context);
        }

        private void AnalyzeBracesForAnonymousObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (AnonymousObjectCreationExpressionSyntax)context.Node;

            AnalyzeBraces(node.GetLocation().GetLineSpan(), node.OpenBraceToken, node.CloseBraceToken, context);
        }

        private void AnalyzeBracesForBlock(SyntaxNodeAnalysisContext context)
        {
            var node = (BlockSyntax)context.Node;

            FileLinePositionSpan nodeLinePositionSpan;
            switch (node.Parent?.Kind())
            {
                case SyntaxKind.IfStatement:
                case SyntaxKind.ElseClause:
                case SyntaxKind.AccessorList:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                    nodeLinePositionSpan = node.Parent.GetLocation().GetLineSpan();
                    break;
                default:
                    nodeLinePositionSpan = node.GetLocation().GetLineSpan();
                    break;
            }

            AnalyzeBraces(nodeLinePositionSpan, node.OpenBraceToken, node.CloseBraceToken, context);
        }

        private void AnalyzeBracesForSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (SwitchStatementSyntax)context.Node;

            AnalyzeBraces(node.GetLocation().GetLineSpan(), node.OpenBraceToken, node.CloseBraceToken, context);
        }

        private void AnalyzeBracesForTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (BaseTypeDeclarationSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.StartLinePosition.Line == nodeLineSpan.EndLinePosition.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(TypesShouldNotBeDefinedOnASingleLineDescriptor,
                    node.Identifier.GetLocation()));
            }
            else
            {
                AnalyzeBraces(nodeLineSpan, node.OpenBraceToken, node.CloseBraceToken, context);
            }
        }

        private void AnalyzeBracesForAccessorList(SyntaxNodeAnalysisContext context)
        {
            var node = (AccessorListSyntax)context.Node;

            AnalyzeBraces(node.GetLocation().GetLineSpan(), node.OpenBraceToken, node.CloseBraceToken, context);
        }

        private void AnalyzeBraces(FileLinePositionSpan nodeLineSpan, SyntaxToken openBraceToken,
            SyntaxToken closeBraceToken, SyntaxNodeAnalysisContext context)
        {
            if (nodeLineSpan.StartLinePosition.Line == nodeLineSpan.EndLinePosition.Line)
            {
                return;
            }

            if (!IsOnOwnLine(openBraceToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(BracesForMultiLineElementsShouldBeOnANewLineDescriptor,
                    openBraceToken.GetLocation()));
            }

            if (!IsOnOwnLine(closeBraceToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(BracesForMultiLineElementsShouldBeOnANewLineDescriptor,
                    closeBraceToken.GetLocation()));
            }
        }

        private bool IsOnOwnLine(SyntaxToken braceToken)
        {
            var braceTokenLineSpan = braceToken.GetLocation().GetLineSpan();

            var previousToken = braceToken.GetPreviousToken();
            while (previousToken != default(SyntaxToken))
            {
                var previousTokenLineSpan = previousToken.GetLocation().GetLineSpan();
                if (previousTokenLineSpan.EndLinePosition.Line != braceTokenLineSpan.StartLinePosition.Line)
                {
                    break;
                }

                if (!SyntaxFacts.IsTrivia(previousToken.Kind())
                    && previousToken.Kind() != SyntaxKind.SemicolonToken
                    && previousToken.Kind() != SyntaxKind.ColonToken)
                {
                    return false;
                }

                previousToken = previousToken.GetPreviousToken();
            }

            var nextToken = braceToken.GetNextToken();
            while (nextToken != default(SyntaxToken))
            {
                var nextTokenLineSpan = nextToken.GetLocation().GetLineSpan();
                if (nextTokenLineSpan.StartLinePosition.Line != braceTokenLineSpan.EndLinePosition.Line)
                {
                    break;
                }

                if (!SyntaxFacts.IsTrivia(nextToken.Kind()) && nextToken.Kind() != SyntaxKind.SemicolonToken)
                {
                    return false;
                }

                nextToken = nextToken.GetNextToken();
            }

            return true;
        }
    }
}
