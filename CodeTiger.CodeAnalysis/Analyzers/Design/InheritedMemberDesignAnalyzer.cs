using System.Collections.Immutable;
using System.Linq;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design
{
    /// <summary>
    /// Analyzes the design of inherited members.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InheritedMemberDesignAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor
            MembersOfBaseTypesShouldNotBeHiddenExceptToReturnMoreSpecializedTypesDescriptor
            = new DiagnosticDescriptor("CT1002",
                "Members of base types should not be hidden except to return more specialized types.",
                "Members of base types should not be hidden except to return more specialized types.",
                "CodeTiger.Design", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    MembersOfBaseTypesShouldNotBeHiddenExceptToReturnMoreSpecializedTypesDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeMethodForHidingOfBaseImplementation,
                SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyForHidingOfBaseImplementation,
                SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeFieldForHidingOfBaseImplementation,
                SyntaxKind.FieldDeclaration);
        }

        private static void AnalyzeMethodForHidingOfBaseImplementation(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!methodDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.NewKeyword))
            {
                return;
            }

            var method = context.SemanticModel
                .GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

            var objectType = context.Compilation.GetSpecialType(SpecialType.System_Object);

            if (method.ReturnType == objectType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MembersOfBaseTypesShouldNotBeHiddenExceptToReturnMoreSpecializedTypesDescriptor,
                    methodDeclaration.Identifier.GetLocation()));
                return;
            }

            var baseType = method.ContainingType.BaseType;

            while (baseType != null)
            {
                var matchingBaseMethods = baseType.GetMembers(method.Name)
                    .Where(x => x.Kind == SymbolKind.Method && x.Name == method.Name)
                    .OfType<IMethodSymbol>()
                    .Where(x => AreParameterTypesEqual(method.Parameters, x.Parameters))
                    .ToList();

                switch (matchingBaseMethods.Count)
                {
                    case 0:
                        break;
                    case 1:
                        var matchingBaseMethod = matchingBaseMethods.Single();
                        if (method.ReturnType != objectType
                            && !method.ReturnType.IsSubclassOf(matchingBaseMethod.ReturnType)
                            && !method.ReturnType.AllInterfaces.Contains(matchingBaseMethod.ReturnType))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                MembersOfBaseTypesShouldNotBeHiddenExceptToReturnMoreSpecializedTypesDescriptor,
                                methodDeclaration.Identifier.GetLocation()));
                        }
                        return;
                    default:
                        return;
                }

                baseType = baseType.BaseType;
            }
        }

        private static void AnalyzePropertyForHidingOfBaseImplementation(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            if (!propertyDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.NewKeyword))
            {
                return;
            }

            var property = context.SemanticModel
                .GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);

            var baseType = property.ContainingType.BaseType;

            while (baseType != null)
            {
                var matchingBaseProperties = baseType.GetMembers(property.Name)
                    .Where(x => x.Kind == SymbolKind.Property && x.Name == property.Name)
                    .OfType<IPropertySymbol>()
                    .Where(x => AreParameterTypesEqual(property.Parameters, x.Parameters))
                    .ToList();

                switch (matchingBaseProperties.Count)
                {
                    case 0:
                        break;
                    case 1:
                        var matchingBaseProperty = matchingBaseProperties.Single();
                        if (!property.Type.IsSubclassOf(matchingBaseProperty.Type)
                            && !property.Type.AllInterfaces.Contains(matchingBaseProperty.Type))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                MembersOfBaseTypesShouldNotBeHiddenExceptToReturnMoreSpecializedTypesDescriptor,
                                propertyDeclaration.Identifier.GetLocation()));
                        }
                        return;
                    default:
                        return;
                }

                baseType = baseType.BaseType;
            }
        }

        private static void AnalyzeFieldForHidingOfBaseImplementation(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            if (!fieldDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.NewKeyword))
            {
                return;
            }

            var fieldDeclarationType = context.SemanticModel
                .GetTypeInfo(fieldDeclaration.Declaration.Type, context.CancellationToken).Type;

            foreach (var fieldVariable in fieldDeclaration.Declaration.Variables)
            {
                var field = context.SemanticModel.GetDeclaredSymbol(fieldVariable, context.CancellationToken);

                var baseType = field.ContainingType.BaseType;

                while (baseType != null)
                {
                    var matchingBaseFields = baseType.GetMembers(field.Name)
                        .Where(x => x.Kind == SymbolKind.Field && x.Name == field.Name)
                        .OfType<IFieldSymbol>()
                        .ToList();

                    switch (matchingBaseFields.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            var matchingBaseField = matchingBaseFields.Single();

                            if (!fieldDeclarationType.IsSubclassOf(matchingBaseField.Type)
                                && !fieldDeclarationType.AllInterfaces.Contains(matchingBaseField.Type))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(
                                    MembersOfBaseTypesShouldNotBeHiddenExceptToReturnMoreSpecializedTypesDescriptor,
                                    fieldVariable.Identifier.GetLocation()));
                                return;
                            }
                            break;
                        default:
                            return;
                    }

                    baseType = baseType.BaseType;
                }
            }
        }

        private static bool AreParameterTypesEqual(ImmutableArray<IParameterSymbol> firstParameters,
            ImmutableArray<IParameterSymbol> secondParameters)
        {
            if (firstParameters.Length != secondParameters.Length)
            {
                return false;
            }

            for (int i = 0; i < firstParameters.Length; i++)
            {
                if (firstParameters[i].Type != secondParameters[i].Type)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
