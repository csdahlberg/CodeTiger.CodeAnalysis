using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design
{
    /// <summary>
    /// Analyzes copy constructors for design issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CopyConstructorDesignAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor CopyConstructorsShouldNotBePublicDescriptor
            = new DiagnosticDescriptor("CT1001", "Copy constructors should not be public.",
                "Copy constructors should not be public.", "CodeTiger.Design", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(CopyConstructorsShouldNotBePublicDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeCopyConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalyzeCopyConstructor(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            var constructorSymbol = context.SemanticModel.GetDeclaredSymbol(constructorDeclaration,
                context.CancellationToken);

            if (IsProbablyCopyConstructor(context, constructorDeclaration, constructorSymbol)
                && constructorSymbol.DeclaredAccessibility == Accessibility.Public)
            {
                context.ReportDiagnostic(Diagnostic.Create(CopyConstructorsShouldNotBePublicDescriptor,
                    constructorDeclaration.Identifier.GetLocation()));
            }
        }

        private static bool IsProbablyCopyConstructor(SyntaxNodeAnalysisContext context,
            ConstructorDeclarationSyntax constructorDeclaration, IMethodSymbol constructorSymbol)
        {
            if (constructorSymbol.Parameters.Count() != 1)
            {
                return false;
            }

            var parameterSymbol = constructorSymbol.Parameters.Single();
            if (!parameterSymbol?.Type?.Equals(constructorSymbol.ContainingType) ?? false)
            {
                return false;
            }

            return constructorDeclaration.Body?.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Any(x => IsAssigningToSameProperty(context, x)) ?? false;
        }

        private static bool IsAssigningToSameProperty(SyntaxNodeAnalysisContext context,
            AssignmentExpressionSyntax assignmentExpression)
        {
            return context.SemanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol
                ?.Equals(context.SemanticModel.GetSymbolInfo(assignmentExpression.Right).Symbol) ?? false;
        }
    }
}