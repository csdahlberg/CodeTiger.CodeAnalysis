using CodeTiger.CodeAnalysis.Analyzers.Naming;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Naming;

public class SymbolNamingAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void TypesWithPascalCasedNamesDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class ClassType
    {
    }
    public struct StructType
    {
    }
    public enum EnumType
    {
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TypesWithNonPascalCasedNamesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class classType
    {
    }
    public struct IStructType
    {
    }
    public enum ENUMTYPE
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1702",
                Message = "Type names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1702",
                Message = "Type names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 19)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1702",
                Message = "Type names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 17)
                }
            });
    }

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
                Message = "Constant field names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 26)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1703",
                Message = "Constant field names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 29)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1703",
                Message = "Constant field names should use pascal casing",
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
                Message = "Private field names should use camel casing with a leading underscore",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1704",
                Message = "Private field names should use camel casing with a leading underscore",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 30)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1704",
                Message = "Private field names should use camel casing with a leading underscore",
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
                Message = "Event names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 29)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1705",
                Message = "Event names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 32)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1705",
                Message = "Event names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 30)
                }
            });
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
                Message = "Delegate names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 30)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1706",
                Message = "Delegate names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 32)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1706",
                Message = "Delegate names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 34)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1706",
                Message = "Delegate names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 33)
                }
            });
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
                Message = "Property names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 23)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1707",
                Message = "Property names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 24)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1707",
                Message = "Property names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 22)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1707",
                Message = "Property names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 13, 24)
                }
            });
    }

    [Fact]
    public void PropertiesWithNamesPrefixedWithGetOrSetProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public string GetName { get; }
        private string SetName { set; }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1718",
                Message = "Property names should not be prefixed with 'Get' or 'Set'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 23)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1718",
                Message = "Property names should not be prefixed with 'Get' or 'Set'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 24)
                }
            });
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
                Message = "Method names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1708",
                Message = "Method names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 24)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1708",
                Message = "Method names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 22)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1708",
                Message = "Method names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 24)
                }
            });
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
                Message = "Enumeration member names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 9)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1709",
                Message = "Enumeration member names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 9)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1709",
                Message = "Enumeration member names should use pascal casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 9)
                }
            });
    }

    [Fact]
    public void VariablesWithCamelCasedNamesDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var objectVariable = new object();
            int intVariable1 = 13, intVariable2 = 36;
            intVariable1 = objectVariable == null ? 12 : intVariable2;
        }
        
        public static string GetString(int number)
        {
            return number.ToString();
        }
    }
    
    public static class Class2
    {
        public static void DoSomething()
        {
            var objectVariable = new object();
            int intVariable1 = 13, intVariable2 = 26;
            intVariable1 = objectVariable == null ? 12 : 14;
        }
        
        public static string GetString(int number)
        {
            return number.ToString();
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void VariablesWithNonCamellCasedNamesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var _objectVariable = new object();
            int IntVariable1 = 13, _intVariable2 = 36;
            IntVariable1 = _objectVariable == null ? 12 : _intVariable2;
        }
    }
    
    public static class Class2
    {
        public static void DoSomething()
        {
            var _objectVariable = new object();
            int IntVariable1 = 13, _IntVariable2 = 26;
            IntVariable1 = _objectVariable == null ? 12 : 14;
        }
        
        public static string GetString(int number)
        {
            return number.ToString();
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1710",
                Message = "Variable names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1710",
                Message = "Variable names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1710",
                Message = "Variable names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 36)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1710",
                Message = "Variable names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 18, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1710",
                Message = "Variable names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 19, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1710",
                Message = "Variable names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 19, 36)
                }
            });
    }

    [Fact]
    public void VariablesWithHungarianPrefixesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var oVariable = new object();
            int nVariable = 13, iVariable = 36;
            string strVariable = oVariable == null ? 12.ToString() : iVariable.ToString();
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 33)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 20)
                }
            });
    }

    [Fact]
    public void ParametersWithHungarianPrefixesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething(object oVariable, int nVariable, string strVariable)
        {
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 40)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 55)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 73)
                }
            });
    }

    [Fact]
    public void ParametersWithCamelCasedNamesDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething(string firstName, int age)
        {
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ParametersWithPascalCasedNamesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething(string FirstName, int Age)
        {
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1712",
                Message = "Parameter names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 40)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1712",
                Message = "Parameter names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 55)
                }
            });
    }

    [Fact]
    public void ParametersWithDiscardNamesWhereAllowedDoNotProduceDiagnostics()
    {
        string code = @"using System;
using System.Collections.Concurrent;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var dictionary = new ConcurrentDictionary<int, string>();
            dictionary.GetOrAdd(0, _ => ""0"");
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ParameterWithDiscardNamesWhereNotAllowedProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething(string _)
        {
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1712",
                Message = "Parameter names should use camel casing",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 40)
                }
            });
    }

    [Fact]
    public void FieldsWithHungarianPrefixesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        object _oVariable = new object();
        int nVariable = 13;
        string strVariable = oVariable == null ? 12.ToString() : nVariable.ToString();
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 16)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 13)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1713",
                Message = "Hungarian notation should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 16)
                }
            });
    }

    [Fact]
    public void InterfaceWithPascalCasedNamesDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public interface IInterface1
    {
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void InterfacesWithNonPascalCasedNamesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public interface interface1
    {
    }
    public interface _Interface2
    {
    }
    public interface INTERFACE3
    {
    }
    public interface Interface4
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1711",
                Message = "Interface names should use pascal casing prefixed with 'I'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 22)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1711",
                Message = "Interface names should use pascal casing prefixed with 'I'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 22)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1711",
                Message = "Interface names should use pascal casing prefixed with 'I'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 22)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1711",
                Message = "Interface names should use pascal casing prefixed with 'I'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 13, 22)
                }
            });
    }

    [Fact]
    public void PropertiesWithNamesNotPrefixedWithContainingTypeNameDoNotProduceDiagnostics()
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
    public class Run { public string RunnerId { get; set; } }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void PropertiesWithNamesPrefixedWithContainingTypeNameProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public object Class1One { get; set; }
        protected bool Class1Two => true;
        internal int Class1Three
        {
            get { return 3; }
            set { }
        }
        private object Class1FOUR => one;
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1714",
                Message = "Property names should not begin with the name of the containing type",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 23)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1714",
                Message = "Property names should not begin with the name of the containing type",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 24)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1714",
                Message = "Property names should not begin with the name of the containing type",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 22)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1714",
                Message = "Property names should not begin with the name of the containing type",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 13, 24)
                }
            });
    }

    [Fact]
    public void GenericTypeParametersWithValidNamesDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1<T, TFactory>
    {
        public TOut DoSomething<TIn, TOut>(TIn in)
        {
            return default(TOut);
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void GenericTypeParametersWithInvalidNamesProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1<t, TFac>
    {
        public Tout DoSomething<TInType, Tout>(TInType in)
        {
            return default(Tout);
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1715",
                Message = "Generic type parameter names should use pascal casing prefixed with 'T'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 25)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1716",
                Message = "Generic type parameter names should be descriptive",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 28)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1717",
                Message = "Generic type parameter names should not be suffixed with 'Type'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 33)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1715",
                Message = "Generic type parameter names should use pascal casing prefixed with 'T'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            });
    }

    [Fact]
    public void TypesWithNamesPrefixedWithAbstractProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class AbstractClass1
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1719",
                Message = "Type names should not be prefixed with 'Abstract'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            });
    }

    [Fact]
    public void TypesWithNamesPrefixedOrSuffixedWithBaseProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class BaseClass1
    {
    }
    public class Class1Base
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1720",
                Message = "Type names should not be prefixed or suffixed with 'Base'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1720",
                Message = "Type names should not be prefixed or suffixed with 'Base'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 18)
                }
            });
    }

    [Fact]
    public void AttributeTypesWithNamesSuffixedWithAttributeDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstAttribute : Attribute
    {
    }
    public class SecondAttribute : FirstAttribute
    {
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AttributeTypesWithNamesNotSuffixedWithAttributeProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstAttr : Attribute
    {
    }
    public class SecondAttr : FirstAttr
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1721",
                Message = "Attribute type names should be suffixed with 'Attribute'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1721",
                Message = "Attribute type names should be suffixed with 'Attribute'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 18)
                }
            });
    }

    [Fact]
    public void NonAttributeTypesWithNamesNotSuffixedWithAttributeDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstThing
    {
    }
    public class SecondThing : FirstThing
    {
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void NonAttributeTypesWithNamesSuffixedWithAttributeProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstAttribute
    {
    }
    public class SecondAttribute : FirstAttribute
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1722",
                Message = "Non-attribute type names should not be suffixed with 'Attribute'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1722",
                Message = "Non-attribute type names should not be suffixed with 'Attribute'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 18)
                }
            });
    }

    [Fact]
    public void ExceptionTypesWithNamesSuffixedWithExceptionDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstException : Exception
    {
    }
    public class SecondException : FirstException
    {
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ExceptionTypesWithNamesNotSuffixedWithExceptionProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstAttr : Exception
    {
    }
    public class SecondAttr : FirstAttr
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1723",
                Message = "Exception type names should be suffixed with 'Exception'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1723",
                Message = "Exception type names should be suffixed with 'Exception'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 18)
                }
            });
    }

    [Fact]
    public void NonExceptionTypesWithNamesNotSuffixedWithExceptionDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstThing
    {
    }
    public class SecondThing : FirstThing
    {
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void NonExceptionTypesWithNamesSuffixedWithExceptionProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstException
    {
    }
    public class SecondException : FirstException
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1724",
                Message = "Non-exception type names should not be suffixed with 'Exception'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1724",
                Message = "Non-exception type names should not be suffixed with 'Exception'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 18)
                }
            });
    }

    [Fact]
    public void EventArgsTypesWithNamesSuffixedWithEventArgsDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstEventArgs : EventArgs
    {
    }
    public class SecondEventArgs : FirstEventArgs
    {
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void EventArgsTypesWithNamesNotSuffixedWithEventArgsProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstArgs : EventArgs
    {
    }
    public class SecondArgs : FirstArgs
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1725",
                Message = "EventArgs type names should be suffixed with 'EventArgs'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1725",
                Message = "EventArgs type names should be suffixed with 'EventArgs'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 18)
                }
            });
    }

    [Fact]
    public void NonEventArgsTypesWithNamesNotSuffixedWithEventArgsDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstThing
    {
    }
    public class SecondThing : FirstThing
    {
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void NonEventArgsTypesWithNamesSuffixedWithEventArgsProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class FirstEventArgs
    {
    }
    public class SecondEventArgs : FirstEventArgs
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1726",
                Message = "Non-EventArgs type names should not be suffixed with 'EventArgs'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1726",
                Message = "Non-EventArgs type names should not be suffixed with 'EventArgs'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 18)
                }
            });
    }

    [Fact]
    public void MethodsReturningATaskWithNamesSuffixedWithAsyncDoNotProduceDiagnostics()
    {
        string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class TestClass
    {
        public Task<bool> DoSomethingAsync() { return Task.FromResult(true); }
        protected async Task DoSomething2Async() { }
        private async void DoSomething3Async() { }
        private T DoSomething4Async<T>(T original) where T : Task { return original; }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MethodsReturningATaskWithNamesNotSuffixedWithAsyncProduceDiagnostics()
    {
        string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class TestClass
    {
        public Task DoSomething() { return Task.FromResult(true); }
        protected async Task DoSomething2() { }
        private async void DoSomething3() { }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1727",
                Message = "Methods returning a Task should be suffixed with 'Async'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1727",
                Message = "Methods returning a Task should be suffixed with 'Async'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 30)
                }
            });
    }

    [Fact]
    public void MethodsNotReturningATaskWithNamesNotSuffixedWithAsyncDoNotProduceDiagnostics()
    {
        string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class TestClass
    {
        public bool DoSomething() { return true; }
        protected bool DoSomething2() { }
        private void DoSomething3() { }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MethodsNotReturningATaskWithNamesSuffixedWithAsyncProduceDiagnostics()
    {
        string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class TestClass
    {
        public bool DoSomethingAsync() { return true; }
        protected bool DoSomething2Async() { }
        private void DoSomething3Async() { }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1728",
                Message = "Methods not returning a Task or void should not be suffixed with 'Async'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1728",
                Message = "Methods not returning a Task or void should not be suffixed with 'Async'",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 24)
                }
            });
    }

    [Fact]
    public void ClassWithNameThatEqualsContainingNamespaceNameProducesDiagnostics()
    {
        string code = @"namespace Test
{
    public class Test
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1730",
                Message = "Type names should not match their containing namespace name",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 3, 18)
                }
            });
    }

    [Fact]
    public void StructWithNameThatEqualsContainingNamespaceNameProducesDiagnostics()
    {
        string code = @"namespace Test
{
    public struct Test
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1730",
                Message = "Type names should not match their containing namespace name",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 3, 19)
                }
            });
    }

    [Fact]
    public void EnumWithNameThatEqualsContainingNamespaceNameProducesDiagnostics()
    {
        string code = @"namespace Test
{
    public enum Test
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1730",
                Message = "Type names should not match their containing namespace name",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 3, 17)
                }
            });
    }

    [Fact]
    public void InterfaceWithNameThatEqualsContainingNamespaceNameProducesDiagnostics()
    {
        string code = @"namespace ITest
{
    public interface ITest
    {
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1730",
                Message = "Type names should not match their containing namespace name",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 3, 22)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new SymbolNamingAnalyzer();
    }
}
