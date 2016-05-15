using System.Collections.Generic;
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
    /// Analyzes the use of non-generic collections.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonGenericCollectionDesignAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor NonGenericCollectionsShouldNotBeHeldAsStateDescriptor
            = new DiagnosticDescriptor("CT1004", "Non-generic collections should not be held as state.",
                "Non-generic collections should not be held as state.", "CodeTiger.Design",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor NonGenericCollectionsShouldNotBeExposedDescriptor
            = new DiagnosticDescriptor("CT1005", "Non-generic collections should not be exposed.",
                "Non-generic collections should not be exposed.", "CodeTiger.Design",
                DiagnosticSeverity.Warning, true);

        private static readonly string[] _nonGenericCollectionMetadataNames = new string[]
            {
                "System.Collections.ArrayList",
                "System.Collections.CollectionBase",
                "System.Collections.DictionaryBase",
                "System.Collections.Hashtable",
                "System.Collections.ICollection",
                "System.Collections.IDictionary",
                "System.Collections.IEnumerable",
                "System.Collections.IList",
                "System.Collections.Queue",
                "System.Collections.ReadOnlyCollectionBase",
                "System.Collections.SortedList",
                "System.Collections.Stack",
                "System.Runtime.InteropServices.BINDPTR",
                "System.Runtime.InteropServices.ComTypes.BindPtr",
            };

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(NonGenericCollectionsShouldNotBeHeldAsStateDescriptor,
                    NonGenericCollectionsShouldNotBeExposedDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeTypeForNonGenericCollectionState, SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyForNonGenericCollectionExposure,
                SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodForNonGenericCollectionExposure,
                SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConstructorForNonGenericCollectionExposure,
                SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeFieldForNonGenericCollectionExposure,
                SyntaxKind.FieldDeclaration);
        }

        private static void AnalyzeTypeForNonGenericCollectionState(SyntaxNodeAnalysisContext context)
        {
            SyntaxList<MemberDeclarationSyntax> members;

            switch (context.Node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    members = ((ClassDeclarationSyntax)context.Node).Members;
                    break;
                case SyntaxKind.StructDeclaration:
                    members = ((StructDeclarationSyntax)context.Node).Members;
                    break;
                default:
                    return;
            }

            foreach (var nonGenericCollectionMember in members
                .Where(x => IsOrIncludesNonGenericCollection(context, x)))
            {
                context.ReportDiagnostic(Diagnostic.Create(NonGenericCollectionsShouldNotBeHeldAsStateDescriptor,
                    GetIdentifierLocation(nonGenericCollectionMember)));
            }
        }

        private static void AnalyzePropertyForNonGenericCollectionExposure(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            var propertySymbol = context.SemanticModel
                .GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);

            if (!propertySymbol.IsExternallyAccessible())
            {
                return;
            }

            if (IsOrIncludesNonGenericCollection(context, propertyDeclaration.Type))

            context.ReportDiagnostic(Diagnostic.Create(NonGenericCollectionsShouldNotBeExposedDescriptor,
                propertyDeclaration.Type.GetLocation()));
        }

        private static void AnalyzeMethodForNonGenericCollectionExposure(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var methodSymbol = context.SemanticModel
                .GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

            if (!methodSymbol.IsExternallyAccessible())
            {
                return;
            }
            
            if (IsOrIncludesNonGenericCollection(context, methodDeclaration.ReturnType))
            {
                context.ReportDiagnostic(Diagnostic.Create(NonGenericCollectionsShouldNotBeExposedDescriptor,
                    methodDeclaration.ReturnType.GetLocation()));
            }

            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                if (IsOrIncludesNonGenericCollection(context, parameter.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(NonGenericCollectionsShouldNotBeExposedDescriptor,
                        parameter.Type.GetLocation()));
                }
            }
        }

        private static void AnalyzeConstructorForNonGenericCollectionExposure(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;
            var constructorSymbol = context.SemanticModel
                .GetDeclaredSymbol(constructorDeclaration, context.CancellationToken);

            if (!constructorSymbol.IsExternallyAccessible())
            {
                return;
            }

            foreach (var parameter in constructorDeclaration.ParameterList.Parameters)
            {
                if (IsOrIncludesNonGenericCollection(context, parameter.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(NonGenericCollectionsShouldNotBeExposedDescriptor,
                        parameter.Type.GetLocation()));
                }
            }
        }

        private static void AnalyzeFieldForNonGenericCollectionExposure(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
            var variableDeclarator = fieldDeclaration.Declaration.Variables.Single();
            var fieldSymbol = context.SemanticModel
                .GetDeclaredSymbol(variableDeclarator, context.CancellationToken);

            if (!fieldSymbol.IsExternallyAccessible())
            {
                return;
            }

            if (IsOrIncludesNonGenericCollection(context, fieldDeclaration.Declaration.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(NonGenericCollectionsShouldNotBeExposedDescriptor,
                    fieldDeclaration.Declaration.Type.GetLocation()));
            }
        }

        private static bool IsOrIncludesNonGenericCollection(SyntaxNodeAnalysisContext context,
            MemberDeclarationSyntax member)
        {
            TypeSyntax memberTypeSyntax;

            switch (member.Kind())
            {
                case SyntaxKind.FieldDeclaration:
                    memberTypeSyntax = ((FieldDeclarationSyntax)member).Declaration.Type;
                    break;
                case SyntaxKind.PropertyDeclaration:
                    {
                        var propertyNode = (PropertyDeclarationSyntax)member;
                        if (propertyNode.AccessorList.Accessors.Any(x => x.Body != null))
                        {
                            return false;
                        }

                        memberTypeSyntax = propertyNode.Type;
                    }
                    break;
                default:
                    return false;
            }

            return IsOrIncludesNonGenericCollection(context, memberTypeSyntax);
        }

        private static bool IsOrIncludesNonGenericCollection(SyntaxNodeAnalysisContext context,
            TypeSyntax typeSyntax)
        {
            if (typeSyntax.Kind() == SyntaxKind.ArrayType)
            {
                return IsOrIncludesNonGenericCollection(context, ((ArrayTypeSyntax)typeSyntax).ElementType);
            }

            var genericArguments = typeSyntax.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault();
            if (genericArguments != null)
            {
                return genericArguments.TypeArgumentList.Arguments
                    .Any(x => IsOrIncludesNonGenericCollection(context, x));
            }

            var typeSymbol = context.SemanticModel
                .GetTypeInfo(typeSyntax, context.CancellationToken)
                .Type;

            foreach (string nonGenericCollectionName in _nonGenericCollectionMetadataNames)
            {
                var nonGenericSymbol = context.SemanticModel.Compilation
                    .GetTypeByMetadataName(nonGenericCollectionName);
                if (nonGenericSymbol != null && nonGenericSymbol.Equals(typeSymbol))
                {
                    return true;
                }
            }

            return false;
        }

        private static Location GetIdentifierLocation(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.FieldDeclaration:
                    return ((FieldDeclarationSyntax)node).Declaration.Variables.First().GetLocation();
                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)node).Identifier.GetLocation();
                default:
                    return null;
            }
        }
    }
}