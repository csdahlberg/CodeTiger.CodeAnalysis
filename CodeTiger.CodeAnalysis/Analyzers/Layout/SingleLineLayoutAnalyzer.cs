using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeTiger.CodeAnalysis.Analyzers.Layout
{
    /// <summary>
    /// Analyzes elements for layout issues related to elements being defined on a single line.
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
        internal static readonly DiagnosticDescriptor LockStatementsShouldNotBeDefinedOnASingleLineDescriptor
            = new DiagnosticDescriptor("CT3523", "Lock statements should not be defined on a single line.",
                "Lock statements should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            NonTrivialSwitchSectionsShouldNotBeDefinedOnASingleLineDescriptor = new DiagnosticDescriptor("CT3524",
                "Non-trivial switch sections should not be defined on a single line.",
                "Non-trivial switch sections should not be defined on a single line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor MultipleStatementsShouldNotBeOnTheSameLineDescriptor
            = new DiagnosticDescriptor("CT3528", "Multiple statements should not be on the same line.",
                "Multiple statements should not be on the same line.", "CodeTiger.Layout",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            LinqQueryClausesShouldAllBeOnTheSameLineOrSeparateLinesDescriptor = new DiagnosticDescriptor("CT3529",
                "LINQ query clauses should all be on the same line or separate lines.",
                "LINQ query clauses should all be on the same line or separate lines.", "CodeTiger.Layout",
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
                    FixedStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    LockStatementsShouldNotBeDefinedOnASingleLineDescriptor,
                    NonTrivialSwitchSectionsShouldNotBeDefinedOnASingleLineDescriptor,
                    MultipleStatementsShouldNotBeOnTheSameLineDescriptor,
                    LinqQueryClausesShouldAllBeOnTheSameLineOrSeparateLinesDescriptor);
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
            context.RegisterSyntaxNodeAction(AnalyzeLockStatement, SyntaxKind.LockStatement);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchSection, SyntaxKind.SwitchSection);
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
            context.RegisterSyntaxNodeAction(AnalyzeQueryExpression, SyntaxKind.QueryExpression);
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

            var firstNonAttributeToken = node.ChildNodesAndTokens()
                .SkipWhile(x => x.Kind() == SyntaxKind.AttributeList)
                .FirstOrDefault();
            if (firstNonAttributeToken == null)
            {
                return;
            }

            var locationWithoutAttributes = Location.Create(context.Node.SyntaxTree,
                TextSpan.FromBounds(firstNonAttributeToken.Span.Start, node.Span.End));
            var nodeLineSpan = locationWithoutAttributes.GetLineSpan();
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

            var firstNonAttributeToken = node.ChildNodesAndTokens()
                .SkipWhile(x => x.Kind() == SyntaxKind.AttributeList)
                .FirstOrDefault();
            if (firstNonAttributeToken == null)
            {
                return;
            }

            var locationWithoutAttributes = Location.Create(context.Node.SyntaxTree,
                TextSpan.FromBounds(firstNonAttributeToken.Span.Start, node.Span.End));
            var nodeLineSpan = locationWithoutAttributes.GetLineSpan();

            if (node.ExpressionBody == null && node.AccessorList.Accessors.All(x => x.Body == null))
            {
                if (nodeLineSpan.Span.Start.Line != nodeLineSpan.Span.End.Line)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        AutoPropertiesShouldBeDefinedOnASingleLineDescriptor, node.Identifier.GetLocation()));
                }
            }
            else if (node.AccessorList.Accessors.Any(x => x.Body != null)
                && nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
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

            if (node.Body == null || node.ExpressionBody != null)
            {
                return;
            }

            var firstNonAttributeToken = node.ChildNodesAndTokens()
                .SkipWhile(x => x.Kind() == SyntaxKind.AttributeList)
                .FirstOrDefault();
            if (firstNonAttributeToken == null)
            {
                return;
            }

            var locationWithoutAttributes = Location.Create(context.Node.SyntaxTree,
                TextSpan.FromBounds(firstNonAttributeToken.Span.Start, node.Span.End));
            var nodeLineSpan = locationWithoutAttributes.GetLineSpan();
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

        private void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (LockStatementSyntax)context.Node;

            var nodeLineSpan = node.GetLocation().GetLineSpan();
            if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    LockStatementsShouldNotBeDefinedOnASingleLineDescriptor, node.LockKeyword.GetLocation()));
            }
        }

        private void AnalyzeSwitchSection(SyntaxNodeAnalysisContext context)
        {
            var node = (SwitchSectionSyntax)context.Node;

            if (node.Statements.Count > 1
                || (node.Statements.Count == 1
                    && node.Statements[0].Kind() == SyntaxKind.Block
                    && IsBlockNonTrivial((BlockSyntax)node.Statements[0])))
            {
                var nodeLineSpan = node.GetLocation().GetLineSpan();
                if (nodeLineSpan.Span.Start.Line == nodeLineSpan.Span.End.Line)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        NonTrivialSwitchSectionsShouldNotBeDefinedOnASingleLineDescriptor,
                        node.Labels.Last().GetLocation()));
                }
            }
        }

        private void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var node = (BlockSyntax)context.Node;

            if (!node.Statements.Any())
            {
                return;
            }

            var previousLineSpan = node.Statements[0].GetLocation().GetLineSpan();

            for (int i = 1; i < node.Statements.Count; i += 1)
            {
                var currentLocation = node.Statements[i].GetLocation();
                var currentLineSpan = currentLocation.GetLineSpan();
                if (currentLineSpan.StartLinePosition.Line == previousLineSpan.EndLinePosition.Line)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        MultipleStatementsShouldNotBeOnTheSameLineDescriptor, currentLocation));
                }

                previousLineSpan = currentLineSpan;
            }
        }

        private void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context)
        {
            var node = (QueryExpressionSyntax)context.Node;

            var queryParts = new List<CSharpSyntaxNode>();

            if (node.FromClause != null)
            {
                queryParts.Add(node.FromClause);
            }

            if (node.Body != null)
            {
                queryParts.AddRange(node.Body.Clauses);

                if (node.Body.SelectOrGroup != null)
                {
                    queryParts.Add(node.Body.SelectOrGroup);
                }

                // A continuation begins with the "into" keyword followed by an identifier (for example, "into y"),
                // then the body of the continuation. This currently ignores the location of "into y" so that it
                // can be located either on the same line as the previous clause or on a new line, but it does
                // check any clauses in the continuation body.
                if (node.Body.Continuation?.Body != null)
                {
                    queryParts.AddRange(node.Body.Continuation.Body.Clauses);

                    if (node.Body.Continuation.Body.SelectOrGroup != null)
                    {
                        queryParts.Add(node.Body.Continuation.Body.SelectOrGroup);
                    }
                }
            }

            if (queryParts.Count <= 0)
            {
                return;
            }

            var partsWithLineSpans = queryParts
                .Select(x => new { Part = x, Location = x.GetLocation().GetLineSpan() })
                .ToList();
            var partsGroupedByStartLine = partsWithLineSpans
                .GroupBy(x => x.Location.StartLinePosition.Line)
                .ToList();

            // If all query parts start on the same line, they will all be put into a single grouping. If all query
            // parts start on a different line, they will all be in their own groupings, so the number of groupings
            // will equal the number of parts. Either case is allowable.
            if (partsGroupedByStartLine.Count != 1
                && partsWithLineSpans.Count != partsGroupedByStartLine.Count)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    LinqQueryClausesShouldAllBeOnTheSameLineOrSeparateLinesDescriptor, node.GetLocation()));
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

        private static bool IsOnNewLine(SyntaxNode node)
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
