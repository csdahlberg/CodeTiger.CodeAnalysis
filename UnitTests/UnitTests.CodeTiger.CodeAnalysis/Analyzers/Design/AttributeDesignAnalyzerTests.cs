using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design;

public class AttributeDesignAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void AttributeWithAttributeUsageAttributeDoesNotProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                [AttributeUsage(AttributeTargets.Assembly)]
                public class FooAttribute : Attribute
                {
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AttributeWithoutAttributeUsageAttributeProducesDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class FooAttribute : Attribute
                {
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1012",
                Message = "Attribute classes should include an AttributeUsage attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 4, 18) }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new AttributeDesignAnalyzer();
    }
}
