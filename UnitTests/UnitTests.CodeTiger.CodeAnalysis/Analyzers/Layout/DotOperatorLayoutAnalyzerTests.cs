using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout
{
    public class DotOperatorLayoutAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void SingleLineExpressionsDoNotProduceDiagnostics()
        {
            string code = @"using System;
using System.Linq;
namespace ClassLibrary1.Namespace1
{
    public class Class1
    {
        public void DoSomething()
        {
            var values = new Values { X = 1, Y = 2 };
            values.X.ToString();

            unsafe
            {
                var valuesPointer = &values;
                valuesPointer->Y.ToString();
            }
        }

        struct Values { public int X; public int Y; }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void MultiLineExpressionsSplitBeforeDotOrMemberAccessTokensDoNotProduceDiagnostics()
        {
            string code = @"using System;
using System
    .Linq;
namespace ClassLibrary1
    .Namespace1
{
    public class Class1
    {
        public void DoSomething()
        {
            var values = new Values { X = 1, Y = 2 };
            values
                .X
                .ToString();

            unsafe
            {
                var valuesPointer = &values;
                valuesPointer
                    ->Y
                    .ToString();
            }
        }

        struct Values { public int X; public int Y; }
    }
}";

            VerifyCSharpDiagnostic(code);
        }


        [Fact]
        public void MultiLineExpressionsSplitAfterDotOrMemberAccessTokensProduceDiagnostics()
        {
            string code = @"using System;
using System.
    Linq;
namespace ClassLibrary1.
    Namespace1
{
    public class Class1
    {
        public void DoSomething()
        {
            var values = new Values { X = 1, Y = 2 };
            values.
                X.
                ToString();

            unsafe
            {
                var valuesPointer = &values;
                valuesPointer->
                    Y.
                    ToString();
            }
        }

        struct Values { public int X; public int Y; }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3530",
                    Message = "Multi-line expressions should not be split after a dot or member access token.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 2, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3530",
                    Message = "Multi-line expressions should not be split after a dot or member access token.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 24)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3530",
                    Message = "Multi-line expressions should not be split after a dot or member access token.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 12, 19)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3530",
                    Message = "Multi-line expressions should not be split after a dot or member access token.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 13, 18)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3530",
                    Message = "Multi-line expressions should not be split after a dot or member access token.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 19, 30)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3530",
                    Message = "Multi-line expressions should not be split after a dot or member access token.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 20, 22)
                    }
                }
            );
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DotOperatorLayoutAnalyzer();
        }
    }
}
