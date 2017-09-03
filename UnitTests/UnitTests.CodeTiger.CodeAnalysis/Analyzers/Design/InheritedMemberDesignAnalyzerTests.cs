using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design
{
    public class InheritedMemberDesignAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ClassWithBaseClassAndWithStaticMethodDoesNotProduceDiagnostics()
        {
            string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class Class1
    {
        public object GetValue() { return new object(); }
        public Class1 Self { get { return this; } }
    }
    public class Class2 : Class1
    {
        public new int GetValue() { return 1; }
        public new Class2 Self { get { return this; } }
    }
    public class Class3<T> : Class1 where T : IDisposable
    {
        public new T GetValue() { return default(T); }
        public new Class3<T> Self { get { return this; } }
    }
    public class Class4 : Class3<IDisposable>
    {
        public new Task GetValue() { return default(Task); }
        public new Class4 Self { get { return this; } }
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
        public int GetValue() { return 1; }
        public Class1 Self { get { return this; } }
    }
    public class Class2 : Class1
    {
        public new int GetValue() { return 2; }
        public new string Self { get { return ""blah""; } }
    }
    public class Class3<T> : Class1 where T : IDisposable
    {
        public new int GetValue() { return 3; }
        public new string Self { get { return ""blah""; } }
    }
    public class Class4 : Class3<IDisposable>
    {
        public new IDisposable GetValue() { return default(IDisposable); }
        public new Class4 Self { get { return this; } }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 24)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 12, 27)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 16, 24)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 17, 27)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 21, 32)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 22, 27)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new InheritedMemberDesignAnalyzer();
        }
    }
}
