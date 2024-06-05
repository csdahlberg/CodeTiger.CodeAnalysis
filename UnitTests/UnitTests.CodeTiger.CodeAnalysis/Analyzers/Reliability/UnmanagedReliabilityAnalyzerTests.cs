using CodeTiger.CodeAnalysis.Analyzers.Reliability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Reliability;

public class UnmanagedReliabilityAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void ClassWithDisposableStateAndTheFullDisposePatternDoesNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Thing : IDisposable
    {
        private IDisposable _disposable;
        ~Thing() { Dispose(false); }
        public void Dispose() { Dispose(true); }
        protected virtual void Dispose(bool isDisposing) { }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ClassWithDisposableStateWithoutADestructorDoesNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Thing : IDisposable
    {
        private IDisposable _disposable;
        public void Dispose() { Dispose(true); }
        protected virtual void Dispose(bool isDisposing) { }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ClassWithDisposableStateThatDoesNotImplementIDisposableProducesDiagnostic()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Thing
    {
        private IDisposable _disposable;
        ~Thing() { }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2001",
                Message = "Types with disposable state should implement IDisposable",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            });
    }

    [Fact]
    public void StructWithDisposableStateDoesNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public struct Thing
    {
        private IDisposable _disposable;
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ClassWithUnmanagedStateAndTheFullDisposePatternDoesNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Thing : IDisposable
    {
        private IntPtr _unmanagedHandle;
        ~Thing() { Dispose(false); }
        public void Dispose() { Dispose(true); }
        protected virtual void Dispose(bool isDisposing) { }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ClassWithUnmanagedStateWithoutADestructorProducesDiagnostic()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Thing : IDisposable
    {
        private IntPtr _unmanagedHandle;
        public void Dispose() { Dispose(true); }
        protected virtual void Dispose(bool isDisposing) { }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2002",
                Message = "Types with unmanaged state should implement the full dispose pattern",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            });
    }

    [Fact]
    public void ClassWithUnmanagedStateThatDoesNotImplementIDisposableProducesDiagnostic()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Thing
    {
        private IntPtr _unmanagedHandle;
        ~Thing() { }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2002",
                Message = "Types with unmanaged state should implement the full dispose pattern",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            });
    }

    [Fact]
    public void StructWithUnmanagedStateDoesNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public struct Thing
    {
        private IntPtr _unmanagedHandle;
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new UnmanagedReliabilityAnalyzer();
    }
}
