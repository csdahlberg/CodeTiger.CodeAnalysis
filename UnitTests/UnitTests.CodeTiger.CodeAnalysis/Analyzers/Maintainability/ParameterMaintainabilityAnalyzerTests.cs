using CodeTiger.CodeAnalysis.Analyzers.Maintainability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Maintainability
{
    public class ParameterMaintainabilityAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ParametersWithDefaultValuesThatMatchDefaultValueForTypeDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    enum Color { None, Red }
    public class Thing
    {
        public void DoSomething(Color color = Color.None, Color color2 = default(Color), Color? color3 = null) { }
        public void DoSomething(object value = null, ulong value2 = 0) { }
        public void DoSomething(Thing value = null, Thing value2 = default(Thing)) { }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ExternallyAccessibleParametersWithDefaultValuesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    enum Color { None, Red }
    public class Thing
    {
        public void DoSomething(Color color = Color.Red, Color color2 = (Color)3) { }
        public void DoSomething(byte value = 1, ulong value2 = ulong.MaxValue, bool flag = true) { }
        public void DoSomething(
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1501",
                    Message = "Externally-accessible parameters should not have default values.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 33) }
                },
                new DiagnosticResult
                {
                    Id = "CT1501",
                    Message = "Externally-accessible parameters should not have default values.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 58) }
                },
                new DiagnosticResult
                {
                    Id = "CT1501",
                    Message = "Externally-accessible parameters should not have default values.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 33) }
                },
                new DiagnosticResult
                {
                    Id = "CT1501",
                    Message = "Externally-accessible parameters should not have default values.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 49) }
                },
                new DiagnosticResult
                {
                    Id = "CT1501",
                    Message = "Externally-accessible parameters should not have default values.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 80) }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ParameterMaintainabilityAnalyzer();
        }
    }
}
