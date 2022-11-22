using CodeTiger.CodeAnalysis.Analyzers.Maintainability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Maintainability;

public class EnumMaintainabilityAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void EnumValuesForNonFlagsEnumsDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public enum Color
    {
        None,
        Red = 4,
        Yellow = 8,
        Blue = 5,
        Orange = Red | Yellow | 2,
        Green = 24,
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void EnumValuesForFlagsEnumsThatAreSingleBitsOrCombinationsOfOtherValuesDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    [Flags]
    public enum Color
    {
        None,
        Red = 4,
        Yellow = 8,
        Blue = 16,
        Orange = Red | Yellow,
        Green = 24,
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void EnumValuesForFlagsEnumsThatAreNotSingleBitsOrCombinationsOfOtherValuesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    [Flags]
    public enum Color
    {
        None,
        Red = 4,
        Yellow = 8,
        Blue = 5,
        Orange = Red | Yellow | 2,
        Green = 24,
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1505",
                Message = "Composite values in Flags enumerations should equal a combination of other values",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9) }
            },
            new DiagnosticResult
            {
                Id = "CT1505",
                Message = "Composite values in Flags enumerations should equal a combination of other values",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 9) }
            },
            new DiagnosticResult
            {
                Id = "CT1505",
                Message = "Composite values in Flags enumerations should equal a combination of other values",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 9) }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new EnumMaintainabilityAnalyzer();
    }
}
