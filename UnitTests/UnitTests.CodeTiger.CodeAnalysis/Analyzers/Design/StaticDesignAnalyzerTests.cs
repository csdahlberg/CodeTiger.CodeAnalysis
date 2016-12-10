using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design
{
    public class StaticDesignAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ClassWithNoMembersDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ClassImplmementingInterfaceAndWithStaticMethodDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1 : IDisposable
    {
        public static void DoSomething() { }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ClassWithBaseClassAndWithStaticMethodDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1 : Task
    {
        public static void DoSomething() { }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ClassWithStaticMethodProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static void DoSomething() { }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1000",
                    Message = "Classes with all static members should be static.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 18)
                    }
                });
        }

        [Fact]
        public void ClassWithStaticPropertyProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static bool IsTrue => true;
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1000",
                    Message = "Classes with all static members should be static.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 18)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new StaticDesignAnalyzer();
        }
    }
}
