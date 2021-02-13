using CodeTiger.CodeAnalysis.Analyzers.Usage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Usage
{
    public class StringUsageAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void EmptyStringLiteralDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod()
        {
            string value = """";
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void StringLiteralWithUnbracedLocalVariableDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod()
        {
            int x = 13;
            string value = ""x"";
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void StringLiteralWithBracedLocalVariableProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod()
        {
            int x = 13;
            string value = ""{x}"";
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT2208",
                    Message = "Interpolated strings require a leading '$' character.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 28)
                    }
                });
        }

        [Fact]
        public void StringLiteralWithUnbracedFieldDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        int x = 13;
        public void TestMethod()
        {
            string value = ""x"";
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void StringLiteralWithBracedFieldProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod()
        {
            int x = 13;
            string value = ""{x}"";
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT2208",
                    Message = "Interpolated strings require a leading '$' character.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 28)
                    }
                });
        }

        [Fact]
        public void StringLiteralWithUnbracedParameterDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod(int x)
        {
            string value = ""x"";
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void StringLiteralWithBracedParameterProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod(int x)
        {
            string value = ""{x}"";
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT2208",
                    Message = "Interpolated strings require a leading '$' character.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 28)
                    }
                });
        }

        [Fact]
        public void StringLiteralWithUnbracedNameofExpressionDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod()
        {
            string value = ""nameof(TestMethod)"";
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void StringLiteralWithBracedNameofExpressionProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public void TestMethod()
        {
            string value = ""{nameof(TestMethod)}"";
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT2208",
                    Message = "Interpolated strings require a leading '$' character.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 28)
                    }
                });
        }

        [Fact]
        public void StringLiteralWithUnbracedMemberAccessExpressionDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public static int X;
        public void TestMethod()
        {
            string value = ""TestClass.X"";
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void StringLiteralWithBracedMemberAccessExpressionProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestClass
    {
        public static int X;
        public void TestMethod()
        {
            string value = ""{TestClass.X}"";
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT2208",
                    Message = "Interpolated strings require a leading '$' character.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 28)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new StringUsageAnalyzer();
        }
    }
}
