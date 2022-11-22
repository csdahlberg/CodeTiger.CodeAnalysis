using CodeTiger.CodeAnalysis.Analyzers.Documentation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Documentation;

public class RequiredDocumentationAnalyzerTests : DiagnosticVerifier
{
    protected override DocumentationMode DocumentationMode => DocumentationMode.Diagnose;

    [Fact]
    public void DocumentationCommentsForMethodsWithVoidReturnTypeDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        /// <summary>Does something.</summary>
        public void DoSomething() { }

        /// <summary>Does something.</summary>
        public async void DoSomethingAsync() { }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void DocumentationCommentsForAsyncMethodWithTaskReturnTypeDoNotProduceDiagnostics()
    {
        string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class Class1
    {
        /// <summary>Does something.</summary>
        public async Task DoSomethingAsync() { }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void DocumentationCommentsWithReturnsElementDoNotProduceDiagnostics()
    {
        string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class Class1
    {
        /// <summary>Does something.</summary>
        /// <returns>A flag.</returns>
        public bool DoSomething() { return true; }

        /// <summary>Does something.</summary>
        /// <returns>A flag.</returns>
        public async Task<bool> DoSomethingAsync() { return true; }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void DocumentationCommentsWithoutReturnsElementProduceDiagnostics()
    {
        string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class Class1
    {
        /// <summary>Does something.</summary>
        public bool DoSomething() { return true; }

        /// <summary>Does something.</summary>
        public async Task<bool> DoSomethingAsync() { return true; }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3601",
                Message = "Return values of externally accessible members should be documented",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3601",
                Message = "Return values of externally accessible members should be documented",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 33)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new RequiredDocumentationAnalyzer();
    }
}
