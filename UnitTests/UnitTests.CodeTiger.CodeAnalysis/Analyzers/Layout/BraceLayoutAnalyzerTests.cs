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

            do
            {
            } while (DateTime.Now < DateTime.MinValue);
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

        [Fact]
        public void CodeBlocksWithBracesDoNotProduceDiagnostics()
        {
            string code = @"using System;
using System.Linq;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            if (false)
            {
            }
            else if (true)
            {
            }
            else
            {
            }

            for (int i = 0; i < DateTime.Now.Day; i += 1)
            {
            }

            foreach (int i in Enumerable.Empty<int>())
            {
            }

            while (false)
            {
            }

            do
            {
            } while (false);

            using (var disposable = (IDisposable)null)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void CodeBlocksWithoutBracesProduceDiagnostics()
        {
            string code = @"using System;
using System.Linq;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            if (false)
                ;
            else if (true)
                ;
            else
                ;

            for (int i = 0; i < DateTime.Now.Day; i += 1)
                ;

            foreach (int i in Enumerable.Empty<int>())
                ;

            while (false)
                ;

            do
                ;
            while (false);

            using (var disposable = (IDisposable)null)
                ;
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3525",
                    Message = "Braces should not be omitted from code blocks.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3525",
                    Message = "Braces should not be omitted from code blocks.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 18)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3525",
                    Message = "Braces should not be omitted from code blocks.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 13, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3525",
                    Message = "Braces should not be omitted from code blocks.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 16, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3525",
                    Message = "Braces should not be omitted from code blocks.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 19, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3525",
                    Message = "Braces should not be omitted from code blocks.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 22, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3525",
                    Message = "Braces should not be omitted from code blocks.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 25, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3525",
                    Message = "Braces should not be omitted from code blocks.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 29, 13)
                    }
                }
            );
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BraceLayoutAnalyzer();
        }
    }
}
