using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeTiger.CodeAnalysis.Analyzers.Naming
{
    /// <summary>
    /// Analyzes file names.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileNamingAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor SourceFileNamesShouldUsePascalCasingDescriptor
            = new DiagnosticDescriptor("CT1701", "Source file names should use pascal casing.",
                "Source file names should use pascal casing.", "CodeTiger.Naming", DiagnosticSeverity.Warning,
                true);
        internal static readonly DiagnosticDescriptor SourceFileNamesShouldMatchThePrimaryTypeNameDescriptor
            = new DiagnosticDescriptor("CT1729", "Source file names should match the primary type name.",
                "Source file names should match the primary type name.", "CodeTiger.Naming",
                DiagnosticSeverity.Warning, true);

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(SourceFileNamesShouldUsePascalCasingDescriptor,
                    SourceFileNamesShouldMatchThePrimaryTypeNameDescriptor);
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

            context.RegisterSyntaxTreeAction(AnalyzeFileName);
        }

        private void AnalyzeFileName(SyntaxTreeAnalysisContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Tree.FilePath))
            {
                return;
            }

            string[] fileNamePartsWithoutExtension = Path.GetFileNameWithoutExtension(context.Tree.FilePath)
                .Split('.')
                .ToArray();
            foreach (string fileNamePart in fileNamePartsWithoutExtension)
            {
                if (NamingUtility.IsProbablyPascalCased(fileNamePart) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(SourceFileNamesShouldUsePascalCasingDescriptor,
                        Location.Create(context.Tree, TextSpan.FromBounds(0, 0))));
                }
            }

            var firstTypeDeclarationNode = context.Tree.GetRoot(context.CancellationToken)
                .DescendantNodes()
                .FirstOrDefault(IsTypeDeclaration);
            if (firstTypeDeclarationNode != null)
            {
                string firstDeclaredTypeName = ((BaseTypeDeclarationSyntax)firstTypeDeclarationNode)
                    .Identifier.Text;

                if (!string.Equals(firstDeclaredTypeName, fileNamePartsWithoutExtension[0]))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        SourceFileNamesShouldMatchThePrimaryTypeNameDescriptor,
                        Location.Create(context.Tree, TextSpan.FromBounds(0, 0))));
                }
            }
        }

        private static bool IsTypeDeclaration(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.EnumDeclaration:
                    return true;
                default:
                    return false;
            }
        }
    }
}
