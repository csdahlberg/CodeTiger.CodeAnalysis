using CodeTiger.CodeAnalysis.Analyzers.Naming;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Naming
{
    public class SymbolNamingAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ConstFieldsWithPascalCasedNamesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public const int One = 1;
        protected const int Two = 2;
        private const int Three = 3;
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ConstFieldsWithNonPascalCasedNamesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public const int one = 1;
        protected const int _two = 2;
        private const int THREE = 3;
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1703",
                    Message = "Constant field names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 26)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1703",
                    Message = "Constant field names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 29)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1703",
                    Message = "Constant field names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 27)
                    }
                });
        }

        [Fact]
        public void PrivateFieldsWithUnderscorePrefixedCamelCasedNamesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private int _one = 1;
        private readonly int _two = 2;
        private static int _three = 3;
        private static readonly int _four = 4;
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void PrivateFieldsWithNonUnderscorePrefixedCamelCasedNamesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private int one = 1;
        private readonly int Two = 2;
        private static int THREE = 3;
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1704",
                    Message = "Private field names should use camel casing with a leading underscore.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 21)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1704",
                    Message = "Private field names should use camel casing with a leading underscore.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 30)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1704",
                    Message = "Private field names should use camel casing with a leading underscore.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 28)
                    }
                });
        }

        [Fact]
        public void EventFieldsWithPascalCasedNamesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public event Action One();
        protected event Action Two;
        internal event Action Three;
        private event Action Four;
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void EventFieldsWithNonPascalCasedNamesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public event Action one;
        protected event Action _two;
        private event Action THREE;
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1705",
                    Message = "Event names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 29)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1705",
                    Message = "Event names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 32)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1705",
                    Message = "Event names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 30)
                    }
                }
            );
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SymbolNamingAnalyzer();
        }
    }
}
