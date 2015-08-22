using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability
{
    /// <summary>
    /// Analyzes the handling of unmanaged state for potential reliability issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnmanagedReliabilityAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string[] _unmanagedTypeMetadataNames = new string[]
        {
            "System.IntPtr",
            "System.UIntPtr",
            "System.Runtime.InteropServices.BINDPTR",
            "System.Runtime.InteropServices.ComTypes.BindPtr",
        };

        internal static readonly DiagnosticDescriptor TypesWithDisposableStateShouldImplementIDisposableDescriptor
            = new DiagnosticDescriptor("CT2001", "Types with disposable state should implement IDisposable",
                "Types with disposable state should implement IDisposable", "CodeTiger.Reliability",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            TypesWithUnmanagedStateShouldImplementTheFullDisposePatternDescriptor = new DiagnosticDescriptor(
                "CT2002", "Types with unmanaged state should implement the full dispose pattern.",
                "Types with unmanaged state should implement the full dispose pattern.", "CodeTiger.Reliability",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(TypesWithDisposableStateShouldImplementIDisposableDescriptor,
                    TypesWithUnmanagedStateShouldImplementTheFullDisposePatternDescriptor);
            }
        }

        /// <summary>
        /// Registers actions in an analysis context.
        /// </summary>
        /// <param name="context">The context to register actions in.</param>
        /// <remarks>This method should only be called once, at the start of a session.</remarks>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSemanticModelAction(AnalyzeUnmanagedState);
        }

        private void AnalyzeUnmanagedState(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            var iDisposableType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.IDisposable");
            var disposeMethod = iDisposableType.GetMembers("Dispose").Single();

            foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var instanceStateMembers = typeDeclaration.Members.Where(IsInstanceState);
                var instanceStateMemberTypes = instanceStateMembers.Select(GetMemberType);

                bool doesTypeImplementIDisposable
                    = DoesTypeImplementIDisposable(context, typeDeclaration, iDisposableType);
                bool doesTypeHaveADestructor = DoesTypeHaveAFinalizer(context, typeDeclaration);

                if (iDisposableType != null
                    && AreAnyTypesDisposable(context, instanceStateMemberTypes, iDisposableType)
                    && !doesTypeImplementIDisposable)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        TypesWithDisposableStateShouldImplementIDisposableDescriptor,
                        typeDeclaration.Identifier.GetLocation()));
                }

                if (AreAnyTypesUnmanaged(context, instanceStateMemberTypes)
                    && (!doesTypeImplementIDisposable || !doesTypeHaveADestructor))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        TypesWithUnmanagedStateShouldImplementTheFullDisposePatternDescriptor,
                        typeDeclaration.Identifier.GetLocation()));
                }
            }
        }

        private bool IsInstanceState(MemberDeclarationSyntax memberDeclaration)
        {
            switch (memberDeclaration.Kind())
            {
                case SyntaxKind.FieldDeclaration:
                    {
                        var fieldDeclaration = (BaseFieldDeclarationSyntax)memberDeclaration;
                        return !fieldDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.StaticKeyword);
                    }
                case SyntaxKind.PropertyDeclaration:
                    {
                        var propertyDeclaration = (BasePropertyDeclarationSyntax)memberDeclaration;
                        return !propertyDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.StaticKeyword)
                            && propertyDeclaration.AccessorList.Accessors.All(x => x.Body == null);
                    }
                default:
                    return false;
            }
        }

        private TypeSyntax GetMemberType(MemberDeclarationSyntax memberDeclaration)
        {
            switch (memberDeclaration.Kind())
            {
                case SyntaxKind.FieldDeclaration:
                    {
                        var fieldDeclaration = (BaseFieldDeclarationSyntax)memberDeclaration;
                        return fieldDeclaration.Declaration.Type;
                    }
                case SyntaxKind.PropertyDeclaration:
                    {
                        var propertyDeclaration = (BasePropertyDeclarationSyntax)memberDeclaration;
                        return propertyDeclaration.Type;
                    }
                default:
                    throw new ArgumentException("The argument must be a field or property.",
                        nameof(memberDeclaration));
            }
        }

        private bool AreAnyTypesUnmanaged(SemanticModelAnalysisContext context,
            IEnumerable<TypeSyntax> instanceStateMemberTypes)
        {
            if (instanceStateMemberTypes == null || !instanceStateMemberTypes.Any())
            {
                return false;
            }

            var unmanagedTypes = _unmanagedTypeMetadataNames
                .Select(context.SemanticModel.Compilation.GetTypeByMetadataName)
                .Where(x => x != null)
                .ToList();

            foreach (var instanceMemberType in instanceStateMemberTypes)
            {
                var memberType = context.SemanticModel.GetTypeInfo(instanceMemberType, context.CancellationToken);
                if (unmanagedTypes.Any(x => x.Equals(memberType.Type)))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AreAnyTypesDisposable(SemanticModelAnalysisContext context,
            IEnumerable<TypeSyntax> instanceStateMemberTypes, INamedTypeSymbol iDisposableType)
        {
            if (instanceStateMemberTypes == null || !instanceStateMemberTypes.Any())
            {
                return false;
            }

            return instanceStateMemberTypes
                .Select(x => context.SemanticModel.GetSymbolInfo(x, context.CancellationToken).Symbol)
                .Any(iDisposableType.Equals);
        }

        private bool DoesTypeHaveAFinalizer(SemanticModelAnalysisContext context,
            TypeDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Members.Any(x => x.Kind() == SyntaxKind.DestructorDeclaration);
        }

        private bool DoesTypeImplementIDisposable(SemanticModelAnalysisContext context,
            TypeDeclarationSyntax classDeclaration, INamedTypeSymbol iDisposableType)
        {
            return classDeclaration.BaseList != null
                && classDeclaration.BaseList.Types
                    .Select(x => context.SemanticModel.GetSymbolInfo(x.Type, context.CancellationToken).Symbol)
                    .Any(iDisposableType.Equals);
        }
    }
}