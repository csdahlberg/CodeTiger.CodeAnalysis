using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout
{
    public class BraceLayoutAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void NamespaceDeclarationWithValidBraceLayoutDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void NamespaceDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1 { }";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3500",
                    Message = "Namespaces should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 2, 11)
                    }
                }
            );
        }

        [Fact]
        public void NamespaceDeclarationsWithInvalidBraceLayoutProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1 {
}
namespace ClassLibrary2
{ }";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 2, 25)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 5, 1)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 5, 3)
                    }
                }
            );
        }

        [Fact]
        public void InitializerExpressionsWithValidBraceLayoutsDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private object _obj = new object { };
        private object _anon = new { Name = """" };
        private List<string> _list = new List<string> { """" };
        public Dictionary<string, int> _dictionary = new Dictionary { { """", 0 } };

        public void DoSomething()
        {
            var obj = new object { };
            var anon = new { Name = """" };
            var list = new List<string>
            {
                """"
            };
            var dictionary = new Dictionary
            {
                {
                    """",
                    0
                }
            };
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void InitializerExpressionsWithInvalidBraceLayoutsProduceDiagnostics()
        {
            DiagnosticResult CreateResult(int line, int column)
            {
                return new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line, column)
                    }
                };
            }

            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private object _obj = new object {
        };
        private object _anon = new { Name = """"
        };
        private List<string> _list = new List<string> {
            """" };
        public Dictionary<string, int> _dictionary = new Dictionary { {
            """", 0 } };

        public void DoSomething()
        {
            var obj = new object
            { };
            var anon = new
            { Name = """" };
            var list = new List<string>
            {
                """" };
            var dictionary = new Dictionary {
                { """",
                    0
                } };
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                CreateResult(6, 42),
                CreateResult(8, 36),
                CreateResult(10, 55),
                CreateResult(11, 16),
                CreateResult(12, 69),
                CreateResult(12, 71),
                CreateResult(13, 19),
                CreateResult(13, 21),
                CreateResult(18, 13),
                CreateResult(18, 15),
                CreateResult(20, 13),
                CreateResult(20, 25),
                CreateResult(23, 20),
                CreateResult(24, 45),
                CreateResult(25, 17),
                CreateResult(27, 17),
                CreateResult(27, 19)
            );
        }

        [Fact]
        public void CodeBlocksWithValidBraceLayoutsDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            if (true)
            {
            }
            else
            {
            }

            {
                int x = 1;
                switch (x)
                {
                    case 1:
                    {
                        break;
                    }
                }
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void CodeBlocksWithInvalidBraceLayoutsProduceDiagnostics()
        {
            DiagnosticResult CreateResult(int line, int column)
            {
                return new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line, column)
                    }
                };
            }

            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething() {
            if (true) {
            } else {
            }

            {
                int x = 1;
                switch (x) {
                    case 1: {
                        break;
                    }
                }
            }
        }

        public void DoSomething2()
        { if (true)
            { }
            else
            { }

            {
                int x = 1;
                switch (x) { case 1:
                    { break; } }
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                CreateResult(6, 35),
                CreateResult(7, 23),
                CreateResult(8, 13),
                CreateResult(8, 20),
                CreateResult(13, 28),
                CreateResult(14, 29),
                CreateResult(22, 9),
                CreateResult(23, 13),
                CreateResult(23, 15),
                CreateResult(25, 13),
                CreateResult(25, 15),
                CreateResult(29, 28),
                CreateResult(30, 32)
            );
        }

        [Fact]
        public void ClassDeclarationWithValidBraceLayoutDoesNotProduceDiagnostic()
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
        public void ClassDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1 { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3502",
                    Message = "Types should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 18)
                    }
                }
            );
        }

        [Fact]
        public void ClassDeclarationsWithInvalidBraceLayoutsProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1 {
    }

    public class Class2
    { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 25)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 5)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 7)
                    }
                }
            );
        }

        [Fact]
        public void StructDeclarationWithValidBraceLayoutDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public struct Struct1
    {
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void StructDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public struct Struct1 { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3502",
                    Message = "Types should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 19)
                    }
                }
            );
        }

        [Fact]
        public void StructDeclarationsWithInvalidBraceLayoutsProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public struct Struct1 {
    }

    public struct Struct2
    { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 27)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 5)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 7)
                    }
                }
            );
        }

        [Fact]
        public void InterfaceDeclarationWithValidBraceLayoutDoesNotProduceDiagnostic()
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
        public void InterfaceDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IInterface1 { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3502",
                    Message = "Types should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 22)
                    }
                }
            );
        }

        [Fact]
        public void InterfaceDeclarationsWithInvalidBraceLayoutsProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IInterface1 {
    }

    public interface IInterface2
    { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 34)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 5)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 7)
                    }
                }
            );
        }

        [Fact]
        public void EnumDeclarationWithValidBraceLayoutDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public enum Enum1
    {
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void EnumDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public enum Enum1 { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3502",
                    Message = "Types should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 17)
                    }
                }
            );
        }

        [Fact]
        public void EnumDeclarationsWithInvalidBraceLayoutsProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public enum Enum1 {
    }

    public enum Enum2
    { }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 23)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 5)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 7)
                    }
                }
            );
        }

        [Fact]
        public void AccessorListsWithValidBraceLayoutsDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public string Name { get; set; }
        public int Age
        {
            get { return 1; }
        }
        public event EventHandler OnSomething
        {
            add { }
            remove
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void AccessorListsWithInvalidBraceLayoutsProduceDiagnostics()
        {
            DiagnosticResult CreateResult(int line, int column)
            {
                return new DiagnosticResult
                {
                    Id = "CT3501",
                    Message = "Braces for multi-line elements should be on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line, column)
                    }
                };
            }

            string code = @"using System;
namespace ClassLibrary1
{
        public string Name {
            get; set; }
        public int Age
        { get { return 1;
        } }
        public event EventHandler OnSomething
        {
            add {
            }
            remove
            { }
        }
}";

            VerifyCSharpDiagnostic(code,
                CreateResult(4, 28),
                CreateResult(5, 23),
                CreateResult(7, 9),
                CreateResult(7, 15),
                CreateResult(8, 9),
                CreateResult(8, 11),
                CreateResult(11, 17),
                CreateResult(14, 13),
                CreateResult(14, 15)
            );
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BraceLayoutAnalyzer();
        }
    }
}
