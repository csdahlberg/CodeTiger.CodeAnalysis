using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design
{
    public class ParameterDesignAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ExtensionMethodThatUsesTheThisParameterDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public static class StringExtensions
    {
        public static string WithAsterisk(this string original) { return original + ""*""; }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ExtensionMethodThatDoesNotUseTheThisParameterProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public static class StringExtensions
    {
        public static string WithAsterisk(this string original) { return ""*""; }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1020",
                    Message = "Extension methods should use the 'this' parameter.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 30) }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ParameterDesignAnalyzer();
        }
    }
}
