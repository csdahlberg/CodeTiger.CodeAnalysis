using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout
{
    public class AttributeLayoutAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void AttributesDeclaredSeparatelyDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    [Obsolete]
    [CLSCompliant(false)]
    public class Class1
    {
        [Obsolete]
        [CLSCompliant(false)]
        public void DoSomething()
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void AttributesDeclaredInSameAttributeListProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    [Obsolete, CLSCompliant(false)]
    public class Class1
    {
        [Obsolete, CLSCompliant(false)]
        public void DoSomething()
        {
        }
    }
}
";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3526",
                    Message = "Attributes should be declared separately.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 6),
                        new DiagnosticResultLocation("Test0.cs", 4, 16)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3526",
                    Message = "Attributes should be declared separately.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 10),
                        new DiagnosticResultLocation("Test0.cs", 7, 20)
                    }
                });
        }

        [Fact]
        public void AttributesDeclaredOnSameLineProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    [Obsolete][CLSCompliant(false)]
    public class Class1
    {
        [Obsolete][CLSCompliant(false)]
        public void DoSomething()
        {
        }
    }
}
";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3527",
                    Message = "Attributes should be declared on separate lines.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 15)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3527",
                    Message = "Attributes should be declared on separate lines.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 19)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AttributeLayoutAnalyzer();
        }
    }
}
