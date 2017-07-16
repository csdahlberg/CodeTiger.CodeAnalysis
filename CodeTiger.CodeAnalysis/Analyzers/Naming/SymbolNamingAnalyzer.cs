﻿using System;
using System.Collections.Immutable;
using System.Linq;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Naming
{
    /// <summary>
    /// Analyzes symbol names.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SymbolNamingAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor TypeNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1702", "Type names should use pascal casing.",
                "Type names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ConstantFieldNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1703", "Constant field names should use pascal casing.",
                "Constant field names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning,
                true);
        internal static readonly DiagnosticDescriptor PrivateFieldNamesShouldUseCamelCasingDescriptor
            = new DiagnosticDescriptor("CT1704",
                "Private field names should use camel casing with a leading underscore.",
                "Private field names should use camel casing with a leading underscore.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor EventNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1705", "Event names should use pascal casing.",
                "Event names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor DelegateNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1706", "Delegate names should use pascal casing.",
                "Delegate names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor PropertyNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1707", "Property names should use pacal casing.",
                "Property names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor MethodNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1708", "Method names should use pascal casing.",
                "Method names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor EnumerationMemberNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1709", "Enumeration member names should use pascal casing.",
                "Enumeration member names should use pascal casing.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor VariableNamesShouldUseCamelCasingDescriptor
            = new DiagnosticDescriptor("CT1710", "Variable names should use camel casing.",
                "Variable names should use camel casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor InterfaceNamesShouldUsePascalCasingPrefixedWithIDescriptor
            = new DiagnosticDescriptor("CT1711", "Interface names should use pascal casing prefixed with 'I'.",
                "Interface names should use pascal casing prefixed with 'I'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ParameterNamesShouldUseCamelCasingDescriptor
            = new DiagnosticDescriptor("CT1712", "Parameter names should use camel casing.",
                "Parameter names should use camel casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            PropertyNamesShouldNotBeginWithTheNameOfTheContainingingTypeDescriptor = new DiagnosticDescriptor(
                "CT1714", "Property names should not begin with the name of the containing type.",
                "Property names should not begin with the name of the containing type.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            GenericTypeParameterNamesShouldUsePascalCasingPrefixedWithCapitalTDescriptor
            = new DiagnosticDescriptor("CT1715",
                "Generic type parameter names should use pascal casing prefixed with 'T'.",
                "Generic type parameter names should use pascal casing prefixed with 'T'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor GenericTypeParameterNamesShouldBeDescriptiveDescriptor
            = new DiagnosticDescriptor("CT1716", "Generic type parameter names should be descriptive.",
                "Generic type parameter names should be descriptive.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            GenericTypeParameterNamesShouldNotBeSuffixedWithTypeDescriptor = new DiagnosticDescriptor(
                "CT1717", "Generic type parameter names should not be suffixed with 'Type'.",
                "Generic type parameter names should not be suffixed with 'Type'.", "CodeTiger.Name",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor PropertyNamesShouldNotBePrefixedWithGetOrSetDescriptor
            = new DiagnosticDescriptor("CT1718", "Property names should not be prefixed with 'Get' or 'Set'.",
                "Property names should not be prefixed with 'Get' or 'Set'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor TypeNamesShouldNotBePrefixedWithAbstractDescriptor
            = new DiagnosticDescriptor("CT1719", "Type names should not be prefixed with 'Abstract'.",
                "Type names should not be prefixed with 'Abstract'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor TypeNamesShouldNotBePrefixedOrSuffixedWithBaseDescriptor
            = new DiagnosticDescriptor("CT1720", "Type names should not be prefixed or suffixed with 'Base'.",
                "Type names should not be prefixed or suffixed with 'Base'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor AttributeTypeNamesShouldBeSuffixedWithAttributeDescriptor
            = new DiagnosticDescriptor("CT1721", "Attribute type names should be suffixed with 'Attribute'.",
                "Attribute type names should be suffixed with 'Attribute'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            NonAttributeTypeNamesShouldNotBeSuffixedWithAttributeDescriptor = new DiagnosticDescriptor("CT1722",
                "Non-attribute type names should not be suffixed with 'Attribute'.",
                "Non-attribute type names should not be suffixed with 'Attribute'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ExceptionTypeNamesShouldBeSuffixedWithExceptionDescriptor
            = new DiagnosticDescriptor("CT1723", "Exception type names should be suffixed with 'Exception'.",
                "Exception type names should be suffixed with 'Exception'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            NonExceptionTypeNamesShouldNotBeSuffixedWithExceptionDescriptor = new DiagnosticDescriptor("CT1724",
                "Non-exception type names should not be suffixed with 'Exception'.",
                "Non-exception type names should not be suffixed with 'Exception'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor EventArgsTypeNamesShouldBeSuffixedWithEventArgsDescriptor
            = new DiagnosticDescriptor("CT1725", "EventArgs type names should be suffixed with 'EventArgs'.",
                "EventArgs type names should be suffixed with 'EventArgs'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            NonEventArgsTypeNamesShouldNotBeSuffixedWithEventArgsDescriptor = new DiagnosticDescriptor("CT1726",
                "Non-EventArgs type names should not be suffixed with 'EventArgs'.",
                "Non-EventArgs type names should not be suffixed with 'EventArgs'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor MethodsReturningATaskShouldBeSuffixedWithAsyncDescriptor
            = new DiagnosticDescriptor("CT1727", "Methods returning a Task should be suffixed with 'Async'.",
                "Methods returning a Task should be suffixed with 'Async'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            MethodsNotReturningATaskOrVoidShouldNotBeSuffixedWithAsyncDescriptor = new DiagnosticDescriptor(
                "CT1728", "Methods not returning a Task or void should not be suffixed with 'Async'.",
                "Methods not returning a Task or void should not be suffixed with 'Async'.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(TypeNamesShouldUsePascalCasingDescriptor,
                    ConstantFieldNamesShouldUsePascalCasingDescriptor,
                    PrivateFieldNamesShouldUseCamelCasingDescriptor,
                    EventNamesShouldUsePascalCasingDescriptor,
                    DelegateNamesShouldUsePascalCasingDescriptor,
                    PropertyNamesShouldUsePascalCasingDescriptor,
                    MethodNamesShouldUsePascalCasingDescriptor,
                    EnumerationMemberNamesShouldUsePascalCasingDescriptor,
                    VariableNamesShouldUseCamelCasingDescriptor,
                    InterfaceNamesShouldUsePascalCasingPrefixedWithIDescriptor,
                    ParameterNamesShouldUseCamelCasingDescriptor,
                    PropertyNamesShouldNotBeginWithTheNameOfTheContainingingTypeDescriptor,
                    GenericTypeParameterNamesShouldUsePascalCasingPrefixedWithCapitalTDescriptor,
                    GenericTypeParameterNamesShouldBeDescriptiveDescriptor,
                    GenericTypeParameterNamesShouldNotBeSuffixedWithTypeDescriptor,
                    PropertyNamesShouldNotBePrefixedWithGetOrSetDescriptor,
                    TypeNamesShouldNotBePrefixedWithAbstractDescriptor,
                    TypeNamesShouldNotBePrefixedOrSuffixedWithBaseDescriptor,
                    AttributeTypeNamesShouldBeSuffixedWithAttributeDescriptor,
                    NonAttributeTypeNamesShouldNotBeSuffixedWithAttributeDescriptor,
                    ExceptionTypeNamesShouldBeSuffixedWithExceptionDescriptor,
                    NonExceptionTypeNamesShouldNotBeSuffixedWithExceptionDescriptor,
                    EventArgsTypeNamesShouldBeSuffixedWithEventArgsDescriptor,
                    NonEventArgsTypeNamesShouldNotBeSuffixedWithEventArgsDescriptor,
                    MethodsReturningATaskShouldBeSuffixedWithAsyncDescriptor,
                    MethodsNotReturningATaskOrVoidShouldNotBeSuffixedWithAsyncDescriptor);
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

            context.RegisterSyntaxNodeAction(AnalyzeTypeName, SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeFieldName, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeEventFieldName, SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeDelegateName, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyName, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodName, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeEnumerationMemberName, SyntaxKind.EnumMemberDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLocalVariableNames, SyntaxKind.LocalDeclarationStatement);
            context.RegisterSyntaxNodeAction(AnalyzeInterfaceName, SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeParameterName, SyntaxKind.Parameter);
            context.RegisterSyntaxNodeAction(AnalyzeGenericTypeParameterName, SyntaxKind.TypeParameter);
            context.RegisterSyntaxNodeAction(AnalyzeClassNamePrefixesAndSuffixes, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeTypeName(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken typeIdentifier;

            switch (context.Node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    typeIdentifier = ((ClassDeclarationSyntax)context.Node).Identifier;
                    break;
                case SyntaxKind.StructDeclaration:
                    typeIdentifier = ((StructDeclarationSyntax)context.Node).Identifier;
                    break;
                case SyntaxKind.EnumDeclaration:
                    typeIdentifier = ((EnumDeclarationSyntax)context.Node).Identifier;
                    break;
                default:
                    return;
            }

            if (NamingUtility.IsProbablyPascalCased(typeIdentifier.Text) == false
                || NamingUtility.IsProbablyPascalCased(typeIdentifier.Text, 'I') == true)
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeNamesShouldUsePascalCasingDescriptor,
                    typeIdentifier.GetLocation()));
            }
        }

        private void AnalyzeFieldName(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclarationNode = (FieldDeclarationSyntax)context.Node;
            if (fieldDeclarationNode.Modifiers.Any(x => x.Kind() == SyntaxKind.ConstKeyword))
            {
                foreach (var fieldDeclaration in fieldDeclarationNode.Declaration.Variables)
                {
                    if (NamingUtility.IsProbablyPascalCased(fieldDeclaration.Identifier.Text) == false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            ConstantFieldNamesShouldUsePascalCasingDescriptor,
                            fieldDeclaration.Identifier.GetLocation()));
                    }
                }
            }
            else if (fieldDeclarationNode.Modifiers.Any(x => x.Kind() == SyntaxKind.PrivateKeyword))
            {
                foreach (var fieldDeclaration in fieldDeclarationNode.Declaration.Variables)
                {
                    if (NamingUtility.IsProbablyCamelCased(fieldDeclaration.Identifier.Text, '_') == false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(PrivateFieldNamesShouldUseCamelCasingDescriptor,
                            fieldDeclaration.Identifier.GetLocation()));
                    }
                }
            }
        }

        private void AnalyzeEventFieldName(SyntaxNodeAnalysisContext context)
        {
            var eventFieldDeclarationNode = (EventFieldDeclarationSyntax)context.Node;
            foreach (var eventFieldDeclaration in eventFieldDeclarationNode.Declaration.Variables)
            {
                if (NamingUtility.IsProbablyPascalCased(eventFieldDeclaration.Identifier.Text) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(EventNamesShouldUsePascalCasingDescriptor,
                        eventFieldDeclaration.Identifier.GetLocation()));
                }
            }
        }

        private void AnalyzeDelegateName(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclarationNode = (DelegateDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(delegateDeclarationNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(DelegateNamesShouldUsePascalCasingDescriptor,
                    delegateDeclarationNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzePropertyName(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclarationNode = (PropertyDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(propertyDeclarationNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyNamesShouldUsePascalCasingDescriptor,
                    propertyDeclarationNode.Identifier.GetLocation()));
            }

            if (propertyDeclarationNode.Identifier.Text.StartsWith("Get", StringComparison.OrdinalIgnoreCase)
                || propertyDeclarationNode.Identifier.Text.StartsWith("Set", StringComparison.OrdinalIgnoreCase))
            {
                string propertyNameWithoutPrefix = propertyDeclarationNode.Identifier.Text.Substring(3);
                if (NamingUtility.IsProbablyPascalCased(propertyNameWithoutPrefix) == true)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        PropertyNamesShouldNotBePrefixedWithGetOrSetDescriptor,
                        propertyDeclarationNode.Identifier.GetLocation()));
                }
            }

            switch (propertyDeclarationNode.Parent.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.EnumDeclaration:
                    {
                        var parentIdentifier = ((BaseTypeDeclarationSyntax)propertyDeclarationNode.Parent)
                            .Identifier;
                        if (propertyDeclarationNode.Identifier.Text
                            .StartsWith(parentIdentifier.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                PropertyNamesShouldNotBeginWithTheNameOfTheContainingingTypeDescriptor,
                                propertyDeclarationNode.Identifier.GetLocation()));
                        }
                    }
                    break;
            }
        }

        private void AnalyzeMethodName(SyntaxNodeAnalysisContext context)
        {
            var methodDeclarationNode = (MethodDeclarationSyntax)context.Node;
            var methodDeclaration = context.SemanticModel.GetDeclaredSymbol(methodDeclarationNode);

            if (NamingUtility.IsProbablyPascalCased(methodDeclarationNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(MethodNamesShouldUsePascalCasingDescriptor,
                    methodDeclarationNode.Identifier.GetLocation()));
            }

            AnalyzeMethodNameForAsyncSuffix(context, methodDeclarationNode, methodDeclaration);
        }

        private static void AnalyzeMethodNameForAsyncSuffix(SyntaxNodeAnalysisContext context,
            MethodDeclarationSyntax methodDeclarationNode, IMethodSymbol methodDeclaration)
        {
            const string asyncText = "Async";
            var taskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            bool returnsTaskType = methodDeclaration.ReturnType.IsSameOrSubclassOf(taskType);
            bool hasAsyncSuffix = methodDeclaration.Name.EndsWith(asyncText, StringComparison.OrdinalIgnoreCase);
            if (returnsTaskType)
            {
                if (!hasAsyncSuffix)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        MethodsReturningATaskShouldBeSuffixedWithAsyncDescriptor,
                        methodDeclarationNode.Identifier.GetLocation()));
                }
            }
            else if (!methodDeclaration.ReturnsVoid && hasAsyncSuffix)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MethodsNotReturningATaskOrVoidShouldNotBeSuffixedWithAsyncDescriptor,
                    methodDeclarationNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzeEnumerationMemberName(SyntaxNodeAnalysisContext context)
        {
            var enumMemberDeclarationNode = (EnumMemberDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(enumMemberDeclarationNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(EnumerationMemberNamesShouldUsePascalCasingDescriptor,
                    enumMemberDeclarationNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzeLocalVariableNames(SyntaxNodeAnalysisContext context)
        {
            var localDeclarationSyntax = (LocalDeclarationStatementSyntax)context.Node;
            
            foreach (var variableDeclaratorSyntax in localDeclarationSyntax.Declaration.Variables)
            {
                if (NamingUtility.IsProbablyCamelCased(variableDeclaratorSyntax.Identifier.Text) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(VariableNamesShouldUseCamelCasingDescriptor,
                        variableDeclaratorSyntax.Identifier.GetLocation()));
                }
            }
        }

        private void AnalyzeInterfaceName(SyntaxNodeAnalysisContext context)
        {
            var interfaceDeclarationNode = (InterfaceDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(interfaceDeclarationNode.Identifier.Text, 'I') == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InterfaceNamesShouldUsePascalCasingPrefixedWithIDescriptor,
                    interfaceDeclarationNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzeParameterName(SyntaxNodeAnalysisContext context)
        {
            var parameterNode = (ParameterSyntax)context.Node;
            
            if (NamingUtility.IsProbablyCamelCased(parameterNode.Identifier.Text) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(ParameterNamesShouldUseCamelCasingDescriptor,
                    parameterNode.Identifier.GetLocation()));
            }
        }

        private void AnalyzeGenericTypeParameterName(SyntaxNodeAnalysisContext context)
        {
            var typeParameterNode = (TypeParameterSyntax)context.Node;

            if (typeParameterNode.Identifier.Text == "T")
            {
                return;
            }

            if (NamingUtility.IsProbablyPascalCased(typeParameterNode.Identifier.Text, 'T') == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GenericTypeParameterNamesShouldUsePascalCasingPrefixedWithCapitalTDescriptor,
                    typeParameterNode.Identifier.GetLocation()));
            }
            else
            {
                if (!IsNameProbablyDescriptive(typeParameterNode))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        GenericTypeParameterNamesShouldBeDescriptiveDescriptor,
                        typeParameterNode.Identifier.GetLocation()));
                }

                if (typeParameterNode.Identifier.Text.EndsWith("Type", StringComparison.OrdinalIgnoreCase))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        GenericTypeParameterNamesShouldNotBeSuffixedWithTypeDescriptor,
                        typeParameterNode.Identifier.GetLocation()));
                }
            }
        }

        private void AnalyzeClassNamePrefixesAndSuffixes(SyntaxNodeAnalysisContext context)
        {
            var classDeclarationNode = (ClassDeclarationSyntax)context.Node;
            var classDeclaration = context.SemanticModel.GetDeclaredSymbol(classDeclarationNode);
            string className = classDeclarationNode.Identifier.Text;

            const string abstractText = "Abstract";
            if (className.StartsWith(abstractText, StringComparison.OrdinalIgnoreCase)
                && NamingUtility.IsProbablyPascalCased(className.Substring(abstractText.Length)) == true)
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeNamesShouldNotBePrefixedWithAbstractDescriptor,
                    classDeclarationNode.Identifier.GetLocation()));
            }

            const string baseText = "Base";
            if (className.EndsWith(baseText, StringComparison.OrdinalIgnoreCase)
                || (className.StartsWith(baseText, StringComparison.OrdinalIgnoreCase)
                    && NamingUtility.IsProbablyPascalCased(className.Substring(baseText.Length)) == true))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    TypeNamesShouldNotBePrefixedOrSuffixedWithBaseDescriptor,
                    classDeclarationNode.Identifier.GetLocation()));
            }

            AnalyzeClassNameForAttributeSuffix(context, classDeclarationNode, classDeclaration);
            AnalyzeClassNameForExceptionSuffix(context, classDeclarationNode, classDeclaration);
            AnalyzeClassNameForEventArgsSuffix(context, classDeclarationNode, classDeclaration);
        }

        private static void AnalyzeClassNameForAttributeSuffix(SyntaxNodeAnalysisContext context,
            ClassDeclarationSyntax classDeclarationNode, INamedTypeSymbol classDeclaration)
        {
            const string attributeText = "Attribute";
            var attributeType = context.Compilation.GetTypeByMetadataName("System.Attribute");
            bool isAttributeType = classDeclaration.IsSubclassOf(attributeType);
            bool hasAttributeSuffix = classDeclaration.Name
                .EndsWith(attributeText, StringComparison.OrdinalIgnoreCase);
            if (isAttributeType)
            {
                if (!hasAttributeSuffix)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        AttributeTypeNamesShouldBeSuffixedWithAttributeDescriptor,
                        classDeclarationNode.Identifier.GetLocation()));
                }
            }
            else if (hasAttributeSuffix)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    NonAttributeTypeNamesShouldNotBeSuffixedWithAttributeDescriptor,
                    classDeclarationNode.Identifier.GetLocation()));
            }
        }

        private static void AnalyzeClassNameForExceptionSuffix(SyntaxNodeAnalysisContext context,
            ClassDeclarationSyntax classDeclarationNode, INamedTypeSymbol classDeclaration)
        {
            const string exceptionText = "Exception";
            var exceptionType = context.Compilation.GetTypeByMetadataName("System.Exception");
            bool isExceptionType = classDeclaration.IsSubclassOf(exceptionType);
            bool hasExceptionSuffix = classDeclaration.Name
                .EndsWith(exceptionText, StringComparison.OrdinalIgnoreCase);
            if (isExceptionType)
            {
                if (!hasExceptionSuffix)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        ExceptionTypeNamesShouldBeSuffixedWithExceptionDescriptor,
                        classDeclarationNode.Identifier.GetLocation()));
                }
            }
            else if (hasExceptionSuffix)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    NonExceptionTypeNamesShouldNotBeSuffixedWithExceptionDescriptor,
                    classDeclarationNode.Identifier.GetLocation()));
            }
        }

        private static void AnalyzeClassNameForEventArgsSuffix(SyntaxNodeAnalysisContext context,
            ClassDeclarationSyntax classDeclarationNode, INamedTypeSymbol classDeclaration)
        {
            const string eventArgsText = "EventArgs";
            var exceptionType = context.Compilation.GetTypeByMetadataName("System.EventArgs");
            bool isEventArgsType = classDeclaration.IsSubclassOf(exceptionType);
            bool hasEventArgsSuffix = classDeclaration.Name
                .EndsWith(eventArgsText, StringComparison.OrdinalIgnoreCase);
            if (isEventArgsType)
            {
                if (!hasEventArgsSuffix)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        EventArgsTypeNamesShouldBeSuffixedWithEventArgsDescriptor,
                        classDeclarationNode.Identifier.GetLocation()));
                }
            }
            else if (hasEventArgsSuffix)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    NonEventArgsTypeNamesShouldNotBeSuffixedWithEventArgsDescriptor,
                    classDeclarationNode.Identifier.GetLocation()));
            }
        }

        private static bool IsNameProbablyDescriptive(TypeParameterSyntax typeParameterNode)
        {
            if (typeParameterNode.Identifier.Text.Length >= 5)
            {
                return true;
            }

            switch (typeParameterNode.Identifier.Text)
            {
                case "TIn":
                case "TOut":
                case "TId":
                case "TKey":
                    return true;
            }

            return false;
        }
    }
}