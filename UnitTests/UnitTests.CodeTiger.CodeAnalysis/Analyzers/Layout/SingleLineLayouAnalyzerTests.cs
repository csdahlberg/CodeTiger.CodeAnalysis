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

        [Fact]
        public void NonTrivialAccessorsOnMultipleLinesDoNotProduceDiagnostics()
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
                if (DateTime.Now > DateTime.MinValue)
                    return _name;
                else
                    return """";
            }
            set
            {
                if (value == null)
                {
                    throw new Exception();
                }

                _name = value; }
        }
        public event EventHandler TestEvent
        {
            add
            {
                _name = """";
                _testEvent += value;
            }
            remove
            {
                _testEvent -= value; ToString();
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void NonTrivialAccessorsOnSingleLinesProduceDiagnostics()
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
            get { DateTime.Now.ToString(); return """"; }
            set { DateTime.Now.ToString(); _name = value; }
        }
        public event EventHandler TestEvent
        {
            add { _name = """"; _testEvent += value; }
            remove { _testEvent -= value; ToString(); }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3506",
                    Message = "Non-trivial accessors should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 10, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3506",
                    Message = "Non-trivial accessors should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3506",
                    Message = "Non-trivial accessors should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 15, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3506",
                    Message = "Non-trivial accessors should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 16, 13)
                    }
                }
            );
        }

        [Fact]
        public void MethodOnMultipleLinesDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
        }
        public bool IsEnabled() => true;
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void MethodOnSingleLinesProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething() { }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3507",
                    Message = "Methods should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 21)
                    }
                }
            );
        }

        [Fact]
        public void TryStatementOnMultipleLinesDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
                int.Parse(""blah"");
            }
            catch
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void TryStatementOnSingleLinesProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try { int.Parse(""blah""); } catch { }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3508",
                    Message = "Try statements should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3510",
                    Message = "Catch clauses should begin on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 40)
                    }
                }
            );
        }

        [Fact]
        public void NonTrivialCatchClausesOnMultipleLinesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
            }
            catch (Exception)
            {
                Console.WriteLine(""ERROR!"");
                throw;
            }
            try
            {
            }
            catch (Exception ex) when(ex.Message?.Contains(""SQL"") == true)
            {
                if (ex.HResult != 0)
                {
                    throw new Exception(""Blah"", ex);
                }
                throw;
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void NonTrivialCatchClausesOnSingleLinesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
            }
            catch (Exception) { Console.WriteLine(""ERROR!""); throw; }
            try
            {
            }
            catch (Exception ex) when(ex.Message?.Contains(""SQL"") == true) { DateTime.Now.ToString(); return; }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3509",
                    Message = "Non-trivial catch clauses should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3509",
                    Message = "Non-trivial catch clauses should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 15, 13)
                    }
                }
            );
        }

        [Fact]
        public void CatchClausesBeginningOnNewLinesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
            }
            catch (Exception) { }
            try
            {
            }
            catch (Exception ex) when(ex.Message?.Contains(""SQL"") == true)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void CatchClausesNotBeginningOnNewLinesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
            } catch (Exception) { }
            try
            {
            } catch (Exception ex) when(ex.Message?.Contains(""SQL"") == true)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3510",
                    Message = "Catch clauses should begin on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 10, 15)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3510",
                    Message = "Catch clauses should begin on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 13, 15)
                    }
                }
            );
        }

        [Fact]
        public void FinallyClausesOnMultipleLinesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
            }
            finally
            {
            }
            try
            {
            }
            finally
            {
                if (DateTime.Now > DateTime.MinValue)
                {
                    throw new Exception();
                }
                return;
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void FinallyClausesOnSingleLinesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
            }
            finally { }
            try
            {
            }
            finally { DateTime.Now.ToString(); return; }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3511",
                    Message = "Finally clauses should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3511",
                    Message = "Finally clauses should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 15, 13)
                    }
                }
            );
        }

        [Fact]
        public void FinallyClauseBeginningOnNewLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
            }
            finally
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void FinallyClauseNotBeginningOnNewLineProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            try
            {
            } finally
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3512",
                    Message = "Finally clauses should begin on a new line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 10, 15)
                    }
                }
            );
        }

        [Fact]
        public void IfStatementsOnMultipleLinesDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            if (DateTime.Now > DateTime.MinValue)
            {
            }
            if (DateTime.Now < DateTime.MaxValue)
            {
            }
            else
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void IfStatementsOnSingleLinesProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            if (DateTime.Now > DateTime.MinValue) { }
            if (DateTime.Now < DateTime.MaxValue) { } else { }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3513",
                    Message = "If statements should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3513",
                    Message = "If statements should not be defined on a single line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 13)
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
