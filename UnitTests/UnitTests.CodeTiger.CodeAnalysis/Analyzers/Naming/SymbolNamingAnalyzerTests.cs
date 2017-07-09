﻿using CodeTiger.CodeAnalysis.Analyzers.Naming;
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

        [Fact]
        public void DelegatesWithPascalCasedNamesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public delegate void One();
        protected delegate int Two(string x);
        internal delegate string Three(int x);
        private delegate object Four();
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void DelegatesWithNonPascalCasedNamesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public delegate void one();
        protected delegate int TWO(string x);
        internal delegate string _three(int x);
        private delegate object _FOUR();
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1706",
                    Message = "Delegate names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 30)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1706",
                    Message = "Delegate names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 32)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1706",
                    Message = "Delegate names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 34)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1706",
                    Message = "Delegate names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 33)
                    }
                }
            );
        }

        [Fact]
        public void PropertiesWithPascalCasedNamesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public object One { get; set; }
        protected bool Two => true;
        internal int Three
        {
            get { return 3; }
            set { }
        }
        private object Four => One;
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void PropertiesWithNonPascalCasedNamesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public object one { get; set; }
        protected bool _two => true;
        internal int _Three
        {
            get { return 3; }
            set { }
        }
        private object FOUR => one;
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1707",
                    Message = "Property names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 23)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1707",
                    Message = "Property names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 24)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1707",
                    Message = "Property names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 22)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1707",
                    Message = "Property names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 13, 24)
                    }
                }
            );
        }

        [Fact]
        public void MethodsWithPascalCasedNamesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void One() { }
        protected bool Two() => true;
        internal int Three() { return 3; }
        private object Four(int a) { return a; }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void MethodsWithNonPascalCasedNamesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void one() { }
        protected bool _two() => true;
        internal int _Three() { return 3; }
        private object FOUR(int a) { return a; }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1708",
                    Message = "Method names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 21)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1708",
                    Message = "Method names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 24)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1708",
                    Message = "Method names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 22)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1708",
                    Message = "Method names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 24)
                    }
                }
            );
        }

        [Fact]
        public void EnumerationMembersWithPascalCasedNamesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public enum Enum1
    {
        One,
        Two = 2,
        Three = 1 + 2,
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void EnumerationMembersWithNonPascalCasedNamesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public enum Enum1
    {
        one,
        _two = 2,
        THREE = 1 + 2,
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1709",
                    Message = "Enumeration member names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 9)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1709",
                    Message = "Enumeration member names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 9)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT1709",
                    Message = "Enumeration member names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 9)
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
