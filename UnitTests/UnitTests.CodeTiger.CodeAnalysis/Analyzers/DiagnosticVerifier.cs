﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers;

/// <summary>
/// Superclass of all Unit Tests for DiagnosticAnalyzers
/// </summary>
public abstract class DiagnosticVerifier
{
    private const string DefaultFilePathPrefix = "Test";
    private const string CSharpDefaultFileExt = "cs";
    private const string TestProjectName = "TestProject";

    private static readonly ImmutableArray<MetadataReference> _metadataReferences = CreateMetadataReferences();

    protected virtual DocumentationMode DocumentationMode => DocumentationMode.Parse;

    protected virtual OutputKind CompilationOutputKind => OutputKind.DynamicallyLinkedLibrary;

    protected virtual bool CompilationAllowsUnsafeCode => false;

    /// <summary>
    /// Get the CSharp analyzer being tested - to be implemented in non-abstract class
    /// </summary>
    protected abstract DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer();

    /// <summary>
    /// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
    /// Note: input a DiagnosticResult for each Diagnostic expected
    /// </summary>
    /// <param name="source">A class in the form of a string to run the analyzer on</param>
    /// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source
    /// </param>
    protected private void VerifyCSharpDiagnostic(string source, params DiagnosticResult[] expected)
    {
        VerifyDiagnostics(new[] { source }, GetCSharpDiagnosticAnalyzer(), expected);
    }

    /// <summary>
    /// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
    /// Note: input a DiagnosticResult for each Diagnostic expected
    /// </summary>
    /// <param name="source">A class in the form of a string to run the analyzer on</param>
    /// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source
    /// </param>
    protected private void VerifyCSharpDiagnostic(Tuple<string, string> source,
        params DiagnosticResult[] expected)
    {
        VerifyDiagnostics(new[] { source }, GetCSharpDiagnosticAnalyzer(), expected);
    }

    /// <summary>
    /// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source
    /// Note: input a DiagnosticResult for each Diagnostic expected
    /// </summary>
    /// <param name="sources">An array of strings to create source documents from to run the analyzers on
    /// </param>
    /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources
    /// </param>
    protected private void VerifyCSharpDiagnostic(string[] sources, params DiagnosticResult[] expected)
    {
        VerifyDiagnostics(sources, GetCSharpDiagnosticAnalyzer(), expected);
    }

    /// <summary>
    /// Create a Document from a string through creating a project that contains it.
    /// </summary>
    /// <param name="source">Classes in the form of a string</param>
    /// <returns>A Document created from the source string</returns>
    protected private Document CreateDocument(string source)
    {
        return CreateProject(CreateNamedSources(new[] { source })).Documents.First();
    }

    /// <summary>
    /// General method that gets a collection of actual diagnostics found in the source after the analyzer is
    /// run, then verifies each of them.
    /// </summary>
    /// <param name="sources">An array of strings to create source documents from to run the analyzers on
    /// </param>
    /// <param name="analyzer">The analyzer to be run on the source code</param>
    /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources
    /// </param>
    private void VerifyDiagnostics(string[] sources, DiagnosticAnalyzer analyzer,
        params DiagnosticResult[] expected)
    {
        var diagnostics = GetSortedDiagnostics(sources, analyzer);
        VerifyDiagnosticResults(diagnostics, analyzer, expected);
    }

    /// <summary>
    /// General method that gets a collection of actual diagnostics found in the source after the analyzer is
    /// run, then verifies each of them.
    /// </summary>
    /// <param name="sources">An array of strings to create source documents from to run the analyzers on
    /// </param>
    /// <param name="analyzer">The analyzer to be run on the source code</param>
    /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources
    /// </param>
    private void VerifyDiagnostics(Tuple<string, string>[] sources, DiagnosticAnalyzer analyzer,
        params DiagnosticResult[] expected)
    {
        var diagnostics = GetSortedDiagnostics(sources, analyzer);
        VerifyDiagnosticResults(diagnostics, analyzer, expected);
    }

