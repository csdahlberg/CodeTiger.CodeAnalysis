using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
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

        private static readonly ReadOnlyCollection<string> _knownPartialFileNames
            = new List<string> { "aspx", "xaml", "cshtml" }.AsReadOnly();

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

        private static void AnalyzeFileName(SyntaxTreeAnalysisContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Tree.FilePath))
            {
                return;
            }
            
            string[] fileNamePartsWithoutExtension = Path.GetFileNameWithoutExtension(context.Tree.FilePath)
                .Split('.')
                .ToArray();
            for (int i = 0; i < fileNamePartsWithoutExtension.Length; i += 1)
            {
                string fileNamePart = fileNamePartsWithoutExtension[i];

                if (NamingUtility.IsProbablyPascalCased(fileNamePart, true) == false
                    && (i < fileNamePartsWithoutExtension.Length - 1
                        || !_knownPartialFileNames.Contains(fileNamePart, StringComparer.Ordinal)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(SourceFileNamesShouldUsePascalCasingDescriptor,
                        Location.Create(context.Tree, TextSpan.FromBounds(0, 0))));
                }
            }

            var firstTypeDeclarationNode = context.Tree.GetRoot(context.CancellationToken)
                .DescendantNodes()
                .OfType<BaseTypeDeclarationSyntax>()
                .FirstOrDefault();
            if (firstTypeDeclarationNode != null)
            {
                string[] firstDeclaredTypeNames;

                var firstTypeDeclarationNode2 = firstTypeDeclarationNode as TypeDeclarationSyntax;
                if (firstTypeDeclarationNode2?.TypeParameterList?.Parameters.Any() == true)
                {
                    // Allow filenames for generic types to either omit or include the type arity
                    firstDeclaredTypeNames = new[]
                    {
                        firstTypeDeclarationNode.Identifier.ValueText,
                        firstTypeDeclarationNode.Identifier.ValueText + "`"
                            + firstTypeDeclarationNode2.TypeParameterList.Parameters.Count
                            .ToString(CultureInfo.InvariantCulture),
                    };
                }
                else
                {
                    firstDeclaredTypeNames = new[] { firstTypeDeclarationNode.Identifier.ValueText };
                }

                if (!firstDeclaredTypeNames
                    .Any(x => DoesTypeNameMatchFileName(x, fileNamePartsWithoutExtension[0])))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        SourceFileNamesShouldMatchThePrimaryTypeNameDescriptor,
                        Location.Create(context.Tree, TextSpan.FromBounds(0, 0))));
                }
            }
        }

        private static bool DoesTypeNameMatchFileName(string typeName, string fileNamePart)
        {
            string typeNameAlphaNumeric = new string(typeName.Where(char.IsLetterOrDigit).ToArray());
            string fileNamePartAlphaNumeric = new string(fileNamePart.Where(char.IsLetterOrDigit).ToArray());

            return string.Equals(typeNameAlphaNumeric, fileNamePartAlphaNumeric, StringComparison.Ordinal);
        }
    }
}
