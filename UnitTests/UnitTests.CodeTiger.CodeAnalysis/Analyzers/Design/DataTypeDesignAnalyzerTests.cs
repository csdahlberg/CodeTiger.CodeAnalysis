using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design;

public class DataTypeDesignAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void NonPublicTuplesInSignaturesDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Thing
    {
        private Tuple<int, string> Tuple { get; set; }
        internal void SetTuple((int, string) tuple) { Tuple = new Tuple<int, string>(tuple.Item1, tuple.Item2); }
        internal (int Id, string Name) GetTuple3() { return (Tuple.Item1, Tuple.Item2); }
    }
    internal class Thing2
    {
        public Tuple<int, string> Tuple { get; set; }
        public void SetTuple((int, string) tuple) { Tuple = new Tuple<int, string>(tuple.Item1, tuple.Item2); }
        public (int Id, string Name) GetTuple3() { return (Tuple.Item1, Tuple.Item2); }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ExternallyAccessibleTuplesInSignaturesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Thing
    {
        protected Tuple<int, string> Tuple { get; set; }
        public void SetTuple((int, string) tuple) { Tuple = new Tuple<int, string>(tuple.Item1, tuple.Item2); }
        public (int Id, string Name) GetTuple3() { return (Tuple.Item1, Tuple.Item2); }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1014",
                Message = "Externally-accessible members should not use tuples.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 19) }
            },
            new DiagnosticResult
            {
                Id = "CT1014",
                Message = "Externally-accessible members should not use tuples.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 30) }
            },
            new DiagnosticResult
            {
                Id = "CT1014",
                Message = "Externally-accessible members should not use tuples.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 16) }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new DataTypeDesignAnalyzer();
    }
}
