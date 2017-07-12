using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design
{
    public class UnmanagedDesignAnalyzerTests : DiagnosticVerifier
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
        public void ClassWithUnmanagedMemberAndFinalizerDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private IntPtr _unmanagedState;

        ~Class1()
        {
            _unmanagedState.ToString();
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ClassWithoutUnmanagedMemberAndFinalizerProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private object _thing;

        ~Class1()
        {
            _thing.ToString();
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1007",
                    Message = "Types without unmanaged state should not have a finalizer.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 10)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UnmanagedDesignAnalyzer();
        }
    }
}
