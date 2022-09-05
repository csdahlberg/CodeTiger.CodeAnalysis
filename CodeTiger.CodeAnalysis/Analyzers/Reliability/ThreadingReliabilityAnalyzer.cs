using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability;

/// <summary>
/// Analyzes code related to threading for potential reliability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ThreadingReliabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor ThreadResetAbortShouldNotBeUsedDescriptor
        = new DiagnosticDescriptor("CT2005", "Thread.ResetAbort should not be used",
            "Thread.ResetAbort should not be used", "CodeTiger.Reliability", DiagnosticSeverity.Warning,
            true);
    internal static readonly DiagnosticDescriptor
        ThreadSynchronizationShouldNotBeDoneUsingAPubliclyAccessibleObjectDescriptor
        = new DiagnosticDescriptor("CT2006",
            "Thread synchronization should not be done using a publicly accessible object",
            "Thread synchronization should not be done using a publicly accessible object",
            "CodeTiger.Reliability", DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(ThreadResetAbortShouldNotBeUsedDescriptor,
                ThreadSynchronizationShouldNotBeDoneUsingAPubliclyAccessibleObjectDescriptor);
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

        context.RegisterSemanticModelAction(AnalyzeThreadResetAbortUsage);
        context.RegisterSemanticModelAction(AnalyzeThreadSynchronizationObjects);
    }

    private static void AnalyzeThreadResetAbortUsage(SemanticModelAnalysisContext context)
    {
        var threadType = context.SemanticModel.Compilation?.GetTypeByMetadataName("System.Threading.Thread");

        if (threadType == null)
        {
            return;
        }

        var resetAbortSymbols = threadType.GetMembers("ResetAbort");

        if (resetAbortSymbols.IsDefaultOrEmpty)
        {
            return;
        }

        var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var invokedSymbol = context.SemanticModel
                .GetSymbolInfo(invocation.Expression, context.CancellationToken).Symbol
                    ?? context.SemanticModel
                        .GetDeclaredSymbol(invocation.Expression, context.CancellationToken);

            if (resetAbortSymbols.Contains(invokedSymbol, SymbolEqualityComparer.Default))
            {
                context.ReportDiagnostic(Diagnostic.Create(ThreadResetAbortShouldNotBeUsedDescriptor,
                    invocation.Expression.GetLocation()));
            }
        }
    }

    private static void AnalyzeThreadSynchronizationObjects(SemanticModelAnalysisContext context)
    {
        var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

        foreach (var node in root.DescendantNodes())
        {
            SyntaxNode lockObjectNode;

            switch (node.Kind())
            {
                case SyntaxKind.LockStatement:
                    lockObjectNode = ((LockStatementSyntax)node).Expression;
                    break;
                case SyntaxKind.InvocationExpression:
                    // TODO: Expand this to work with thread synchronization classes (Monitor, etc.)
                    lockObjectNode = null;
                    break;
                default:
                    lockObjectNode = null;
                    break;
            }

            if (lockObjectNode != null)
            {
                if (IsPubliclyAccessible(context, lockObjectNode))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        ThreadSynchronizationShouldNotBeDoneUsingAPubliclyAccessibleObjectDescriptor,
                        lockObjectNode.GetLocation()));
                }
            }
        }
    }

    private static bool IsPubliclyAccessible(SemanticModelAnalysisContext context, SyntaxNode node)
    {
        if (node.Kind() == SyntaxKind.ThisExpression || node.Kind() == SyntaxKind.BaseExpression)
        {
            return true;
        }

        if (node.Kind() == SyntaxKind.SimpleMemberAccessExpression
            || node.Kind() == SyntaxKind.PointerMemberAccessExpression)
        {
            var memberAccessExpression = (MemberAccessExpressionSyntax)node;

            if (!IsPubliclyAccessible(context, memberAccessExpression.Expression))
            {
                return false;
            }
        }

        return context.SemanticModel.GetSymbolInfo(node, context.CancellationToken).Symbol
            ?.DeclaredAccessibility == Accessibility.Public;
    }
}
