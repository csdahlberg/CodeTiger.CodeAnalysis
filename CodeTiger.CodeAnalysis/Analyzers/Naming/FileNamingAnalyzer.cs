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

            AnalyzeNameOfFirstType(context, fileNamePartsWithoutExtension);
        }

        private static void AnalyzeNameOfFirstType(SyntaxTreeAnalysisContext context,
            string[] fileNamePartsWithoutExtension)
        {
            var firstTypeDeclarationNode = context.Tree.GetRoot(context.CancellationToken)
                .DescendantNodes()
                .OfType<BaseTypeDeclarationSyntax>()
                .FirstOrDefault();

            if (firstTypeDeclarationNode == null)
            {
                return;
            }

            string firstTypeName = firstTypeDeclarationNode.Identifier.ValueText;
            string firstFileNamePart = fileNamePartsWithoutExtension[0];

            if (DoesTypeNameMatchFileName(firstTypeName, firstFileNamePart))
            {
                return;
            }

            if (fileNamePartsWithoutExtension.Length >= 2
                && string.Equals("cshtml", fileNamePartsWithoutExtension[^1]))
            {
                // Having PageModel types in the code-behind file for Razor pages is the recommended approach, so
                // allow '{PageName}Model' types to be in '{PageName}.cshtml.cs' files.
                string razorModelName = firstFileNamePart + "Model";
                if (DoesTypeNameMatchFileName(firstTypeName, razorModelName))
                {
                    return;
                }
            }

            var firstTypeDeclarationNode2 = firstTypeDeclarationNode as TypeDeclarationSyntax;
            if (firstTypeDeclarationNode2?.TypeParameterList?.Parameters.Any() == true)
            {
                if (DoesTypeNameMatchFileName(firstTypeName, firstFileNamePart,
                    firstTypeDeclarationNode2.TypeParameterList.Parameters.Count))
                {
                    return;
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(
                SourceFileNamesShouldMatchThePrimaryTypeNameDescriptor,
                Location.Create(context.Tree, TextSpan.FromBounds(0, 0))));
        }

        private static bool DoesTypeNameMatchFileName(string typeName, string fileNamePart, int? arity = null)
        {
            int typeNameIndex = 0;
            int fileNameIndex = 0;

            while (typeNameIndex < typeName.Length && fileNameIndex < fileNamePart.Length)
            {
                char fileNameChar = fileNamePart[fileNameIndex];

                if (typeName[typeNameIndex] != fileNameChar)
                {
                    // Allow for things like a 'Class1Tests' class in a 'Class_1Tests.cs' file
                    if (fileNameChar == '`' || fileNameChar == '_')
                    {
                        fileNameIndex += 1;
                        continue;
                    }

                    return false;
                }

                typeNameIndex += 1;
                fileNameIndex += 1;
            }

            if (typeNameIndex == typeName.Length && fileNameIndex == fileNamePart.Length)
            {
                return true;
            }

            if (arity.HasValue && fileNameIndex < fileNamePart.Length)
            {
                char fileNameChar = fileNamePart[fileNameIndex];

                if (fileNameChar == '`' || fileNameChar == '_')
                {
                    fileNameIndex += 1;
                }

                if (int.TryParse(fileNamePart.Substring(fileNameIndex), out int parsedArity)
                    && parsedArity == arity.Value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
