﻿using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout
{
    /// <summary>
    /// Analyzes braces for layout issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SingleLineLayoutAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor NamespacesShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3500", "Namespaces should not be defined on a single line.",
                "Namespaces should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor TypesShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3502", "Types should not be defined on a single line.",
                "Types should not be defined on a single line.", "CodeTiger.Layout", DiagnosticSeverity.Warning,
                true);
        internal static readonly DiagnosticDescriptor AutoPropertiesShouldBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3503", "Auto properties should be defined on a single line.",
                "Auto properties should be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor NonAutoPropertiesShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3504", "Non-auto properties should not be defined on a single line.",
                "Non-auto properties should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor TrivialAccessorsShouldBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3505", "Trivial accessors should be defined on a single line.",
                "Trivial accessors should be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor NonTrivialAccessorsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3506", "Non-trivial accessors should not be defined on a single line.",
                "Non-trivial accessors should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor MethodsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3507", "Methods should not be defined on a single line.",
                "Methods should not be defined on a single line.", "CodeTiger.Layout", DiagnosticSeverity.Warning,
                true);
        internal static readonly DiagnosticDescriptor TryStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3508", "Try statements should not be defined on a single line.",
                "Try statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            NonTrivialCatchClausesShouldNotBeDefinedOnASingleLineDescriptor = new DiagnosticDescriptor("CT3509",
                "Non-trivial catch clauses should not be defined on a single line.",
                "Non-trivial catch clauses should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor CatchClausesShouldBeginOnANewLineDescriptor
            = new DiagnosticDescriptor("CT3510", "Catch clauses should begin on a new line.",
                "Catch clauses should begin on a new line.", "CodeTiger.Layout", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor FinallyClausesShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3511", "Finally clauses should not be defined on a single line.",
                "Finally clauses should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor FinallyClausesShouldBeginOnANewLineDescriptor
            = new DiagnosticDescriptor("CT3512", "Finally clauses should begin on a new line.",
                "Finally clauses should begin on a new line.", "CodeTiger.Layout", DiagnosticSeverity.Warning,
                true);
        internal static readonly DiagnosticDescriptor IfStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3513", "If statements should not be defined on a single line.",
                "If statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ElseClausesShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3514", "Else clauses should not be defined on a single line.",
                "Else clauses should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ElseClausesShouldBeginOnANewLineDescriptor
            = new DiagnosticDescriptor("CT3515", "Else clauses should begin on a new line.",
                "Else clauses should begin on a new line.", "CodeTiger.Layout", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ForStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3516", "For statements should not be defined on a single line.",
                "For statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ForEachStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3517", "ForEach statements should not be defined on a single line.",
                "ForEach statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor SwitchStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3518", "Switch statements should not be defined on a single line.",
                "Switch statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor WhileStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3519", "While statements should not be defined on a single line.",
                "While statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor DoStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3520", "Do statements should not be defined on a single line.",
                "Do statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            NonEmptyUsingStatementsShouldNotBeDefinedOnASingleLineDescriptor = new DiagnosticDescriptor("CT3521",
                "Non-empty using statements should not be defined on a single line.",
                "Non-empty using statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor FixedStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3522", "Fixed statements should not be defined on a single line.",
                "Fixed statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(NamespacesShouldNotBeDefinedOnASingleLineDescriptor,
                    TypesShouldNotBeDefinedOnASingleLineDescriptor,
                    AutoPropertiesShouldBeDefinedOnASingleLineDescriptor,
                    NonAutoPropertiesShouldNotBeDefinedOnASingleLineDescriptor,
                    TrivialAccessorsShouldBeDefinedOnASingleLineDescriptor,
                    NonTrivialAccessorsShouldNotBeDefinedOnASingleLineDescriptor,
                    MethodsShouldNotBeDefinedOnASingleLineDescriptor,
                    TryStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    NonTrivialCatchClausesShouldNotBeDefinedOnASingleLineDescriptor,
                    CatchClausesShouldBeginOnANewLineDescriptor,
                    FinallyClausesShouldNotBeDefinedOnASingleLineDescriptor,
                    FinallyClausesShouldBeginOnANewLineDescriptor,
                    IfStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    ElseClausesShouldNotBeDefinedOnASingleLineDescriptor,
                    ElseClausesShouldBeginOnANewLineDescriptor,
                    ForStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    ForEachStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    SwitchStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    WhileStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    DoStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    NonEmptyUsingStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    FixedStatementsShouldNotBeDefinedOnASingleLineDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration, SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeTryStatement, SyntaxKind.TryStatement);
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
            context.RegisterSyntaxNodeAction(AnalyzeFinallyClause, SyntaxKind.FinallyClause);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);
            context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(AnalyzeDoStatement, SyntaxKind.DoStatement);
            context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
            context.RegisterSyntaxNodeAction(AnalyzeFixedStatement, SyntaxKind.FixedStatement);
        }

        private void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (NamespaceDeclarationSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.StartLinePosition.Line == nodeLineSpan.EndLinePosition.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(NamespacesShouldNotBeDefinedOnASingleLineDescriptor,
                    node.Name.GetLocation()));
            }
        }

        private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (BaseTypeDeclarationSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.StartLinePosition.Line == nodeLineSpan.EndLinePosition.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(TypesShouldNotBeDefinedOnASingleLineDescriptor,
                    node.Identifier.GetLocation()));
            }
        }

        private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (PropertyDeclarationSyntax)context.Node;
            
            if (node.AccessorList?.Accessors == null)
            {
                return;
            }

            var nodeLineSpan = node.GetLocation().GetLineSpan();

            if (node.AccessorList.Accessors.All(x => x.Body == null))
            {
                if (nodeLineSpan.Span.Start.Line != nodeLineSpan.Span.End.Line)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        AutoPropertiesShouldBeDefinedOnASingleLineDescriptor, node.Identifier.GetLocation()));
                }
            }
            else if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    NonAutoPropertiesShouldNotBeDefinedOnASingleLineDescriptor, node.Identifier.GetLocation()));
            }
        }

        private void AnalyzeAccessorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (AccessorDeclarationSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();

            if (IsAccessorTrivial(node.Body))
            {
                if (nodeLineSpan.Span.Start.Line != nodeLineSpan.Span.End.Line)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        TrivialAccessorsShouldBeDefinedOnASingleLineDescriptor, node.Keyword.GetLocation()));
                }
            }
            else if (IsAccessorNonTrivial(node.Body))
            {
                if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        NonTrivialAccessorsShouldNotBeDefinedOnASingleLineDescriptor, node.Keyword.GetLocation()));
                }
            }
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (MethodDeclarationSyntax)context.Node;

            if (node.ExpressionBody != null)
            {
                return;
            }

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(MethodsShouldNotBeDefinedOnASingleLineDescriptor,
                    node.Identifier.GetLocation()));
            }
        }

        private void AnalyzeTryStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (TryStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(TryStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    node.TryKeyword.GetLocation()));
            }
        }

        private void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var node = (CatchClauseSyntax)context.Node;
            
            if (IsBlockNonTrivial(node.Block))
            {
                var nodeLineSpan = node.GetLocation().GetLineSpan();
                if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        NonTrivialCatchClausesShouldNotBeDefinedOnASingleLineDescriptor,
                        node.CatchKeyword.GetLocation()));
                }
            }

            if (!IsOnNewLine(node))
            {
                context.ReportDiagnostic(Diagnostic.Create(CatchClausesShouldBeginOnANewLineDescriptor,
                    node.CatchKeyword.GetLocation()));
            }
        }

        private void AnalyzeFinallyClause(SyntaxNodeAnalysisContext context)
        {
            var node = (FinallyClauseSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(FinallyClausesShouldNotBeDefinedOnASingleLineDescriptor,
                    node.FinallyKeyword.GetLocation()));
            }

            if (!IsOnNewLine(node))
            {
                context.ReportDiagnostic(Diagnostic.Create(FinallyClausesShouldBeginOnANewLineDescriptor,
                    node.FinallyKeyword.GetLocation()));
            }
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (IfStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.StartLinePosition.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(IfStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    node.IfKeyword.GetLocation()));
            }
        }

        private void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var node = (ElseClauseSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(ElseClausesShouldNotBeDefinedOnASingleLineDescriptor,
                    node.ElseKeyword.GetLocation()));
            }

            if (!IsOnNewLine(node))
            {
                context.ReportDiagnostic(Diagnostic.Create(ElseClausesShouldBeginOnANewLineDescriptor,
                    node.ElseKeyword.GetLocation()));
            }
        }

        private void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(ForStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    node.ForKeyword.GetLocation()));
            }
        }

        private void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForEachStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ForEachStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    node.ForEachKeyword.GetLocation()));
            }
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (SwitchStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    SwitchStatementsShouldNotBeDefinedOnASingleLineDescriptor, node.SwitchKeyword.GetLocation()));
            }
        }

        private void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (WhileStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    WhileStatementsShouldNotBeDefinedOnASingleLineDescriptor, node.WhileKeyword.GetLocation()));
            }
        }

        private void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (DoStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DoStatementsShouldNotBeDefinedOnASingleLineDescriptor, node.DoKeyword.GetLocation()));
            }
        }

        private void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (UsingStatementSyntax)context.Node;

            if (node.Statement == null || node.Statement.Kind() == SyntaxKind.EmptyStatement)
            {
                return;
            }

            if (node.Statement.Kind() == SyntaxKind.Block)
            {
                var blockNode = (BlockSyntax)node.Statement;
                if (blockNode.Statements.Count == 0)
                {
                    return;
                }
            }

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    NonEmptyUsingStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    node.UsingKeyword.GetLocation()));
            }
        }

        private void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (FixedStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    FixedStatementsShouldNotBeDefinedOnASingleLineDescriptor, node.FixedKeyword.GetLocation()));
            }
        }

        private static bool IsAccessorTrivial(BlockSyntax body)
        {
            if (body == null || body.Statements.Count == 0)
            {
                return true;
            }

            if (body.Statements.Count != 1)
            {
                return false;
            }

            var statement = body.Statements[0];

            if (statement.Span.Length > 80)
            {
                return false;
            }

            switch (statement.Kind())
            {
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.ExpressionStatement:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsAccessorNonTrivial(BlockSyntax body)
        {
            if (body == null || body.Statements.Count == 0)
            {
                return false;
            }

            if (body?.Statements.Count > 1)
            {
                return true;
            }

            switch (body.Statements[0].Kind())
            {
                case SyntaxKind.IfStatement:
                case SyntaxKind.SwitchStatement:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsBlockNonTrivial(BlockSyntax block)
        {
            if (block == null || block.Statements.Count == 0)
            {
                return false;
            }

            if (block.Statements.Count > 1)
            {
                return true;
            }

            switch (block.Statements[0].Kind())
            {
                case SyntaxKind.ThrowStatement:
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.YieldReturnStatement:
                case SyntaxKind.BreakStatement:
                case SyntaxKind.YieldBreakStatement:
                case SyntaxKind.GotoStatement:
                case SyntaxKind.GotoCaseStatement:
                case SyntaxKind.GotoDefaultStatement:
                case SyntaxKind.InvocationExpression:
                    return false;
                default:
                    return true;
            }
        }

        private bool IsOnNewLine(SyntaxNode node)
        {
            var nodeLineSpan = node.GetLocation().GetLineSpan();

            var previousToken = node.ChildTokens().FirstOrDefault().GetPreviousToken();
            while (previousToken != default(SyntaxToken))
            {
                var previousTokenLineSpan = previousToken.GetLocation().GetLineSpan();
                if (previousTokenLineSpan.EndLinePosition.Line != nodeLineSpan.StartLinePosition.Line)
                {
                    break;
                }

                if (!SyntaxFacts.IsTrivia(previousToken.Kind()))
                {
                    return false;
                }

                previousToken = previousToken.GetPreviousToken();
            }

            return true;
        }
    }
}
