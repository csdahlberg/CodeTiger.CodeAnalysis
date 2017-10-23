using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout
{
    public class ParenthesisLayoutAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ConstructorInitializersWithOpenParenthesisOnSameLineDoNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Class1(bool flag)
        {
        }
    }

    public class Class2
    {
        public Class2()
            : this(false)
        {
        }

        public Class2(bool flag)
            : base(flag)
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ConstructorInitializersWithOpenParenthesisOnNewLineProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Class1(bool flag)
        {
        }
    }

    public class Class2
    {
        public Class2()
            : this
                (false)
        {
        }

        public Class2(bool flag)
            : base
                (flag)
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 15, 17)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 21, 17)
                    }
                }
            );
        }

        [Fact]
        public void CatchClausWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
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
            catch (Exception ex)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void CatchClauseWithOpenParenthesisOnNewLineProducesDiagnostic()
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
            catch
            (Exception ex)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 12, 13)
                    }
                }
            );
        }

        [Fact]
        public void DefaultExpressionWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            int i = default(int);
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void DefaultExpressionWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            int i = default
                (int);
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void FixedStatementWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public unsafe void DoSomething()
        {
            var thing = new BaseClass();
            fixed (int* age = &thing.Age)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void FixedStatementWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public unsafe void DoSomething()
        {
            var thing = new BaseClass();
            fixed
                (int* age = &thing.Age)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 10, 17)
                    }
                }
            );
        }

        [Fact]
        public void ForStatementWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            for (int i = 0; i < 10; i += 1)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ForStatementWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            for
                (int i = 0; i < 10; i += 1)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void ForEachStatementWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            foreach (int i in Enumerable.Empty<int>())
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ForEachStatementWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            foreach
                (int i in Enumerable.Empty<int>())
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void IfStatementWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void IfStatementWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            if
                (true)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void LockStatementWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            lock (this)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void LockStatementWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            lock
                (this)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void SizeOfExpressionWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public unsafe void DoSomething()
        {
            int size = sizeof(IntPtr);
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void SizeOfExpressionWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public unsafe void DoSomething()
        {
            int size = sizeof
                (IntPtr);
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void SwitchStatementWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            switch (int.MinValue)
            {
                default:
                    break;
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void SwitchStatementWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            switch
                (int.MinValue)
            {
                default:
                    break;
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void TypeOfExpressionWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var type = typeof(Class1);
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void TypeOfExpressionWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var type = typeof
                (Class1);
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void UsingStatementWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            using (var disposable = (IDisposable)null)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void UsingStatementWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            using
                (var disposable = (IDisposable)null)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void WhileStatementWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            while (false)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void WhileStatementWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            while
                (false)
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void NameOfExpressionWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            string name = nameof(Class1);
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void NameOfExpressionWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            string name = nameof
                (Class1);
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3532",
                    Message = "Opening parenthesis should be on the same line as the preceding keyword.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void InvocationExpressionsWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var time = DateTime.Now.AddMinutes(5);
            int min = Math.Min(1, 2);
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void InvocationExpressionsWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var time = DateTime.Now.AddMinutes
                (5);
            int min = Math.Min
                (1, 2);
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3533",
                    Message = "Opening parenthesis should be on the same line as the preceding identifier.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3533",
                    Message = "Opening parenthesis should be on the same line as the preceding identifier.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 17)
                    }
                }
            );
        }

        [Fact]
        public void NewExpressionsWithOpenParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var time = new DateTime(2000, 1, 1);
            int min = new int();
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void NewExpressionsWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var time = new DateTime
                (2000, 1, 1);
            int min = new int
                ();
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3533",
                    Message = "Opening parenthesis should be on the same line as the preceding identifier.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3533",
                    Message = "Opening parenthesis should be on the same line as the preceding identifier.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 17)
                    }
                }
            );
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ParenthesisLayoutAnalyzer();
        }
    }
}
