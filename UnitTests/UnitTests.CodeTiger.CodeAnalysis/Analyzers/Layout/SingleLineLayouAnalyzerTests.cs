using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Xunit;
using Microsoft.CodeAnalysis;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout
{
    public class SingleLineLayouAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void NamespaceDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1 { }";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3500",
                    Message = "Namespaces should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 2, 11)
                    }
                }
            );
        }

        [Fact]
        public void ClassDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1 { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3502",
                    Message = "Types should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 18)
                    }
                }
            );
        }

        [Fact]
        public void StructDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public struct Struct1 { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3502",
                    Message = "Types should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 19)
                    }
                }
            );
        }

        [Fact]
        public void InterfaceDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IInterface1 { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3502",
                    Message = "Types should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 22)
                    }
                }
            );
        }

        [Fact]
        public void EnumDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public enum Enum1 { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3502",
                    Message = "Types should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 17)
                    }
                }
            );
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SingleLineLayoutAnalyzer();
        }
    }
}
