using CodeTiger.CodeAnalysis.Analyzers.Usage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Usage;

public class EnvironmentUsageAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void CallingMethodOtherThanExitDoesNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod()
        {
            Environment.ExpandEnvironmentVariables(""%userprofile%"");
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void CallingExitProducesDiagnostic()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod()
        {
            Environment.Exit(0);
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2203",
                Message = "Environment.Exit should not be used.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 13)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new EnvironmentUsageAnalyzer();
    }
}