    /// <summary>
    /// Given classes in the form of strings, their language, and an IDiagnosticAnlayzer to apply to it, return
    /// the diagnostics found in the string after converting it to a document.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <param name="analyzer">The analyzer to be run on the sources</param>
    /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
    private Diagnostic[] GetSortedDiagnostics(string[] sources, DiagnosticAnalyzer analyzer)
    {
        return GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources));
    }

    /// <summary>
    /// Given classes in the form of strings, their language, and an IDiagnosticAnlayzer to apply to it, return
    /// the diagnostics found in the string after converting it to a document.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <param name="analyzer">The analyzer to be run on the sources</param>
    /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
    private Diagnostic[] GetSortedDiagnostics(Tuple<string, string>[] sources,
        DiagnosticAnalyzer analyzer)
    {
        return GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources));
    }

    /// <summary>
    /// Given an array of strings as sources and a language, turn them into a project and return the documents
    /// and spans of it.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant
    /// </returns>
    private Document[] GetDocuments(string[] sources)
    {
        var namedSources = CreateNamedSources(sources);

        var project = CreateProject(namedSources);
        var documents = project.Documents.ToArray();

        if (sources.Length != documents.Length)
        {
            throw new SystemException("Amount of sources did not match amount of Documents created");
        }

        return documents;
    }

    /// <summary>
    /// Given an array of strings as sources and a language, turn them into a project and return the documents
    /// and spans of it.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant
    /// </returns>
    private Document[] GetDocuments(Tuple<string, string>[] sources)
    {
        var project = CreateProject(sources);
        var documents = project.Documents.ToArray();

        if (sources.Length != documents.Length)
        {
            throw new SystemException("Amount of sources did not match amount of Documents created");
        }

        return documents;
    }

    /// <summary>
    /// Create a project using the inputted strings as sources.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <returns>A Project created out of the Documents created from the source strings</returns>
    private Project CreateProject(Tuple<string, string>[] sources)
    {
        var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

        var parseOptions = new CSharpParseOptions(documentationMode: DocumentationMode,
            languageVersion: LanguageVersion.Latest);
        var compilationOptions = new CSharpCompilationOptions(CompilationOutputKind,
            allowUnsafe: CompilationAllowsUnsafeCode);
        var projectInfo = ProjectInfo.Create(projectId, VersionStamp.Default, TestProjectName, TestProjectName,
            LanguageNames.CSharp, parseOptions: parseOptions, compilationOptions: compilationOptions);
        var solution = new AdhocWorkspace()
            .CurrentSolution
            .AddProject(projectInfo)
            .AddMetadataReferences(projectId, _metadataReferences);
        foreach (var source in sources)
        {
            var documentId = DocumentId.CreateNewId(projectId, debugName: source.Item1);
            solution = solution.AddDocument(documentId, source.Item1, SourceText.From(source.Item2));
        }

        var diagnostics = solution.Projects.First().GetCompilationAsync().Result?.GetDiagnostics();
        var errors = diagnostics?.Where(x => x.Severity == DiagnosticSeverity.Error);
        if (errors?.Any() == true)
        {
            throw new Exception("Compilation failed due to errors:" + Environment.NewLine
                + string.Join(Environment.NewLine, errors));
        }

        return solution.GetProject(projectId)!;
    }

    /// <summary>
    /// Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics
    /// found in it. The returned diagnostics are then ordered by location in the source document.
    /// </summary>
    /// <param name="analyzer">The analyzer to run on the documents</param>
    /// <param name="documents">The Documents that the analyzer will be run on</param>
    /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
    protected static Diagnostic[] GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer analyzer,
        Document[] documents)
    {
        var projects = new HashSet<Project>();
        foreach (var document in documents)
        {
            projects.Add(document.Project);
        }

        var diagnostics = new List<Diagnostic>();
        foreach (var project in projects)
        {
            var compilation = project.GetCompilationAsync().Result;
            var compilationWithAnalyzers = compilation!.WithAnalyzers(ImmutableArray.Create(analyzer));
            var diags = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
            foreach (var diag in diags)
            {
                if (diag.Location == Location.None || diag.Location.IsInMetadata)
                {
                    diagnostics.Add(diag);
                }
                else
                {
                    for (int i = 0; i < documents.Length; i++)
                    {
                        var document = documents[i];
                        var tree = document.GetSyntaxTreeAsync().Result;
                        if (tree == diag.Location.SourceTree)
                        {
                            diagnostics.Add(diag);
                        }
                    }
                }
            }
        }

        var results = SortDiagnostics(diagnostics);
        diagnostics.Clear();
        return results;
    }

    /// <summary>
    /// Sort diagnostics by location in source document
    /// </summary>
    /// <param name="diagnostics">The list of Diagnostics to be sorted</param>
    /// <returns>An IEnumerable containing the Diagnostics in order of Location</returns>
    private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
    }

    /// <summary>
    /// Checks each of the actual Diagnostics found and compares them with the corresponding DiagnosticResult
    /// in the array of expected results.
    /// Diagnostics are considered equal only if the DiagnosticResultLocation, Id, Severity, and Message of the
    /// DiagnosticResult match the actual diagnostic.
    /// </summary>
    /// <param name="actualResults">The Diagnostics found by the compiler after running the analyzer on the
    /// source code</param>
    /// <param name="analyzer">The analyzer that was being run on the sources</param>
    /// <param name="expectedResults">Diagnostic Results that should have appeared in the code</param>
    private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults,
        DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
    {
        int expectedCount = expectedResults.Length;
        int actualCount = actualResults.Count();

        if (expectedCount != actualCount)
        {
            string diagnosticsOutput = actualResults.Any()
                ? FormatDiagnostics(analyzer, actualResults.ToArray())
                : "NONE.";

            Assert.Fail(
                $"""
                Mismatch between number of diagnostics returned, expected {expectedCount}, actual {actualCount}

                Diagnostics:
                    {diagnosticsOutput}
                """);
        }

        for (int i = 0; i < expectedResults.Length; i++)
        {
            var actual = actualResults.ElementAt(i);
            var expected = expectedResults[i];

            if (expected.Line == -1 && expected.Column == -1)
            {
                if (actual.Location != Location.None)
                {
                    Assert.Fail(
                        $"""
                        Expected:
                            A project diagnostic with No location
                        Actual:
                            {FormatDiagnostics(analyzer, actual)}
                        """);
                }
            }
            else
            {
                VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Locations.First());
                var additionalLocations = actual.AdditionalLocations.ToArray();

                if (additionalLocations.Length != expected.Locations.Length - 1)
                {
                    expectedCount = expected.Locations.Length - 1;
                    actualCount = additionalLocations.Length;

                    Assert.Fail(
                        $"""
                        Expected {expectedCount} additional locations but got {actualCount} for Diagnostic:
                            {FormatDiagnostics(analyzer, actual)}
                        """);
                }

                for (int j = 0; j < additionalLocations.Length; ++j)
                {
                    VerifyDiagnosticLocation(analyzer, actual, additionalLocations[j],
                        expected.Locations[j + 1]);
                }
            }

            if (actual.Id != expected.Id)
            {
                Assert.Fail(
                    $"""
                    Expected diagnostic id to be "{expected.Id}" was "{actual.Id}"

                    Diagnostic:
                        {FormatDiagnostics(analyzer, actual)}
                    """);
            }

            if (actual.Severity != expected.Severity)
            {
                Assert.Fail(
                    $"""
                    Expected diagnostic severity to be "{expected.Severity}" was "{actual.Severity}"

                    Diagnostic:
                        {FormatDiagnostics(analyzer, actual)}
                    """);
            }

            if (actual.GetMessage() != expected.Message)
            {
                Assert.Fail(
                    $"""
                    Expected diagnostic message to be "{expected.Message}" was "{actual.GetMessage()}"

                    Diagnostic:
                        {FormatDiagnostics(analyzer, actual)}
                    """);
            }
        }
    }

    /// <summary>
    /// Helper method to VerifyDiagnosticResult that checks the location of a diagnostic and compares it with
    /// the location in the expected DiagnosticResult.
    /// </summary>
    /// <param name="analyzer">The analyzer that was being run on the sources</param>
    /// <param name="diagnostic">The diagnostic that was found in the code</param>
    /// <param name="actual">The Location of the Diagnostic found in the code</param>
    /// <param name="expected">The DiagnosticResultLocation that should have been found</param>
    private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic,
        Location actual, DiagnosticResultLocation expected)
    {
        var actualSpan = actual.GetLineSpan();

        bool isSpanCorrect = actualSpan.Path == expected.Path
            || (actualSpan.Path?.Contains("Test0.") == true && expected.Path?.Contains("Test.") == true);

        Assert.True(isSpanCorrect,
            $"""
            Expected diagnostic to be in file "{expected.Path}" was actually in file "{actualSpan.Path}"

            Diagnostic:
                {FormatDiagnostics(analyzer, diagnostic)}
            """);

        var actualLinePosition = actualSpan.StartLinePosition;

        // Only check line position if there is an actual line in the real diagnostic
        if (actualLinePosition.Line > 0)
        {
            int expectedLine = expected.Line;
            int actualLine = actualLinePosition.Line + 1;

            if (actualLine != expectedLine)
            {
                Assert.Fail(
                    $"""
                    Expected diagnostic to be on line {expectedLine} was actually on line {actualLine}

                    Diagnostic:
                        {FormatDiagnostics(analyzer, diagnostic)}
                    """);
            }
        }

        // Only check column position if there is an actual column position in the real diagnostic
        if (actualLinePosition.Character > 0)
        {
            int expectedColumn = expected.Column;
            int actualColumn = actualLinePosition.Character + 1;

            if (actualColumn != expectedColumn)
            {
                Assert.Fail(
                    $"""
                    Expected diagnostic to start at column {expectedColumn} was actually at column {actualColumn}

                    Diagnostic:
                        {FormatDiagnostics(analyzer, diagnostic)}
                    """);
            }
        }
    }

    /// <summary>
    /// Helper method to format a Diagnostic into an easily readable string
    /// </summary>
    /// <param name="analyzer">The analyzer that this verifier tests</param>
    /// <param name="diagnostics">The Diagnostics to be formatted</param>
    /// <returns>The Diagnostics formatted as a string</returns>
    private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < diagnostics.Length; ++i)
        {
            builder.AppendLine("// " + diagnostics[i].ToString());

            var analyzerType = analyzer.GetType();
            var rules = analyzer.SupportedDiagnostics;

            foreach (var rule in rules)
            {
                if (rule != null && rule.Id == diagnostics[i].Id)
                {
                    var location = diagnostics[i].Location;
                    if (location == Location.None)
                    {
                        builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
                    }
                    else
                    {
                        Assert.True(location.IsInSource,
                            "Test base does not currently handle diagnostics in metadata locations. Diagnostic: "
                            + diagnostics[i]);

                        var sourceTree = location.SourceTree;
                        string resultMethodName = sourceTree is null || sourceTree.FilePath.EndsWith(".cs") == true
                            ? "GetCSharpResultAt"
                            : "GetBasicResultAt";
                        var linePosition = location.GetLineSpan().StartLinePosition;

                        builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
                            resultMethodName,
                            linePosition.Line + 1,
                            linePosition.Character + 1,
                            analyzerType.Name,
                            rule.Id);
                    }

                    if (i != diagnostics.Length - 1)
                    {
                        builder.Append(',');
                    }

                    builder.AppendLine();
                    break;
                }
            }
        }

        return builder.ToString();
    }

    private static Tuple<string, string>[] CreateNamedSources(string[] sources)
    {
        string fileNamePrefix = DefaultFilePathPrefix;
        string fileExt = CSharpDefaultFileExt;

        var namedSources = sources
            .Select((source, index) => Tuple.Create($"{fileNamePrefix}{index}.{fileExt}", source))
            .ToArray();
        return namedSources;
    }

    private static ImmutableArray<MetadataReference> CreateMetadataReferences()
    {
        var assemblyLocations = new List<string>
        {
            typeof(object).Assembly.Location,
            typeof(Console).Assembly.Location,
            typeof(Enumerable).Assembly.Location,
            typeof(IEnumerable<>).Assembly.Location,
            typeof(ConcurrentDictionary<,>).Assembly.Location,
            typeof(CSharpCompilation).Assembly.Location,
            typeof(Compilation).Assembly.Location,
            typeof(ExpandoObject).Assembly.Location,
        };

        foreach (var backupReference in Assembly.GetEntryAssembly()!.GetReferencedAssemblies())
        {
            assemblyLocations.Add(Assembly.Load(backupReference).Location);
        }

        return assemblyLocations
            .Distinct()
            .Select(x => MetadataReference.CreateFromFile(x))
            .AsEnumerable<MetadataReference>()
            .ToImmutableArray();
    }
}
