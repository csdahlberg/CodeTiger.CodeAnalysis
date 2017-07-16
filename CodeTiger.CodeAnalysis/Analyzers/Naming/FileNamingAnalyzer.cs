using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
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

        /// <summary>
        /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(SourceFileNamesShouldUsePascalCasingDescriptor);
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
            if (!string.IsNullOrWhiteSpace(context.Tree.FilePath))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(context.Tree.FilePath)
                    .Split('.')
                    .First();
                if (NamingUtility.IsProbablyPascalCased(fileNameWithoutExtension) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(SourceFileNamesShouldUsePascalCasingDescriptor,
                        Location.Create(context.Tree, TextSpan.FromBounds(0, 0))));
                }
            }
        }
    }
}
