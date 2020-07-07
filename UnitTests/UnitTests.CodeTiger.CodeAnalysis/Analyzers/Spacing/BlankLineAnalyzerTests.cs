using CodeTiger.CodeAnalysis.Analyzers.Spacing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Spacing
{

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

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BlankLineAnalyzer();
        }
    }
}
