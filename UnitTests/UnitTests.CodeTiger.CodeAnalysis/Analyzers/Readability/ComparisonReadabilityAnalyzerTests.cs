using CodeTiger.CodeAnalysis.Analyzers.Readability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Readability
{
    public class ComparisonReadabilityAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void CharacterComparisonsWithLiteralOnRightDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void DoSomething(char charValue, bool boolValue, string stringValue, int intValue)
        {
            if (charValue <= 'C'
                || boolValue == true
                || boolValue != false
                || stringValue == null
                || stringValue != """"
                || intValue > 1)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(Tuple.Create("TestFile.cs", code));
        }

        [Fact]
        public void CharacterComparisonsWithLiteralOnLeftProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void DoSomething(char charValue, bool boolValue, string stringValue, int intValue)
        {
            if ('C' >= charValue
                || true == boolValue
                || false != boolValue
                || null == stringValue
                || """" != stringValue
                || 1 < intValue)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3113",
                    Message = "Literals should not be on the left side of comparison operators.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 17)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3113",
                    Message = "Literals should not be on the left side of comparison operators.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 20)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3113",
                    Message = "Literals should not be on the left side of comparison operators.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 10, 20)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3113",
                    Message = "Literals should not be on the left side of comparison operators.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 20)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3113",
                    Message = "Literals should not be on the left side of comparison operators.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 12, 20)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3113",
                    Message = "Literals should not be on the left side of comparison operators.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 13, 20)
                    }
                });
        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ComparisonReadabilityAnalyzer();
        }
    }
}
