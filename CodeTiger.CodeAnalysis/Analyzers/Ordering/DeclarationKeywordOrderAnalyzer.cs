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
    /// Analyzes the order of keywords in type and member declarations.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DeclarationKeywordOrderAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor DeclarationKeywordsShouldBeCorrectlyOrdered
            = new DiagnosticDescriptor("CT3217", "Declaration keywords should be correctly ordered.",
                "The {0} keyword should be before the {1} in declarations.", "CodeTiger.Ordering",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(DeclarationKeywordsShouldBeCorrectlyOrdered);
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

            context.RegisterSyntaxNodeAction(AnalyzeDeclaration, SyntaxKind.EnumDeclaration,
                SyntaxKind.InterfaceDeclaration, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration,
                SyntaxKind.FieldDeclaration, SyntaxKind.EventFieldDeclaration, SyntaxKind.PropertyDeclaration,
                SyntaxKind.IndexerDeclaration, SyntaxKind.EventDeclaration, SyntaxKind.DelegateDeclaration,
                SyntaxKind.ConstructorDeclaration, SyntaxKind.DestructorDeclaration, SyntaxKind.MethodDeclaration,
                SyntaxKind.ConversionOperatorDeclaration, SyntaxKind.OperatorDeclaration);
        }

        private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context)
        {
            SyntaxTokenList? modifiers;

            switch (context.Node.Kind())
            {
                case SyntaxKind.EnumDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                    modifiers = (context.Node as BaseTypeDeclarationSyntax)?.Modifiers;
                    break;
                case SyntaxKind.FieldDeclaration:
                case SyntaxKind.EventFieldDeclaration:
                    modifiers = (context.Node as BaseFieldDeclarationSyntax)?.Modifiers;
                    break;
                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.IndexerDeclaration:
                case SyntaxKind.EventDeclaration:
                    modifiers = (context.Node as BasePropertyDeclarationSyntax)?.Modifiers;
                    break;
                case SyntaxKind.DelegateDeclaration:
                    modifiers = (context.Node as DelegateDeclarationSyntax)?.Modifiers;
                    break;
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.OperatorDeclaration:
                    modifiers = (context.Node as BaseMethodDeclarationSyntax)?.Modifiers;
                    break;
                default:
                    modifiers = null;
                    break;
            }

            if (modifiers.HasValue && modifiers.Value.Count > 1)
            {
                AnalyzeModifiers(modifiers.Value, context);
            }
        }

        private static void AnalyzeModifiers(SyntaxTokenList modifiers, SyntaxNodeAnalysisContext context)
        {
            var encounteredModifiers = new Dictionary<SyntaxKind, int>();

            foreach (var token in modifiers)
            {
                int order = GetOrder(token.Kind());

                if (order >= 0)
                {
                    if (encounteredModifiers.Any(x => x.Value > order))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DeclarationKeywordsShouldBeCorrectlyOrdered,
                            token.GetLocation(), token.Kind().GetKeywordName(),
                            GetKeywordsOfHigherOrderedKinds(encounteredModifiers, order)));
                    }

                    if (!encounteredModifiers.Keys.Contains(token.Kind()))
                    {
                        encounteredModifiers.Add(token.Kind(), order);
                    }
                }
            }
        }

        private static int GetOrder(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PublicKeyword:
                case SyntaxKind.ProtectedKeyword:
                    return 0;
                case SyntaxKind.InternalKeyword:
                case SyntaxKind.PrivateKeyword:
                    return 1;
                case SyntaxKind.StaticKeyword:
                    return 2;
                case SyntaxKind.ConstKeyword:
                case SyntaxKind.ReadOnlyKeyword:
                case SyntaxKind.VolatileKeyword:
                    return 3;
                case SyntaxKind.NewKeyword:
                case SyntaxKind.SealedKeyword:
                case SyntaxKind.AbstractKeyword:
                    return 4;
                case SyntaxKind.VirtualKeyword:
                case SyntaxKind.OverrideKeyword:
                    return 5;
                case SyntaxKind.ExternKeyword:
                    return 6;
                case SyntaxKind.UnsafeKeyword:
                    return 7;
                case SyntaxKind.AsyncKeyword:
                    return 8;
                case SyntaxKind.ExplicitKeyword:
                case SyntaxKind.ImplicitKeyword:
                    return 9;
                case SyntaxKind.OperatorKeyword:
                    return 10;
                case SyntaxKind.PartialKeyword:
                    return 11;
                case SyntaxKind.EnumKeyword:
                case SyntaxKind.InterfaceKeyword:
                case SyntaxKind.ClassKeyword:
                case SyntaxKind.StructKeyword:
                case SyntaxKind.DelegateKeyword:
                case SyntaxKind.EventKeyword:
                    return 12;
                default:
                    return -1;
            }
        }

        private static string GetKeywordsOfHigherOrderedKinds(Dictionary<SyntaxKind, int> encounteredModifiers, int order)
        {
            var higherOrderedKinds = encounteredModifiers.Where(x => x.Value > order)
                .Select(x => x.Key)
                .ToList();

            switch (higherOrderedKinds.Count)
            {
                case 0:
                    throw new ArgumentOutOfRangeException("encounteredModifiers", "");
                case 1:
                    return higherOrderedKinds[0].GetKeywordName() + " keyword";
                case 2:
                    return higherOrderedKinds[0].GetKeywordName() + " or "
                        + higherOrderedKinds[1].GetKeywordName() + " keywords";
                default:
                    return string.Join(", ", higherOrderedKinds.Take(higherOrderedKinds.Count - 1)
                        .Select(SyntaxKindExtensions.GetKeywordName))
                        + ", or " + higherOrderedKinds.Last().GetKeywordName() + " keywords";
            }
        }
    }
}