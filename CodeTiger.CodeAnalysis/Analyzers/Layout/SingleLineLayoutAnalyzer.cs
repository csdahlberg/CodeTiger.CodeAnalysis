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
                    NonAutoPropertiesShouldNotBeDefinedOnASingleLineDescriptor);
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
    }
}
