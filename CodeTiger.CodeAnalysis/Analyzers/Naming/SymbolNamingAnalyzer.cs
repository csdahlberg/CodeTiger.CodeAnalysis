using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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
        internal static readonly DiagnosticDescriptor TypeNamesShouldNotMatchTheirContainingNamespaceNameDescriptor
            = new DiagnosticDescriptor("CT1730", "Type names should not match their containing namespace name.",
                "Type names should not match their containing namespace name.", "CodeTiger.Naming",
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
                    MethodsNotReturningATaskOrVoidShouldNotBeSuffixedWithAsyncDescriptor,
                    TypeNamesShouldNotMatchTheirContainingNamespaceNameDescriptor);
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

        private static void AnalyzeTypeName(SyntaxNodeAnalysisContext context)
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

            if (NamingUtility.IsProbablyPascalCased(typeIdentifier.ValueText) == false
                || NamingUtility.IsProbablyPascalCased(typeIdentifier.ValueText, 'I') == true)
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeNamesShouldUsePascalCasingDescriptor,
                    typeIdentifier.GetLocation()));
            }

            AnalyzeTypeNameAndContainingNamespaceName(context, typeIdentifier);
        }

        private static void AnalyzeFieldName(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclarationNode = (FieldDeclarationSyntax)context.Node;
            if (fieldDeclarationNode.Modifiers.Any(x => x.Kind() == SyntaxKind.ConstKeyword))
            {
                foreach (var fieldDeclaration in fieldDeclarationNode.Declaration.Variables)
                {
                    if (NamingUtility.IsProbablyPascalCased(fieldDeclaration.Identifier.ValueText) == false)
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
                    if (NamingUtility.IsProbablyCamelCased(fieldDeclaration.Identifier.ValueText, '_') == false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(PrivateFieldNamesShouldUseCamelCasingDescriptor,
                            fieldDeclaration.Identifier.GetLocation()));
                    }
                }
            }
        }

        private static void AnalyzeEventFieldName(SyntaxNodeAnalysisContext context)
        {
            var eventFieldDeclarationNode = (EventFieldDeclarationSyntax)context.Node;
            foreach (var eventFieldDeclaration in eventFieldDeclarationNode.Declaration.Variables)
            {
                if (NamingUtility.IsProbablyPascalCased(eventFieldDeclaration.Identifier.ValueText) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(EventNamesShouldUsePascalCasingDescriptor,
                        eventFieldDeclaration.Identifier.GetLocation()));
                }
            }
        }

        private static void AnalyzeDelegateName(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclarationNode = (DelegateDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(delegateDeclarationNode.Identifier.ValueText) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(DelegateNamesShouldUsePascalCasingDescriptor,
                    delegateDeclarationNode.Identifier.GetLocation()));
            }
        }

        private static void AnalyzePropertyName(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclarationNode = (PropertyDeclarationSyntax)context.Node;

            string propertyName = propertyDeclarationNode.Identifier.ValueText;

            if (NamingUtility.IsProbablyPascalCased(propertyName) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyNamesShouldUsePascalCasingDescriptor,
                    propertyDeclarationNode.Identifier.GetLocation()));
            }

            if (propertyName.StartsWith("Get", StringComparison.OrdinalIgnoreCase)
                || propertyName.StartsWith("Set", StringComparison.OrdinalIgnoreCase))
            {
                string propertyNameWithoutPrefix = propertyName.Substring(3);

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
                    AnalyzePropertyNameForPrefixMatchingParentName(context, propertyDeclarationNode);
                    break;
            }
        }

        private static void AnalyzePropertyNameForPrefixMatchingParentName(SyntaxNodeAnalysisContext context,
            PropertyDeclarationSyntax propertyDeclarationNode)
        {
            var parentIdentifier = ((BaseTypeDeclarationSyntax)propertyDeclarationNode.Parent)
                .Identifier;
            if (!propertyDeclarationNode.Identifier.ValueText
                .StartsWith(parentIdentifier.ValueText, StringComparison.OrdinalIgnoreCase))
            {
                // The property name does not start with the name of the containing type, so it is not a violation
                return;
            }

            var propertySymbol = context.SemanticModel
                .GetDeclaredSymbol(propertyDeclarationNode, context.CancellationToken);
            if (propertySymbol?.ExplicitInterfaceImplementations.Any() == true)
            {
                // The property is an explicit implementation, so it cannot simply be renamed in this class
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                PropertyNamesShouldNotBeginWithTheNameOfTheContainingingTypeDescriptor,
                propertyDeclarationNode.Identifier.GetLocation()));
        }

        private static void AnalyzeMethodName(SyntaxNodeAnalysisContext context)
        {
            var methodDeclarationNode = (MethodDeclarationSyntax)context.Node;
            var methodDeclaration = context.SemanticModel.GetDeclaredSymbol(methodDeclarationNode,
                context.CancellationToken);

            if (NamingUtility.IsProbablyPascalCased(methodDeclarationNode.Identifier.ValueText) == false)
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
            var taskType = context.Compilation.GetTypeByMetadataName(typeof(Task).FullName);
            bool returnsTaskType = methodDeclaration.ReturnType.IsSameOrSubclassOf(taskType)
                || methodDeclaration.ReturnType.IsConstrainedTo(taskType);
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

        private static void AnalyzeEnumerationMemberName(SyntaxNodeAnalysisContext context)
        {
            var enumMemberDeclarationNode = (EnumMemberDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(enumMemberDeclarationNode.Identifier.ValueText) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(EnumerationMemberNamesShouldUsePascalCasingDescriptor,
                    enumMemberDeclarationNode.Identifier.GetLocation()));
            }
        }

        private static void AnalyzeLocalVariableNames(SyntaxNodeAnalysisContext context)
        {
            var localDeclarationSyntax = (LocalDeclarationStatementSyntax)context.Node;
            
            foreach (var variableDeclaratorSyntax in localDeclarationSyntax.Declaration.Variables)
            {
                if (NamingUtility.IsProbablyCamelCased(variableDeclaratorSyntax.Identifier.ValueText) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(VariableNamesShouldUseCamelCasingDescriptor,
                        variableDeclaratorSyntax.Identifier.GetLocation()));
                }
            }
        }

        private static void AnalyzeInterfaceName(SyntaxNodeAnalysisContext context)
        {
            var interfaceDeclarationNode = (InterfaceDeclarationSyntax)context.Node;

            if (NamingUtility.IsProbablyPascalCased(interfaceDeclarationNode.Identifier.ValueText, 'I') == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InterfaceNamesShouldUsePascalCasingPrefixedWithIDescriptor,
                    interfaceDeclarationNode.Identifier.GetLocation()));
            }

            AnalyzeTypeNameAndContainingNamespaceName(context, interfaceDeclarationNode.Identifier);
        }

        private static void AnalyzeParameterName(SyntaxNodeAnalysisContext context)
        {
            var parameterNode = (ParameterSyntax)context.Node;
            
            if (NamingUtility.IsProbablyCamelCased(parameterNode.Identifier.ValueText) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(ParameterNamesShouldUseCamelCasingDescriptor,
                    parameterNode.Identifier.GetLocation()));
            }
        }

        private static void AnalyzeGenericTypeParameterName(SyntaxNodeAnalysisContext context)
        {
            var typeParameterNode = (TypeParameterSyntax)context.Node;

            if (typeParameterNode.Identifier.ValueText == "T"
                || (typeParameterNode.Identifier.ValueText.StartsWith("T", StringComparison.Ordinal)
                    && int.TryParse(typeParameterNode.Identifier.ValueText.Substring(1), out int x)))
            {
                return;
            }

            if (NamingUtility.IsProbablyPascalCased(typeParameterNode.Identifier.ValueText, 'T') == false)
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

                if (typeParameterNode.Identifier.ValueText.EndsWith("Type", StringComparison.OrdinalIgnoreCase))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        GenericTypeParameterNamesShouldNotBeSuffixedWithTypeDescriptor,
                        typeParameterNode.Identifier.GetLocation()));
                }
            }
        }

        private static void AnalyzeClassNamePrefixesAndSuffixes(SyntaxNodeAnalysisContext context)
        {
            var classDeclarationNode = (ClassDeclarationSyntax)context.Node;
            var classDeclaration = context.SemanticModel.GetDeclaredSymbol(classDeclarationNode,
                context.CancellationToken);
            string className = classDeclarationNode.Identifier.ValueText;

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
            var attributeType = context.Compilation.GetTypeByMetadataName(typeof(Attribute).FullName);
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
            var exceptionType = context.Compilation.GetTypeByMetadataName(typeof(Exception).FullName);
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
            var exceptionType = context.Compilation.GetTypeByMetadataName(typeof(EventArgs).FullName);
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

        private static void AnalyzeTypeNameAndContainingNamespaceName(SyntaxNodeAnalysisContext context,
            SyntaxToken typeIdentifier)
        {
            var containingNode = context.Node.Parent;
            if (containingNode?.Kind() == SyntaxKind.NamespaceDeclaration)
            {
                var containingNamespaceDeclarationNode = (NamespaceDeclarationSyntax)context.Node.Parent;
                if (string.Equals(typeIdentifier.ValueText,
                    containingNamespaceDeclarationNode.Name.GetUnqualifiedName()?.Identifier.ValueText,
                    StringComparison.Ordinal))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        TypeNamesShouldNotMatchTheirContainingNamespaceNameDescriptor,
                        typeIdentifier.GetLocation()));
                }
            }
        }

        private static bool IsNameProbablyDescriptive(TypeParameterSyntax typeParameterNode)
        {
            if (typeParameterNode.Identifier.ValueText.Length >= 5)
            {
                return true;
            }

            switch (typeParameterNode.Identifier.ValueText)
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