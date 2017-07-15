using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeTiger.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeTiger.CodeAnalysis.Analyzers.Ordering
{
    /// <summary>
    /// Analyzes the order of type members in source code.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberOrderAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor MembersShouldBeCorrectlyOrderedBasedOnMemberKindDescriptor
            = new DiagnosticDescriptor("CT3200", "Members should be correctly ordered based on member kind.",
                "All {0} members should be before any {1} members.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ConstantFieldsShouldBeBeforeNonConstantFieldsDescriptor
            = new DiagnosticDescriptor("CT3201", "Constant fields should be before non-constant fields.",
                "Constant fields should be before non-constant fields.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor StaticFieldsShouldBeBeforeInstanceFieldsDescriptor
            = new DiagnosticDescriptor("CT3202", "Static fields should be before instance fields.",
                "Static fields should be before instance fields.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor ReadOnlyFieldsShouldBeBeforeMutableFieldsDescriptor
            = new DiagnosticDescriptor("CT3203", "Read-only fields should be before mutable fields.",
                "Read-only {0} fields should be before mutable {0} fields.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor NonVolatileFieldsShouldBeBeforeVolatileFieldsDescriptor
            = new DiagnosticDescriptor("CT3204", "Non-volatile fields should be before volatile fields.",
                "Non-volatile {0} fields should be before volatile {0} fields.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor StaticPropertiesShouldBeBeforeInstancePropertiesDescriptor
            = new DiagnosticDescriptor("CT3205", "Static properties should be before instance properties.",
                "Static properties should be before instance properties.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            StaticConstructorsShouldBeBeforeInstanceConstructorsDescriptor
            = new DiagnosticDescriptor("CT3206", "Static constructors should be before instance constructors.",
                "Static constructors should be before instance constructors.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor InstanceMethodsShouldBeBeforeStaticMethodsDescriptor
            = new DiagnosticDescriptor("CT3207", "Instance methods should be before static methods.",
                "Instance methods should be before static methods.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor
            NestedStaticClassesShouldShouldBeBeforeNestedInstanceClassesDescriptor
            = new DiagnosticDescriptor("CT3208", "Nested static classes should be before nested instance classes.",
                "Nested static classes should be before nested instance classes.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor MembersOfLikeKindShouldBeOrderedByDecreasingAccessibility
            = new DiagnosticDescriptor("CT3209",
                "Members of like kind should be ordered by decreasing accessibility.",
                "{0} members should be before {1} members of the same kind and with the same modifiers.",
                "CodeTiger.Ordering", DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(MembersShouldBeCorrectlyOrderedBasedOnMemberKindDescriptor,
                    MembersOfLikeKindShouldBeOrderedByDecreasingAccessibility,
                    ConstantFieldsShouldBeBeforeNonConstantFieldsDescriptor,
                    StaticFieldsShouldBeBeforeInstanceFieldsDescriptor,
                    ReadOnlyFieldsShouldBeBeforeMutableFieldsDescriptor,
                    NonVolatileFieldsShouldBeBeforeVolatileFieldsDescriptor,
                    StaticPropertiesShouldBeBeforeInstancePropertiesDescriptor,
                    StaticConstructorsShouldBeBeforeInstanceConstructorsDescriptor,
                    InstanceMethodsShouldBeBeforeStaticMethodsDescriptor,
                    NestedStaticClassesShouldShouldBeBeforeNestedInstanceClassesDescriptor);
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

            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTreeForMemberOrder);
            context.RegisterSemanticModelAction(AnalyzeSemanticModelForMemberOrder);
        }

        private static void AnalyzeSyntaxTreeForMemberOrder(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);

            foreach (var node in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                AnalyzeMemberOrderBasedOnMemberKind(node, context);
                AnalyzeFieldOrderBasedOnConst(node, context);
                AnalyzeFieldOrderBasedOnStatic(node, context);
                AnalyzeFieldOrderBasedOnReadOnly(node, context);
                AnalyzeFieldOrderBasedOnVolatile(node, context);
                AnalyzePropertyOrderBasedOnStatic(node, context);
                AnalyzeConstructorOrderBasedOnStatic(node, context);
                AnalyzeMethodOrderBasedOnStatic(node, context);
                AnalyzeNestedClassOrderBasedOnStatic(node, context);
            }
        }

        private void AnalyzeSemanticModelForMemberOrder(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            foreach (var node in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                AnalyzeMemberOrderBasedOnAccessibility(node, context);
            }
        }

        private static void AnalyzeMemberOrderBasedOnMemberKind(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            var encounteredMemberKinds = new Dictionary<SyntaxKind, int>();

            foreach (var member in node.Members)
            {
                int memberOrder = GetOrderForDeclarationKind(member.Kind());
                if (memberOrder < 0)
                {
                    // If -1 is returned by GetOrder, the member kind should be ignored
                    continue;
                }

                if (encounteredMemberKinds.Values.Any(x => x > memberOrder))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        MembersShouldBeCorrectlyOrderedBasedOnMemberKindDescriptor, member.GetIdentifierLocation(),
                        member.Kind().GetDeclarationName(),
                        GetDisplayNamesOfHigherOrderedKinds(encounteredMemberKinds, memberOrder)));
                }

                if (!encounteredMemberKinds.Keys.Contains(member.Kind()))
                {
                    encounteredMemberKinds.Add(member.Kind(), memberOrder);
                }
            }
        }

        private static void AnalyzeMemberOrderBasedOnAccessibility(TypeDeclarationSyntax node,
            SemanticModelAnalysisContext context)
        {
            var encounteredKinds = new Dictionary<long, List<Accessibility>>();

            foreach (var member in node.Members)
            {
                var fieldMember = member as BaseFieldDeclarationSyntax;
                if (fieldMember != null)
                {
                    var fieldMemberVariable = fieldMember.Declaration.Variables.FirstOrDefault();
                    if (fieldMemberVariable != null)
                    {
                        var symbolInfo = context.SemanticModel.GetDeclaredSymbol(fieldMemberVariable,
                            context.CancellationToken);

                        AnalyzeSymbolOrderBasedOnAccessibility(fieldMember, symbolInfo, context,
                            encounteredKinds);
                    }
                }
                else if (member.Kind() != SyntaxKind.IncompleteMember)
                {
                    var symbolInfo = context.SemanticModel.GetDeclaredSymbol(member, context.CancellationToken);
                    AnalyzeSymbolOrderBasedOnAccessibility(member, symbolInfo, context, encounteredKinds);
                }
            }
        }

        private static void AnalyzeSymbolOrderBasedOnAccessibility(MemberDeclarationSyntax member,
            ISymbol memberSymbol, SemanticModelAnalysisContext context,
            Dictionary<long, List<Accessibility>> encounteredKinds)
        {
            var symbolAccessibility = memberSymbol.DeclaredAccessibility;
            var order = GetOrderForAccessibility(symbolAccessibility);

            long kindDescriptor = GetMemberKindDescriptor(member);
            if (encounteredKinds.ContainsKey(kindDescriptor))
            {
                if (encounteredKinds[kindDescriptor].Any(x => GetOrderForAccessibility(x) > order))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        MembersOfLikeKindShouldBeOrderedByDecreasingAccessibility, member.GetIdentifierLocation(),
                        symbolAccessibility,
                        GetDisplayNamesOfHigherOrderedAccessibilities(encounteredKinds[kindDescriptor],
                        symbolAccessibility)));
                }

                if (!encounteredKinds[kindDescriptor].Contains(symbolAccessibility))
                {
                    encounteredKinds[kindDescriptor].Add(symbolAccessibility);
                }
            }
            else
            {
                encounteredKinds.Add(kindDescriptor, new List<Accessibility> { symbolAccessibility });
            }
        }

        private static void AnalyzeFieldOrderBasedOnConst(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            bool wasNonConstFieldEncountered = false;

            foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
            {
                if (field.Modifiers.Any(t => t.Kind() == SyntaxKind.ConstKeyword))
                {
                    if (wasNonConstFieldEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            ConstantFieldsShouldBeBeforeNonConstantFieldsDescriptor,
                            field.GetIdentifierLocation()));
                    }
                }
                else
                {
                    wasNonConstFieldEncountered = true;
                }
            }
        }

        private static void AnalyzeFieldOrderBasedOnStatic(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            bool wasInstanceFieldEncountered = false;

            foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
            {
                if (field.Modifiers.Any(t => t.Kind() == SyntaxKind.StaticKeyword))
                {
                    if (wasInstanceFieldEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            StaticFieldsShouldBeBeforeInstanceFieldsDescriptor, field.GetIdentifierLocation()));
                    }
                }
                else if (!field.Modifiers.Any(t => t.Kind() == SyntaxKind.ConstKeyword))
                {
                    wasInstanceFieldEncountered = true;
                }
            }
        }

        private static void AnalyzeFieldOrderBasedOnReadOnly(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            bool wasStaticMutableFieldEncountered = false;
            bool wasInstanceMutableFieldEncountered = false;

            foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
            {
                bool isFieldStatic = field.Modifiers.Any(t => t.Kind() == SyntaxKind.StaticKeyword);

                if (field.Modifiers.Any(t => t.Kind() == SyntaxKind.ReadOnlyKeyword))
                {
                    if (isFieldStatic && wasStaticMutableFieldEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            ReadOnlyFieldsShouldBeBeforeMutableFieldsDescriptor, field.GetIdentifierLocation(),
                            "static"));
                    }
                    else if (!isFieldStatic && wasInstanceMutableFieldEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            ReadOnlyFieldsShouldBeBeforeMutableFieldsDescriptor, field.GetIdentifierLocation(),
                            "instance"));
                    }
                }
                else if (!field.Modifiers.Any(t => t.Kind() == SyntaxKind.ConstKeyword))
                {
                    if (isFieldStatic)
                    {
                        wasStaticMutableFieldEncountered = true;
                    }
                    else
                    {
                        wasInstanceMutableFieldEncountered = true;
                    }
                }
            }
        }

        private static void AnalyzeFieldOrderBasedOnVolatile(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            bool wasStaticVolatileFieldEncountered = false;
            bool wasInstanceVolatileFieldEncountered = false;

            foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
            {
                bool isFieldStatic = field.Modifiers.Any(t => t.Kind() == SyntaxKind.StaticKeyword);

                if (!field.Modifiers.Any(t => t.Kind() == SyntaxKind.VolatileKeyword))
                {
                    if (isFieldStatic && wasStaticVolatileFieldEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            NonVolatileFieldsShouldBeBeforeVolatileFieldsDescriptor, field.GetIdentifierLocation(),
                            "static"));
                    }
                    else if (!isFieldStatic && wasInstanceVolatileFieldEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            NonVolatileFieldsShouldBeBeforeVolatileFieldsDescriptor, field.GetIdentifierLocation(),
                            "instance"));
                    }
                }
                else if (!field.Modifiers.Any(t => t.Kind() == SyntaxKind.ConstKeyword))
                {
                    if (isFieldStatic)
                    {
                        wasStaticVolatileFieldEncountered = true;
                    }
                    else
                    {
                        wasInstanceVolatileFieldEncountered = true;
                    }
                }
            }
        }

        private static void AnalyzePropertyOrderBasedOnStatic(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            bool isInstancePropertyEncountered = false;

            foreach (var property in node.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (property.Modifiers.Any(t => t.Kind() == SyntaxKind.StaticKeyword))
                {
                    if (isInstancePropertyEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            StaticPropertiesShouldBeBeforeInstancePropertiesDescriptor,
                            property.GetIdentifierLocation()));
                    }
                }
                else
                {
                    isInstancePropertyEncountered = true;
                }
            }
        }

        private static void AnalyzeConstructorOrderBasedOnStatic(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            bool isInstanceConstructorEncountered = false;

            foreach (var constructor in node.Members.OfType<ConstructorDeclarationSyntax>())
            {
                if (constructor.Modifiers.Any(t => t.Kind() == SyntaxKind.StaticKeyword))
                {
                    if (isInstanceConstructorEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            StaticConstructorsShouldBeBeforeInstanceConstructorsDescriptor,
                            constructor.GetIdentifierLocation()));
                    }
                }
                else
                {
                    isInstanceConstructorEncountered = true;
                }
            }
        }

        private static void AnalyzeMethodOrderBasedOnStatic(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            bool isStaticMethodEncountered = false;

            foreach (var method in node.Members.OfType<BaseMethodDeclarationSyntax>())
            {
                switch (method.Kind())
                {
                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKind.OperatorDeclaration:
                    case SyntaxKind.ConversionOperatorDeclaration:
                        break;
                    default:
                        continue;
                }

                if (!method.Modifiers.Any(t => t.Kind() == SyntaxKind.StaticKeyword))
                {
                    if (isStaticMethodEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            InstanceMethodsShouldBeBeforeStaticMethodsDescriptor, method.GetIdentifierLocation()));
                    }
                }
                else
                {
                    isStaticMethodEncountered = true;
                }
            }
        }

        private static void AnalyzeNestedClassOrderBasedOnStatic(TypeDeclarationSyntax node,
            SyntaxTreeAnalysisContext context)
        {
            bool isInstanceClassEncountered = false;

            foreach (var classDeclaration in node.Members.OfType<ClassDeclarationSyntax>())
            {
                if (classDeclaration.Modifiers.Any(t => t.Kind() == SyntaxKind.StaticKeyword))
                {
                    if (isInstanceClassEncountered)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            NestedStaticClassesShouldShouldBeBeforeNestedInstanceClassesDescriptor,
                            classDeclaration.GetIdentifierLocation()));
                    }
                }
                else
                {
                    isInstanceClassEncountered = true;
                }
            }
        }

        private static SyntaxTokenList? GetModifiers(SyntaxNode member)
        {
            var currentMember = member;
            while (currentMember != null)
            {
                switch (currentMember.Kind())
                {
                    case SyntaxKind.DelegateDeclaration:
                        return (currentMember as DelegateDeclarationSyntax)?.Modifiers;
                    case SyntaxKind.EnumDeclaration:
                    case SyntaxKind.InterfaceDeclaration:
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.StructDeclaration:
                        return (currentMember as BaseTypeDeclarationSyntax)?.Modifiers;
                    case SyntaxKind.FieldDeclaration:
                    case SyntaxKind.EventFieldDeclaration:
                        return (currentMember as BaseFieldDeclarationSyntax)?.Modifiers;
                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKind.OperatorDeclaration:
                    case SyntaxKind.ConversionOperatorDeclaration:
                    case SyntaxKind.ConstructorDeclaration:
                    case SyntaxKind.DestructorDeclaration:
                        return (currentMember as BaseMethodDeclarationSyntax)?.Modifiers;
                    case SyntaxKind.PropertyDeclaration:
                    case SyntaxKind.EventDeclaration:
                    case SyntaxKind.IndexerDeclaration:
                        return (currentMember as BasePropertyDeclarationSyntax)?.Modifiers;
                    case SyntaxKind.IncompleteMember:
                        return (currentMember as IncompleteMemberSyntax)?.Modifiers;
                }

                currentMember = currentMember.Parent;
            }

            return null;
        }

        private static int GetOrderForDeclarationKind(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.FieldDeclaration:
                    return 0;
                case SyntaxKind.EventFieldDeclaration:
                    return 1;
                case SyntaxKind.PropertyDeclaration:
                    return 2;
                case SyntaxKind.IndexerDeclaration:
                    return 3;
                case SyntaxKind.DelegateDeclaration:
                    return 4;
                case SyntaxKind.EventDeclaration:
                    return 5;
                case SyntaxKind.ConstructorDeclaration:
                    return 6;
                case SyntaxKind.DestructorDeclaration:
                    return 7;
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.OperatorDeclaration:
                    return 8;
                case SyntaxKind.EnumDeclaration:
                    return 9;
                case SyntaxKind.InterfaceDeclaration:
                    return 10;
                case SyntaxKind.ClassDeclaration:
                    return 11;
                case SyntaxKind.StructDeclaration:
                    return 12;
                default:
                    return -1;
            }
        }

        private static int GetOrderForAccessibility(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Public:
                    return 0;
                case Accessibility.NotApplicable: // Includes explicitly implemented interface members
                    return 1;
                case Accessibility.ProtectedOrInternal:
                    return 2;
                case Accessibility.Protected:
                    return 3;
                case Accessibility.Internal:
                    return 4;
                case Accessibility.ProtectedAndInternal:
                    return 5;
                case Accessibility.Private:
                    return 6;
                default:
                    return -1;
            }
        }

        private static string GetDisplayNamesOfHigherOrderedKinds(Dictionary<SyntaxKind, int> encounteredKinds,
            int memberOrder)
        {
            var higherOrderedKinds = encounteredKinds.Where(x => x.Value > memberOrder)
                .Select(x => x.Key)
                .ToList();

            switch (higherOrderedKinds.Count)
            {
                case 0:
                    throw new ArgumentOutOfRangeException("encounteredKinds");
                case 1:
                    return higherOrderedKinds[0].GetDeclarationName();
                case 2:
                    return higherOrderedKinds[0].GetDeclarationName() + " or "
                        + higherOrderedKinds[1].GetDeclarationName();
                default:
                    return string.Join(", ", higherOrderedKinds.Take(higherOrderedKinds.Count - 1)
                        .Select(SyntaxKindExtensions.GetDeclarationName))
                        + ", or " + higherOrderedKinds.Last().GetDeclarationName();
            }
        }

        private static string GetDisplayNamesOfHigherOrderedAccessibilities(
            List<Accessibility> encounteredAccessibilities, Accessibility memberAccessibility)
        {
            var order = GetOrderForAccessibility(memberAccessibility);
            var higherOrderedAccessibilities = encounteredAccessibilities
                .Where(x => GetOrderForAccessibility(x) > order)
                .ToList();

            switch (higherOrderedAccessibilities.Count)
            {
                case 0:
                    throw new ArgumentOutOfRangeException("encounteredAccessibilities");
                case 1:
                    return higherOrderedAccessibilities[0].ToString();
                case 2:
                    return higherOrderedAccessibilities[0].ToString() + " or "
                        + higherOrderedAccessibilities[1].ToString();
                default:
                    return string.Join(", ",
                        higherOrderedAccessibilities.Take(higherOrderedAccessibilities.Count - 1))
                        + ", or " + higherOrderedAccessibilities.Last().ToString();
            }
        }

        private static long GetMemberKindDescriptor(SyntaxNode member)
        {
            bool isStatic = false;
            bool isConst = false;
            bool isReadOnly = false;
            bool isVolatile = false;

            var modifiers = GetModifiers(member);
            if (modifiers.HasValue)
            {
                foreach (var modifier in modifiers)
                {
                    switch (modifier.Kind())
                    {
                        case SyntaxKind.StaticKeyword:
                            isStatic = true;
                            break;
                        case SyntaxKind.ConstKeyword:
                            isConst = true;
                            break;
                        case SyntaxKind.ReadOnlyKeyword:
                            isReadOnly = true;
                            break;
                        case SyntaxKind.VolatileKeyword:
                            isVolatile = true;
                            break;
                    }
                }
            }

            var declarationMember = member;
            while (declarationMember != null
                && !(declarationMember is MemberDeclarationSyntax)
                && declarationMember.Parent != null)
            {
                declarationMember = declarationMember.Parent;
            }

            return GetMemberKindDescriptor((declarationMember ?? member).Kind(), isStatic, isConst, isReadOnly,
                isVolatile);
        }

        private static long GetMemberKindDescriptor(SyntaxKind kind, bool isStatic, bool isConst, bool isReadOnly,
            bool isVolatile)
        {
            byte[] packedKindOrderBytes = BitConverter.GetBytes((int)kind);
            byte[] memberKindDescriptorBytes = new byte[8]
                {
                    packedKindOrderBytes[0],
                    packedKindOrderBytes[1],
                    packedKindOrderBytes[2],
                    packedKindOrderBytes[3],
                    isStatic ? (byte)1 : (byte)0,
                    isConst ? (byte)1 : (byte)0,
                    isReadOnly ? (byte)1 : (byte)0,
                    isVolatile ? (byte)1 : (byte)0
                };

            return BitConverter.ToInt64(memberKindDescriptorBytes, 0);
        }
    }
}