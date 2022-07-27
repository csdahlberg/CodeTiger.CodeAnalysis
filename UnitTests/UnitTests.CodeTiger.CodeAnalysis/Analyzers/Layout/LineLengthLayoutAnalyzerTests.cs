using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout;

public class LineLengthLayoutAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void LinesShorterOrEqualToTheMaximumLengthDoNotProduceDiagnostics()
    {
        string code =
#pragma warning disable CT3531 // Lines should not exceed the maximum length of 115.
@"using System;
namespace ClassLibrary1.Namespace0000000000000000000000000000000000000000000000000000000000000000000000000000000000
{
    public class Class111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
    {
        public void DoSomething()
        {
            Console.WriteLine(""                                                                                 "");
        }
    }
}";
#pragma warning restore CT3531 // Lines should not exceed the maximum length of 115.

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LinesLongerThanTheMaximumLengthProduceDiagnostics()
    {
        string code =
#pragma warning disable CT3531 // Lines should not exceed the maximum length of 115.
@"using System;
namespace ClassLibrary1.Namespace00000000000000000000000000000000000000000000000000000000000000000000000000000000000
{
    public class Class1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
    {
        public void DoSomething22222222222222222222222222222222222222222222222222222222222222222222222222222222222()
        {
            Console.WriteLine(""                                                                                  "");
        }
    }
}";
#pragma warning restore CT3531 // Lines should not exceed the maximum length of 115.
        
        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3531",
                Message = "Lines should not exceed the maximum length of 115.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 2, 116)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3531",
                Message = "Lines should not exceed the maximum length of 115.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 116)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3531",
                Message = "Lines should not exceed the maximum length of 115.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 116)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3531",
                Message = "Lines should not exceed the maximum length of 115.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 116)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new LineLengthLayoutAnalyzer();
    }
}
