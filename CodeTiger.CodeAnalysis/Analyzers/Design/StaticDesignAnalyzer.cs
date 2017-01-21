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
            var classTypeSymbol = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);

            if (classTypeSymbol.IsAbstract || classTypeSymbol.IsStatic)
            {
                // The class is abstract or already static, and so cannot be made static
                return;
            }

            if (classTypeSymbol.Interfaces.Any())
            {
                // The class implements an interface, and so cannot be made static
                return;
            }

            var classMembers = classTypeSymbol.GetMembers();
            if (!classMembers.Any(x => !x.IsImplicitlyDeclared)
                || classMembers.Any(x => !x.IsStatic && !x.IsImplicitlyDeclared))
            {
                // The class either does not have any members (and so should not be made static) or has some
                // non-static members (and so cannot be made static).
                return;
            }

            var objectTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Object");
            if (!classTypeSymbol.BaseType.Equals(objectTypeSymbol))
            {
                // The class inherits from a class other than System.Object, and so cannot be made static
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(ClassesWithAllStaticMembersShouldBeStaticDescriptor,
                node.Identifier.GetLocation()));
        }
    }
}