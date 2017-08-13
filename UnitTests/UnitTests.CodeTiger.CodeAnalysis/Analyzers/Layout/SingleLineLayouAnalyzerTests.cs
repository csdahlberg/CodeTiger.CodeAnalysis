using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout
{
    public class SingleLineLayouAnalyzerTests : DiagnosticVerifier
    {
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
        public void AutoPropertyDeclarationOnSingleLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public string Name { get; set; }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void AutoPropertyDeclarationsOnMultipleLinesProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public string Name
        {
            get;
            set;
        }
        public int Age
        {
            get; set;
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3503",
                    Message = "Auto properties should be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 23)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3503",
                    Message = "Auto properties should be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 20)
                    }
                }
            );
        }

        [Fact]
        public void NonAutoPropertyDeclarationOnMultipleLinesDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public string Name
        {
            get { return ""; }
            set { }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void NonAutoPropertyDeclarationOnSingleLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public string Name { get { return """"; } set { } }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3504",
                    Message = "Non-auto properties should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 23)
                    }
                }
            );
        }

        [Fact]
        public void TrivialAccessorsOnSingleLinesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private string _name;
        private EventHandler _testEvent;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public event EventHandler TestEvent
        {
            add { _testEvent += value; }
            remove { _testEvent -= value; }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void TrivialAccessorsOnMultipleLinesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private string _name;
        private EventHandler _testEvent;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public event EventHandler TestEvent
        {
            add
            {
                _testEvent += value;
            }
            remove
            {
                _testEvent -= value;
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3505",
                    Message = "Trivial accessors should be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 10, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3505",
                    Message = "Trivial accessors should be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 14, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3505",
                    Message = "Trivial accessors should be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 21, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3505",
                    Message = "Trivial accessors should be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 25, 13)
                    }
                }
            );
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SingleLineLayoutAnalyzer();
        }
    }
}
