using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design
{
    /// <summary>
    /// Analyzes use of the static modifier.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticDesignAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor ClassesWithAllStaticMembersShouldBeStaticDescriptor
            = new DiagnosticDescriptor("CT1000", "Classes with all static members should be static.",
                "Classes with all static members should be static.", "CodeTiger.Design",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(ClassesWithAllStaticMembersShouldBeStaticDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeClassForAllStaticMembers, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassForAllStaticMembers(SyntaxNodeAnalysisContext context)
        {
            var node = (ClassDeclarationSyntax)context.Node;

            if (!node.Modifiers.Any(x => x.Kind() == SyntaxKind.StaticKeyword))
            {
                var classTypeSymbol = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);

                // The implicit default constructor is returned by GetMembers(). To ignore such constructors, any
                // implicitly declared methods will not be considered instance methods.
                if (classTypeSymbol.Interfaces.Any()
                    || classTypeSymbol.GetMembers()
                        .Any(x => !x.IsStatic && (x.Kind != SymbolKind.Method || !x.IsImplicitlyDeclared)))
                {
                    return;
                }

                var objectTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Object");
                if (classTypeSymbol.BaseType.Equals(objectTypeSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ClassesWithAllStaticMembersShouldBeStaticDescriptor,
                        node.Identifier.GetLocation()));
                }
            }
        }
    }
}