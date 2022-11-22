using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design;

public class ConstructorDesignAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void NonPublicConstructorsDoNotProduceDiagnostic()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public abstract class Thing
    {
        internal Thing() { }
        protected Thing(bool flag) : this(flag.ToString()) { }
        private Thing(string value) { }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void PublicConstructorProducesDiagnostic()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public abstract class Thing
    {
        public Thing() { }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1013",
                Message = "Constructors for abstract classes should not be public",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 16) }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new ConstructorDesignAnalyzer();
    }
}
