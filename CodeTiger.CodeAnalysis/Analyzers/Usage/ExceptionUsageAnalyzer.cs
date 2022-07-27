using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Usage;

/// <summary>
/// Analyzes usage of exceptions.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExceptionUsageAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor
        ExceptionsThrownWithinCatchBlocksShouldIncludeTheCaughtExceptionAsAnInnerExceptionDescriptor
        = new DiagnosticDescriptor("CT2200",
            "Exceptions thrown within catch blocks should include the caught exception as an inner exception.",
            "The caught exception{0} should be included as an inner exception.", "CodeTiger.Usage",
            DiagnosticSeverity.Warning, true);

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(
                ExceptionsThrownWithinCatchBlocksShouldIncludeTheCaughtExceptionAsAnInnerExceptionDescriptor);
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

        context.RegisterSemanticModelAction(AnalyzeExceptionUsage);
    }

    private static void AnalyzeExceptionUsage(SemanticModelAnalysisContext context)
    {
        var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

        var nestedThrows = root.DescendantNodes().OfType<CatchClauseSyntax>()
            .SelectMany(catchDeclaration
                => catchDeclaration.Block?.DescendantNodes().OfType<ThrowStatementSyntax>()
                    ?? Enumerable.Empty<ThrowStatementSyntax>())
            .Distinct();
        foreach (var nestedThrow in nestedThrows.Where(x => x?.Expression != null))
        {
            AnalyzeThrowExpressionWithinCatchDeclaration(context, nestedThrow.Expression);
        }
    }

    private static void AnalyzeThrowExpressionWithinCatchDeclaration(SemanticModelAnalysisContext context,
        ExpressionSyntax throwExpression)
    {
        var caughtExceptionIdentifier = GetCaughtExceptionIdentifier(throwExpression);

        // Ignore exceptions that are rethrown by explicitly including the caught exception (e.g. "throw ex;"),
        // since they will be identified by CA2200.
        if (!string.IsNullOrEmpty(caughtExceptionIdentifier?.ValueText)
            && IsRethrowWithExplicitArgument(throwExpression, caughtExceptionIdentifier.Value))
        {
            return;
        }

        if (!IncludesCaughtException(context, throwExpression, caughtExceptionIdentifier))
        {
            string caughtExceptionText = !string.IsNullOrEmpty(caughtExceptionIdentifier?.ValueText)
                ? string.Format(CultureInfo.CurrentCulture, " '{0}'", caughtExceptionIdentifier.Value.ValueText)
                : string.Empty;

            context.ReportDiagnostic(Diagnostic.Create(
                ExceptionsThrownWithinCatchBlocksShouldIncludeTheCaughtExceptionAsAnInnerExceptionDescriptor,
                throwExpression.GetLocation(), caughtExceptionText));
        }
    }

    private static bool IsRethrowWithExplicitArgument(ExpressionSyntax throwExpression,
        SyntaxToken caughtExceptionIdentifier)
    {
        if (throwExpression.Kind() == SyntaxKind.IdentifierName)
        {
            var throwIdentifier = (IdentifierNameSyntax)throwExpression;

            // TODO: Find a more direct/reliable way to do this comparison (using SemanticModel.GetSymbolInfo?)
            if (throwIdentifier.Identifier.ValueText == caughtExceptionIdentifier.ValueText)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IncludesCaughtException(SemanticModelAnalysisContext context,
        ExpressionSyntax throwExpression, SyntaxToken? caughtExceptionIdentifier)
    {
        if (!caughtExceptionIdentifier.HasValue)
        {
            return false;
        }

        return IsIdentifierReadByExpression(context, caughtExceptionIdentifier.Value, throwExpression);
    }

    private static SyntaxToken? GetCaughtExceptionIdentifier(ExpressionSyntax throwExpression)
    {
        var catchExpression = throwExpression.Parent;

        while (catchExpression != null)
        {
            if (catchExpression.Kind() == SyntaxKind.CatchClause)
            {
                return ((CatchClauseSyntax)catchExpression).Declaration?.Identifier;
            }

            catchExpression = catchExpression.Parent;
        }

        return null;
    }

    private static bool IsIdentifierReadByExpression(SemanticModelAnalysisContext context, SyntaxToken identifier,
        ExpressionSyntax expression)
    {
        var expressionDataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(expression);

        // See if the identifier is used directly as part of the expression.
        foreach (var readInside in expressionDataFlowAnalysis.ReadInside)
        {
            // TODO: Find a more direct/reliable way to do this comparison (using SemanticModel.GetSymbolInfo?)
            if (readInside.Name == identifier.ValueText)
            {
                return true;
            }

            foreach (var declaringReference in readInside.DeclaringSyntaxReferences)
            {
                var declaringReferenceNode = declaringReference.GetSyntax(context.CancellationToken);

                if (declaringReferenceNode.Kind() != SyntaxKind.VariableDeclarator)
                {
                    continue;
                }

                var variableDeclarator = (VariableDeclaratorSyntax)declaringReferenceNode;
                if (variableDeclarator.Initializer?.Value != null
                    && IsIdentifierReadByExpression(context, identifier, variableDeclarator.Initializer.Value))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
