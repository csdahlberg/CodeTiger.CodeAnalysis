using System.Collections.Immutable;
using System.Linq;
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
        internal static readonly DiagnosticDescriptor NonGenericdCollectionsShouldNotBeHeldAsStateDescriptor
            = new DiagnosticDescriptor("CT1004", "Non-generic collections should not be held as state.",
                "Non-generic collections should not be held as state.", "CodeTiger.Design",
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
                return ImmutableArray.Create(NonGenericdCollectionsShouldNotBeHeldAsStateDescriptor);
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

            foreach (var nonGenericCollectionMember in members.Where(x => IsNonGenericCollection(context, x)))
            {
                context.ReportDiagnostic(Diagnostic.Create(NonGenericdCollectionsShouldNotBeHeldAsStateDescriptor,
                    GetIdentifierLocation(nonGenericCollectionMember)));
            }
        }

        private static bool IsNonGenericCollection(SyntaxNodeAnalysisContext context,
            MemberDeclarationSyntax member)
        {
            ITypeSymbol memberType;

            switch (member.Kind())
            {
                case SyntaxKind.FieldDeclaration:
                    memberType = context.SemanticModel
                        .GetTypeInfo(((FieldDeclarationSyntax)member).Declaration.Type, context.CancellationToken)
                        .Type;
                    break;
                case SyntaxKind.PropertyDeclaration:
                    {
                        var propertyNode = (PropertyDeclarationSyntax)member;
                        if (propertyNode.AccessorList.Accessors.Any(x => x.Body != null))
                        {
                            return false;
                        }

                        memberType = context.SemanticModel
                            .GetTypeInfo(propertyNode.Type, context.CancellationToken).Type;
                    }
                    break;
                default:
                    return false;
            }

            foreach (string nonGenericCollectionName in _nonGenericCollectionMetadataNames)
            {
                var nonGenericSymbol = context.SemanticModel.Compilation
                    .GetTypeByMetadataName(nonGenericCollectionName);
                if (nonGenericSymbol != null && nonGenericSymbol.Equals(memberType))
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