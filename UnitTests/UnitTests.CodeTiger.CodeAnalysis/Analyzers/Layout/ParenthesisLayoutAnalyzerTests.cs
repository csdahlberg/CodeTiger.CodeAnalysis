using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout
{
    public class ParenthesisLayoutAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ConstructorInitializersWithParenthesisOnSameLineDoNotProduceDiagnostic()
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
        public void ConstructorInitializersWithCloseParenthesisOnNewLineProduceDiagnostic()
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
            : this(false
                )
        {
        }

        public Class2(bool flag)
            : base(flag
                )
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 15, 17)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 21, 17)
                    }
                }
            );
        }

        [Fact]
        public void CatchClausWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void CatchClauseWithCloseParenthesisOnNewLineProducesDiagnostic()
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
            catch (Exception ex
                )
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 12, 17)
                    }
                }
            );
        }

        [Fact]
        public void DefaultExpressionWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void DefaultExpressionWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            int i = default(int
                );
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void FixedStatementWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void FixedStatementWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public unsafe void DoSomething()
        {
            var thing = new BaseClass();
            fixed (int* age = &thing.Age
                )
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 10, 17)
                    }
                }
            );
        }

        [Fact]
        public void ForStatementWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void ForStatementWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            for (int i = 0; i < 10; i += 1
                )
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void ForEachStatementWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void ForEachStatementWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            foreach (int i in Enumerable.Empty<int>()
                )
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void IfStatementWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void IfStatementWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            if (true
                )
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void LockStatementWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void LockStatementWithClosingParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            lock(this
                )
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void SizeOfExpressionWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void SizeOfExpressionWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public unsafe void DoSomething()
        {
            int size = sizeof(IntPtr
                );
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void SwitchStatementWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void SwitchStatementWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            switch(int.MinValue
                )
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
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void TypeOfExpressionWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void TypeOfExpressionWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var type = typeof(Class1
                );
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void UsingStatementWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void UsingStatementWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            using (var disposable = (IDisposable)null
                )
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void WhileStatementWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void WhileStatementWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            while (false
                )
            {
            }
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void NameOfExpressionWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void NameOfExpressionWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            string name = nameof(Class1
                );
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                }
            );
        }

        [Fact]
        public void InvocationExpressionsWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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
        public void InvocationExpressionsWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var time = DateTime.Now.AddMinutes(5
                );
            int min = Math.Min(1, 2
                );
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 17)
                    }
                }
            );
        }

        [Fact]
        public void NewExpressionsWithParenthesisOnSameLineDoesNotProduceDiagnostic()
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

        [Fact]
        public void NewExpressionsWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var time = new DateTime(2000, 1, 1
                );
            string text = new string(new[] { 'a', 'b', 'c' }
                );
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3534",
                    Message = "Closing parenthesis should be on the same line as the preceding argument.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 17)
                    }
                }
            );
        }

        [Fact]
        public void CastExpressionsWithParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var time = (DateTime)null;
            int min = (int)(new int());
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void CastExpressionsWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
            var time = (DateTime
                )null;
            int min = (int
                )(new int());
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 17)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 17)
                    }
                }
            );
        }

        [Fact]
        public void ConstructorDeclarationWithParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Class1()
        {
        }
        public Class1(string name)
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ConstructorDeclarationWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Class1
            ()
        {
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
                        new DiagnosticResultLocation("Test0.cs", 7, 13)
                    }
                }
            );
        }

        [Fact]
        public void ConstructorDeclarationWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Class1(
            )
        {
        }
        public Class1(string name
            )
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3536",
                    Message = "Empty parentheses should be on the same line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 13)
                    }
                }
            );
        }

        [Fact]
        public void DestructorDeclarationWithParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        ~Class1()
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void DestructorDeclarationWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        ~Class1
            ()
        {
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
                        new DiagnosticResultLocation("Test0.cs", 7, 13)
                    }
                }
            );
        }

        [Fact]
        public void DestructorDeclarationWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        ~Class1(
            )
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3536",
                    Message = "Empty parentheses should be on the same line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 13)
                    }
                }
            );
        }

        [Fact]
        public void MethodDeclarationWithParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething()
        {
        }
        public void DoSomething(string name)
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void MethodDeclarationWithOpenParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething
            ()
        {
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
                        new DiagnosticResultLocation("Test0.cs", 7, 13)
                    }
                }
            );
        }

        [Fact]
        public void MethodDeclarationWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public void DoSomething(
            )
        {
        }
        public void DoSomething(string name
            )
        {
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3536",
                    Message = "Empty parentheses should be on the same line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 11, 13)
                    }
                }
            );
        }

        [Fact]
        public void ParenthesizedLambdaExpressionWithParenthesisOnSameLineDoesNotProduceDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private Func<string, bool> _isValidFunc = (s) => true;
        public void DoSomething()
        {
            Func<int, int, int> sumFunc = (a, b) => a + b;
        }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ParenthesizedLambdaExpressionWithCloseParenthesisOnNewLineProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private Func<string, bool> _isValidFunc = (
            ) => true;
        public void DoSomething()
        {
            Func<int, int, int> sumFunc = (a, b
                ) => a + b;
        }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3536",
                    Message = "Empty parentheses should be on the same line.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3535",
                    Message = "Closing parenthesis should be on the same line as the preceding element.",
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
