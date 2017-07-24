using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Usage
{
    /// <summary>
    /// Analyzes usage of the <see cref="Environment"/> class.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnvironmentUsageAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor EnvironmentExitShouldNotBeUsedDescriptor
            = new DiagnosticDescriptor("CT2203", "Environment.Exit should not be used.",
                "Environment.Exit should not be used.", "CodeTiger.Usage", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(EnvironmentExitShouldNotBeUsedDescriptor);
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

            context.RegisterSemanticModelAction(AnalyzeEnvironmentUsage);
        }

        private void AnalyzeEnvironmentUsage(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);
            var environmentType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Environment");
            if (environmentType == null)
            {
                return;
            }

            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations
                .Where(x => x.Expression.Kind() == SyntaxKind.SimpleMemberAccessExpression))
            {
                var memberAccessExpression = (MemberAccessExpressionSyntax)invocation.Expression;

                var targetType = context.SemanticModel.GetTypeInfo(memberAccessExpression.Expression);
                if (targetType.Type == environmentType && memberAccessExpression?.Name?.Identifier.Text == "Exit")
                {
                    context.ReportDiagnostic(Diagnostic.Create(EnvironmentExitShouldNotBeUsedDescriptor,
                        memberAccessExpression.Expression.GetLocation()));
                }
            }
        }
    }
}
