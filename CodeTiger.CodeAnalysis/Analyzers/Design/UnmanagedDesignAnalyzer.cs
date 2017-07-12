using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Design
{
    /// <summary>
    /// Analyzes the design of unmanaged elements.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnmanagedDesignAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor TypesWithoutUnmanagedStateShouldNotHaveAFinalizerDescriptor
            = new DiagnosticDescriptor("CT1007", "Types without unmanaged state should not have a finalizer.",
                "Types without unmanaged state should not have a finalizer.", "CodeTiger.Design",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor EmptyFinalizersShouldNotExistDescriptor
            = new DiagnosticDescriptor("CT1008", "Empty finalizers should not exist.",
                "Empty finalizers should not exist.", "CodeTiger.Design", DiagnosticSeverity.Warning, true);

        private static readonly string[] _metadataNamesOfUnmanagedTypes = new string[]
            {
                "System.IntPtr",
                "System.UIntPtr",
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
                return ImmutableArray.Create(TypesWithoutUnmanagedStateShouldNotHaveAFinalizerDescriptor,
                    EmptyFinalizersShouldNotExistDescriptor);
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

            context.RegisterSemanticModelAction(AnalyzeUnmanagedState);
        }

        private static void AnalyzeUnmanagedState(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            var disposableType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.IDisposable");

            foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var instanceStateMemberTypes = typeDeclaration.Members.Where(IsInstanceState)
                    .Select(GetMemberType)
                    .ToList();

                var destructor = typeDeclaration.Members
                    .FirstOrDefault(x => x.Kind() == SyntaxKind.DestructorDeclaration)
                    as DestructorDeclarationSyntax;

                if (destructor != null)
                {
                    if (!destructor.Body.ChildNodes().OfType<StatementSyntax>().Any())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(EmptyFinalizersShouldNotExistDescriptor,
                            destructor.Identifier.GetLocation()));
                    }

                    if (!AreAnyTypesUnmanaged(context, instanceStateMemberTypes))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            TypesWithoutUnmanagedStateShouldNotHaveAFinalizerDescriptor,
                            destructor.Identifier.GetLocation()));
                    }
                }
            }
        }
        private static bool IsInstanceState(MemberDeclarationSyntax memberDeclaration)
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
                        var propertyDeclaration = (PropertyDeclarationSyntax)memberDeclaration;
                        return !propertyDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.StaticKeyword)
                            && propertyDeclaration.ExpressionBody == null
                            && propertyDeclaration.AccessorList?.Accessors.All(x => x.Body == null) == true;
                    }
                default:
                    return false;
            }
        }

        private static TypeSyntax GetMemberType(MemberDeclarationSyntax memberDeclaration)
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

        private static bool AreAnyTypesUnmanaged(SemanticModelAnalysisContext context,
            IEnumerable<TypeSyntax> instanceStateMemberTypes)
        {
            if (instanceStateMemberTypes == null || !instanceStateMemberTypes.Any())
            {
                return false;
            }

            var unmanagedTypes = _metadataNamesOfUnmanagedTypes
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
    }
}
