using CodeTiger.CodeAnalysis.Analyzers.Spacing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Spacing;

public class BlankLineAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void FileThatDoesNotStartWithBlankLineDoesNotProduceDiagnostic()
    {
        string code = @"namespace ClassLibrary1
{
    public class TestClass
    {
    }
}
";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void FileThatStartsWithBlankLineProducesDiagnostic()
    {
        string code = @"
namespace ClassLibrary1
{
    public class TestClass
    {
    }
}
";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3000",
                Message = "Files should not begin with blank lines.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 0, 0)
                }
            });
    }

    [Fact]
    public void FileThatEndsWithBlankLineDoesNotProduceDiagnostic()
    {
        string code = @"namespace ClassLibrary1
{
    public class TestClass
    {
    }
}
";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void FileThatDoesNotEndWithBlankLineProducesDiagnostic()
    {
        string code = @"namespace ClassLibrary1
{
    public class TestClass
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3001",
                Message = "Files should end with a blank line.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 0)
                }
            });
    }

    [Fact]
    public void CodeThatDoesNotHaveConsecutiveBlankLinesDoesNotProduceDiagnostic()
    {
        string code = @"namespace ClassLibrary1
{
    public class TestClass
    {
        public void DoSomething()
        {
        }
    }
}
";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void CodeThatHasConsecutiveBlankLinesInStringLiteralsDoesNotProduceDiagnostic()
    {
        string code = @"namespace ClassLibrary1
{
    public class TestClass
    {
        public const string StringField = @""


"";

        public void DoSomething()
        {
            string stringVariable = @""


"";
        }
    }
}
";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void CodeThatHasConsecutiveBlankLinesProducesDiagnostic()
    {
        string code = @"namespace ClassLibrary1
{
    public class TestClass
    {
        public string Name { get; set; }


        public void DoSomething()
        {
            int i = 1;


            return;
        }
    }
}
";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3002",
                Message = "Code should not contain consecutive blank lines.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 0)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3002",
                Message = "Code should not contain consecutive blank lines.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 12, 0)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new BlankLineAnalyzer();
    }
}
