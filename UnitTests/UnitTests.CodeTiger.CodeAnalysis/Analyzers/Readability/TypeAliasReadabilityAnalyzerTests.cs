using System;
using CodeTiger.CodeAnalysis.Analyzers.Readability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Readability
{
    public class TypeAliasReadabilityAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void BuiltInTypeAliasesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void DoSomething(int firstValue, int secondValue)
        {
        }
    }
}";

            VerifyCSharpDiagnostic(Tuple.Create("TestFile.cs", code));
        }

        [Fact]
        public void DotNetTypeNamesInDocumentationCommentsDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    /// <summary>Does something with <see cref=""Int32""/> or <see cref=""Nullable{Int32}""/> values.</summary>
    public class TestClass
    {
        public void DoSomething(int firstValue, int secondValue)
        {
        }
    }
}";

            VerifyCSharpDiagnostic(Tuple.Create("TestFile.cs", code));
        }

        [Fact]
        public void ComparisonsWithLiteralOnLeftProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void DoSomething(Int32 firstValue, Int32 secondValue)
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3101",
                    Message = "Built-in type aliases should be used.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 33)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3101",
                    Message = "Built-in type aliases should be used.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 51)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TypeAliasReadabilityAnalyzer();
        }
    }
}
