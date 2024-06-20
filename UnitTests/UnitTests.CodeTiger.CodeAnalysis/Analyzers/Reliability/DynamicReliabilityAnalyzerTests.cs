using CodeTiger.CodeAnalysis.Analyzers.Reliability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Reliability;

public class DynamicReliabilityAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void MethodWithDynamicReturnTypeProducesDiagnostic()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public dynamic GetValue() { return new ExpandoObject(); }
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2010",
                Message = "Dynamic should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 16)
                }
            });
    }

    [Fact]
    public void MethodWithDynamicParameterTypeProducesDiagnostic()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public void SetValue(dynamic value) { }
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2010",
                Message = "Dynamic should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 30)
                }
            });
    }

    [Fact]
    public void PropertyWithDynamicTypeProducesDiagnostic()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public dynamic Value => new ExpandoObject();
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2010",
                Message = "Dynamic should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 16)
                }
            });
    }

    [Fact]
    public void FieldWithDynamicTypeProducesDiagnostic()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    private dynamic _value;
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2010",
                Message = "Dynamic should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 17)
                }
            });
    }

    [Fact]
    public void MethodInheritedFromInterfaceWithDynamicReturnTypeDoesNotProduceDiagnostics()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public interface IThing
                {
            #pragma warning disable CT2010 // Dynamic should not be used
                    dynamic GetValue();
            #pragma warning restore CT2010 // Dynamic should not be used
                }

                public class Thing : IThing
                {
                    public dynamic GetValue() { return new ExpandoObject(); }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MethodInheritedFromBaseTypeWithDynamicReturnTypeDoesNotProduceDiagnostics()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public class Thing
                {
            #pragma warning disable CT2010 // Dynamic should not be used
                    public virtual dynamic GetValue() { return new ExpandoObject(); }
            #pragma warning restore CT2010 // Dynamic should not be used
                }

                public class Thing2 : Thing
                {
                    public override dynamic GetValue() { return new ExpandoObject(); }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MethodInheritedFromInterfaceWithDynamicParameterTypeDoesNotProduceDiagnostics()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public interface IThing
                {
            #pragma warning disable CT2010 // Dynamic should not be used
                    void SetValue(dynamic value);
            #pragma warning restore CT2010 // Dynamic should not be used
                }

                public class Thing : IThing
                {
                    public void SetValue(dynamic value) { }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MethodInheritedFromBaseTypeWithDynamicParameterTypeDoesNotProduceDiagnostics()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public class Thing
                {
            #pragma warning disable CT2010 // Dynamic should not be used
                    public virtual void SetValue(dynamic value) { }
            #pragma warning restore CT2010 // Dynamic should not be used
                }

                public class Thing2 : Thing
                {
                    public override void SetValue(dynamic value) { }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void PropertyInheritedFromInterfaceWithDynamicTypeDoesNotProduceDiagnostics()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public interface IThing
                {
            #pragma warning disable CT2010 // Dynamic should not be used
                    dynamic Value { get; }
            #pragma warning restore CT2010 // Dynamic should not be used
                }

                public class Thing : IThing
                {
                    public dynamic Value => new ExpandoObject();
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void PropertyInheritedFromBaseTypeWithDynamicTypeDoesNotProduceDiagnostics()
    {
        string code = """
            using System.Dynamic;
            namespace ClassLibrary1
            {
                public class Thing
                {
            #pragma warning disable CT2010 // Dynamic should not be used
                    public virtual dynamic Value { get; }
            #pragma warning restore CT2010 // Dynamic should not be used
                }

                public class Thing2 : Thing
                {
                    public override dynamic Value => new ExpandoObject();
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new DynamicReliabilityAnalyzer();
    }
}
