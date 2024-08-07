﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using CodeTiger.CodeAnalysis.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Reliability;

/// <summary>
/// Analyzes the handling of unmanaged state for potential reliability issues.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnmanagedReliabilityAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor TypesWithDisposableStateShouldImplementIDisposableDescriptor
        = new DiagnosticDescriptor("CT2001", "Types with disposable state should implement IDisposable",
            "Types with disposable state should implement IDisposable", "CodeTiger.Reliability",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor
        TypesWithUnmanagedStateShouldImplementTheFullDisposePatternDescriptor = new DiagnosticDescriptor(
            "CT2002", "Types with unmanaged state should implement the full dispose pattern",
            "Types with unmanaged state should implement the full dispose pattern", "CodeTiger.Reliability",
            DiagnosticSeverity.Warning, true);
    internal static readonly DiagnosticDescriptor DestructorsShouldNotAccessManagedObjectsDescriptor
        = new DiagnosticDescriptor("CT2003", "Destructors should not access managed objects",
            "Destructors should not access managed objects", "CodeTiger.Reliability",
            DiagnosticSeverity.Warning, true);

    private static readonly string[] _metadataNamesOfUnmanagedTypes = new string[]
        {
            typeof(IntPtr).FullName,
            typeof(UIntPtr).FullName,
            typeof(BINDPTR).FullName,
        };
    private static readonly string[] _metadataNamesOfDestructorSafeTypeNames = new string[]
        {
            typeof(GC).FullName,
            typeof(Marshal).FullName,
        };
    private static readonly string[] _metadataNamesOfDisposableTypesWhichDoNotNeedToBeDisposed = new string[]
        {
            typeof(Task).FullName,
        };

    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(TypesWithDisposableStateShouldImplementIDisposableDescriptor,
                TypesWithUnmanagedStateShouldImplementTheFullDisposePatternDescriptor,
                DestructorsShouldNotAccessManagedObjectsDescriptor);
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

        context.RegisterSemanticModelAction(AnalyzeUnmanagedState);
    }

    private static void AnalyzeUnmanagedState(SemanticModelAnalysisContext context)
    {
        var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

        var disposableType = context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_IDisposable);

        foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            var instanceStateMemberTypes = typeDeclaration.Members.Where(IsInstanceState)
                .Select(GetMemberType)
                .ToList();

            bool isTypeDisposable = IsTypeDisposable(context, typeDeclaration, disposableType);

            if (typeDeclaration.Kind() == SyntaxKind.ClassDeclaration
                && !isTypeDisposable
                && instanceStateMemberTypes.Any(x => DoesTypeNeedToBeDisposed(context, x, disposableType)))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    TypesWithDisposableStateShouldImplementIDisposableDescriptor,
                    typeDeclaration.Identifier.GetLocation()));
            }

            var destructor = typeDeclaration.Members
                .FirstOrDefault(x => x.Kind() == SyntaxKind.DestructorDeclaration);

            if (typeDeclaration.Kind() == SyntaxKind.ClassDeclaration
                && (!isTypeDisposable || destructor == null)
                && AreAnyTypesUnmanaged(context, instanceStateMemberTypes))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    TypesWithUnmanagedStateShouldImplementTheFullDisposePatternDescriptor,
                    typeDeclaration.Identifier.GetLocation()));
            }

            if (destructor != null)
            {
                AnalyzeDestructorForManagedObjects(context, destructor);
            }
        }
    }

    private static void AnalyzeDestructorForManagedObjects(SemanticModelAnalysisContext context,
        MemberDeclarationSyntax destructorDeclaration)
    {
        var destructorSymbol = context.SemanticModel
            .GetDeclaredSymbol(destructorDeclaration, context.CancellationToken);
        if (destructorSymbol == null)
        {
            return;
        }

        foreach (var node in destructorDeclaration.DescendantNodes())
        {
            SyntaxNode? accessedNode;
            ISymbol? accessedSymbol;

            switch (node.Kind())
            {
                case SyntaxKind.PointerMemberAccessExpression:
                case SyntaxKind.SimpleMemberAccessExpression:
                    accessedNode = node;
                    accessedSymbol = context.SemanticModel
                        .GetSymbolInfo(accessedNode, context.CancellationToken).Symbol;
                    break;
                case SyntaxKind.InvocationExpression:
                    accessedNode = ((InvocationExpressionSyntax)node).Expression;
                    accessedSymbol = context.SemanticModel
                        .GetSymbolInfo(accessedNode, context.CancellationToken).Symbol?.ContainingSymbol;
                    break;
                default:
                    accessedNode = null;
                    accessedSymbol = null;
                    break;
            }

            if (accessedNode != null
                && accessedSymbol is ITypeSymbol accessedTypeSymbol
                && !SymbolEqualityComparer.Default.Equals(accessedTypeSymbol, destructorSymbol.ContainingType)
                && IsTypeProbablyUnsafeToAccessFromDestructor(context, accessedTypeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DestructorsShouldNotAccessManagedObjectsDescriptor, accessedNode.GetLocation()));
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
                    return !fieldDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
                }
            case SyntaxKind.PropertyDeclaration:
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)memberDeclaration;
                    return !propertyDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword))
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
                throw new ArgumentException(Strings.ArgumentMustBeFieldOrPropertyMessage,
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
            if (unmanagedTypes.Contains(memberType.Type, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        return false;
    }

    private static bool DoesTypeNeedToBeDisposed(SemanticModelAnalysisContext context,
        TypeSyntax type, INamedTypeSymbol disposableType)
    {
        var typeSymbol = context.SemanticModel.GetTypeInfo(type, context.CancellationToken).Type;

        if (typeSymbol is null || !IsTypeDisposable(disposableType, typeSymbol))
        {
            return false;
        }

        var disposableTypesWhichDoNotNeedToBeDisposed
            = _metadataNamesOfDisposableTypesWhichDoNotNeedToBeDisposed
                .Select(context.SemanticModel.Compilation.GetTypeByMetadataName)
                .Where(x => x != null)
                .ToList();

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (disposableTypesWhichDoNotNeedToBeDisposed.Contains(baseType, SymbolEqualityComparer.Default))
            {
                return false;
            }

            baseType = baseType.BaseType;
        }

        return true;
    }

    private static bool IsTypeDisposable(SemanticModelAnalysisContext context,
        TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol disposableType)
    {
        var typeSymbol = context.SemanticModel
            .GetDeclaredSymbol(typeDeclaration, context.CancellationToken);

        return typeSymbol is not null && IsTypeDisposable(disposableType, typeSymbol);
    }

    private static bool IsTypeDisposable(INamedTypeSymbol disposableType, ITypeSymbol typeSymbol)
    {
        if (disposableType == null)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(disposableType, typeSymbol)
            || typeSymbol.AllInterfaces.Contains(disposableType, SymbolEqualityComparer.Default);
    }

    private static bool IsTypeProbablyUnsafeToAccessFromDestructor(SemanticModelAnalysisContext context,
        ITypeSymbol accessedTypeSymbol)
    {
        if (accessedTypeSymbol.IsValueType)
        {
            return false;
        }

        foreach (string destructorSafeTypeName in _metadataNamesOfDestructorSafeTypeNames)
        {
            var destructorSafeType = context.SemanticModel.Compilation
                .GetTypeByMetadataName(destructorSafeTypeName);
            if (SymbolEqualityComparer.Default.Equals(destructorSafeType, accessedTypeSymbol))
            {
                return false;
            }
        }

        return true;
    }
}
