using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout;

public class BinaryOperatorLayoutAnalyzerTests : DiagnosticVerifier
{
    protected override bool CompilationAllowsUnsafeCode => true;

    [Fact]
    public void SingleLineExpressionsDoNotProduceDiagnostics()
    {
        string code = @"using System;
using System.Linq;
namespace ClassLibrary1.Namespace1
{
    public class Class1
    {
        public void DoSomething()
        {
            var values = new Values { X = 1, Y = 2 };
            values.X.ToString();

            unsafe
            {
                var valuesPointer = &values;
                valuesPointer->Y.ToString();
            }
        }

        struct Values { public int X; public int Y; }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MultiLineExpressionsSplitBeforeDotOrMemberAccessTokensDoNotProduceDiagnostics()
    {
        string code = @"using System;
using System
    .Linq;
namespace ClassLibrary1
    .Namespace1
{
    public class Class1
    {
        public void DoSomething()
        {
            var values = new Values { X = 1, Y = 2 };
            values
                .X
                .ToString();

            unsafe
            {
                var valuesPointer = &values;
                valuesPointer
                    ->Y
                    .ToString();
            }
        }

        struct Values { public int X; public int Y; }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MultiLineExpressionsSplitAfterDotOrMemberAccessTokensProduceDiagnostics()
    {
        string code = @"using System;
using System.
    Linq;
namespace ClassLibrary1.
    Namespace1
{
    public class Class1
    {
        public void DoSomething()
        {
            var values = new Values { X = 1, Y = 2 };
            values.
                X.
                ToString();

            unsafe
            {
                var valuesPointer = &values;
                valuesPointer->
                    Y.
                    ToString();
            }
        }

        struct Values { public int X; public int Y; }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3530",
                Message = "Multi-line expressions should not be split after a dot or member access token",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 2, 13)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3530",
                Message = "Multi-line expressions should not be split after a dot or member access token",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 24)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3530",
                Message = "Multi-line expressions should not be split after a dot or member access token",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 12, 19)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3530",
                Message = "Multi-line expressions should not be split after a dot or member access token",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 13, 18)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3530",
                Message = "Multi-line expressions should not be split after a dot or member access token",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 19, 30)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3530",
                Message = "Multi-line expressions should not be split after a dot or member access token",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 20, 22)
                }
            });
    }

    [Fact]
    public void AdditionOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 + 2;
        public void DoSomething()
        {
            int nextAge = Age + 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AdditionOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            + 2;
        public void DoSomething()
        {
            int nextAge = Age
                + 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AdditionOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 +
            2;
        public void DoSomething()
        {
            int nextAge = Age +
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void SubtractionOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 - 2;
        public void DoSomething()
        {
            int nextAge = Age - 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void SubtractionOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            - 2;
        public void DoSomething()
        {
            int nextAge = Age
                - 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void SubtractionOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 -
            2;
        public void DoSomething()
        {
            int nextAge = Age -
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void MultiplicationOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 * 2;
        public void DoSomething()
        {
            int nextAge = Age * 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MultiplicationOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            * 2;
        public void DoSomething()
        {
            int nextAge = Age
                * 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MultiplicationOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 *
            2;
        public void DoSomething()
        {
            int nextAge = Age *
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void DivisionOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 / 2;
        public void DoSomething()
        {
            int nextAge = Age / 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void DivisionOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            / 2;
        public void DoSomething()
        {
            int nextAge = Age
                / 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void DivisionOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 /
            2;
        public void DoSomething()
        {
            int nextAge = Age /
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void ModuloOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 % 2;
        public void DoSomething()
        {
            int nextAge = Age % 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ModuloOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            % 2;
        public void DoSomething()
        {
            int nextAge = Age
                % 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ModuloOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 %
            2;
        public void DoSomething()
        {
            int nextAge = Age %
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void LessThanOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsYoung { get; set; } = 1 < 200;
        public void DoSomething()
        {
            bool isYoung = 1 < 200;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LessThanOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsYoung { get; set; } = 1
            < 200;
        public void DoSomething()
        {
            bool isYoung = 1
                < 200;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LessThanOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsYoung { get; set; } = 1 <
            200;
        public void DoSomething()
        {
            bool isYoung = 1 <
                200;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 47)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 30)
                }
            });
    }

    [Fact]
    public void GreaterThanOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsOld { get; set; } = 1 > 200;
        public void DoSomething()
        {
            bool isOld = 1 > 200;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void GreaterThanOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsOld { get; set; } = 1
            > 200;
        public void DoSomething()
        {
            bool isOld = 1
                > 200;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void GreaterThanOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsOld { get; set; } = 1 >
            200;
        public void DoSomething()
        {
            bool isOld = 1 >
                200;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 28)
                }
            });
    }

    [Fact]
    public void LessThanOrEqualToOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsYoung { get; set; } = 1 <= 200;
        public void DoSomething()
        {
            bool isYoung = 1 <= 200;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LessThanOrEqualToOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsYoung { get; set; } = 1
            <= 200;
        public void DoSomething()
        {
            bool isYoung = 1
                <= 200;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LessThanOrEqualToOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsYoung { get; set; } = 1 <=
            200;
        public void DoSomething()
        {
            bool isYoung = 1 <=
                200;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 47)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 30)
                }
            });
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsOld { get; set; } = 1 >= 200;
        public void DoSomething()
        {
            bool isOld = 1 >= 200;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsOld { get; set; } = 1
            >= 200;
        public void DoSomething()
        {
            bool isOld = 1
                >= 200;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsOld { get; set; } = 1 >=
            200;
        public void DoSomething()
        {
            bool isOld = 1 >=
                200;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 28)
                }
            });
    }

    [Fact]
    public void EqualsOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsTwo { get; set; } = 1 == 2;
        public void DoSomething()
        {
            bool isTwo = 1 == 2;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void EqualsOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsTwo { get; set; } = 1
            == 2;
        public void DoSomething()
        {
            bool isTwo = 1
                == 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void EqualsOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsTwo { get; set; } = 1 ==
            2;
        public void DoSomething()
        {
            bool isTwo = 1 ==
                2;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 28)
                }
            });
    }

    [Fact]
    public void NotEqualsOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsNotTwo { get; set; } = 1 != 2;
        public void DoSomething()
        {
            bool isNotTwo = 1 != 2;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void NotEqualsOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsNotTwo { get; set; } = 1
            != 2;
        public void DoSomething()
        {
            bool isNotTwo = 1
                != 2;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void NotEqualsOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsNotTwo { get; set; } = 1 !=
            2;
        public void DoSomething()
        {
            bool isNotTwo = 1 !=
                2;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 48)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void IsOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsInt32 { get; set; } = (object)1 is int;
        public void DoSomething()
        {
            bool isInt32 = (object)1 is int;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void IsOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsInt32 { get; set; } = (object)1
            is int;
        public void DoSomething()
        {
            bool isInt32 = (object)1
                is int;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void IsOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsInt32 { get; set; } = (object)1 is
            int;
        public void DoSomething()
        {
            bool isInt32 = (object)1 is
                int;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 55)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 38)
                }
            });
    }

    [Fact]
    public void AsOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static string _name;
        public string Name { get; set; } = _name as string;
        public void DoSomething()
        {
            string name = Name as string;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AsOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static string _name;
        public string Name { get; set; } = _name
            as string;
        public void DoSomething()
        {
            string name = Name
                as string;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AsOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static string _name;
        public string Name { get; set; } = _name as
            string;
        public void DoSomething()
        {
            string name = Name as
                string;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 50)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 32)
                }
            });
    }

    [Fact]
    public void LeftShiftOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 << 2;
        public void DoSomething()
        {
            int nextAge = Age << 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LeftShiftOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            << 2;
        public void DoSomething()
        {
            int nextAge = Age
                << 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LeftShiftOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 <<
            2;
        public void DoSomething()
        {
            int nextAge = Age <<
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void RightShiftOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 >> 2;
        public void DoSomething()
        {
            int nextAge = Age >> 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void RightShiftOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            >> 2;
        public void DoSomething()
        {
            int nextAge = Age
                >> 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void RightShiftOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 >>
            2;
        public void DoSomething()
        {
            int nextAge = Age >>
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void BitwiseAndOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 & 2;
        public void DoSomething()
        {
            int nextAge = Age & 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void BitwiseAndOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            & 2;
        public void DoSomething()
        {
            int nextAge = Age
                & 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void BitwiseAndOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 &
            2;
        public void DoSomething()
        {
            int nextAge = Age &
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void BitwiseOrOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 | 2;
        public void DoSomething()
        {
            int nextAge = Age | 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void BitwiseOrOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            | 2;
        public void DoSomething()
        {
            int nextAge = Age
                | 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void BitwiseOrOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 |
            2;
        public void DoSomething()
        {
            int nextAge = Age |
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void LogicalAndOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsSchrödinger { get; set; } = true && false;
        public void DoSomething()
        {
            bool isSchrödinger = true && false;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LogicalAndOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsSchrödinger { get; set; } = true
            && false;
        public void DoSomething()
        {
            bool isSchrödinger = true
                && false;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LogicalAndOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsSchrödinger { get; set; } = true &&
            false;
        public void DoSomething()
        {
            bool isSchrödinger = true &&
                false;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 56)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 39)
                }
            });
    }

    [Fact]
    public void LogicalOrOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsEither { get; set; } = true || false;
        public void DoSomething()
        {
            bool isEither = true || false;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LogicalOrOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsEither { get; set; } = true
            || false;
        public void DoSomething()
        {
            bool isEither = true
                || false;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LogicalOrOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public bool IsEither { get; set; } = true ||
            false;
        public void DoSomething()
        {
            bool isEither = true ||
                false;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 51)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 34)
                }
            });
    }

    [Fact]
    public void ExclusiveOrOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 ^ 2;
        public void DoSomething()
        {
            int nextAge = Age ^ 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ExclusiveOrOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1
            ^ 2;
        public void DoSomething()
        {
            int nextAge = Age
                ^ 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ExclusiveOrOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = 1 ^
            2;
        public void DoSomething()
        {
            int nextAge = Age ^
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 42)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 31)
                }
            });
    }

    [Fact]
    public void CoalesceOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = (int?)1 ?? 2;
        public void DoSomething()
        {
            int nextAge = (int?)Age ?? 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void CoalesceOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = (int?)1
            ?? 2;
        public void DoSomething()
        {
            int nextAge = (int?)Age
                ?? 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void CoalesceOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = (int?)1 ??
            2;
        public void DoSomething()
        {
            int nextAge = (int?)Age ??
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 48)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 37)
                }
            });
    }

    [Fact]
    public void ConditionalOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = true ? 1 : 0;
        public void DoSomething()
        {
            int nextAge = true ? 1 : 0;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ConditionalOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = true
            ? 1
            : 0;
        public void DoSomething()
        {
            int nextAge = true
                ? 1
                : 0;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ConditionalOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int Age { get; set; } = true ?
            1 :
            0;
        public void DoSomething()
        {
            int nextAge = true ?
                1 :
                0;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 15)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 32)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 12, 19)
                }
            });
    }

    [Fact]
    public void SimpleAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age = 2;
        public void DoSomething()
        {
            int nextAge = _age = 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void SimpleAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age
            = 2;
        public void DoSomething()
        {
            int nextAge = _age
                = 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void SimpleAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age =
            2;
        public void DoSomething()
        {
            int nextAge = _age =
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 32)
                }
            });
    }

    [Fact]
    public void AddAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age += 2;
        public void DoSomething()
        {
            int nextAge = Age += 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AddAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age
            += 2;
        public void DoSomething()
        {
            int nextAge = Age
                += 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AddAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age +=
            2;
        public void DoSomething()
        {
            int nextAge = Age +=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 31)
                }
            });
    }

    [Fact]
    public void SubtractAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age -= 2;
        public void DoSomething()
        {
            int nextAge = _age -= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void SubtractAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age
            -= 2;
        public void DoSomething()
        {
            int nextAge = _age
                -= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void SubtractAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age -=
            2;
        public void DoSomething()
        {
            int nextAge = _age -=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 32)
                }
            });
    }

    [Fact]
    public void MultiplyAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age *= 2;
        public void DoSomething()
        {
            int nextAge = Age *= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MultiplyAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age
            *= 2;
        public void DoSomething()
        {
            int nextAge = Age
                *= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void MultiplyAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age *=
            2;
        public void DoSomething()
        {
            int nextAge = Age *=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 31)
                }
            });
    }

    [Fact]
    public void DivideAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age /= 2;
        public void DoSomething()
        {
            int nextAge = Age /= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void DivideAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age
            /= 2;
        public void DoSomething()
        {
            int nextAge = Age
                /= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void DivideAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age /=
            2;
        public void DoSomething()
        {
            int nextAge = Age /=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 31)
                }
            });
    }

    [Fact]
    public void ModuloAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age %= 2;
        public void DoSomething()
        {
            int nextAge = Age %= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ModuloAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age
            %= 2;
        public void DoSomething()
        {
            int nextAge = Age
                %= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ModuloAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age %=
            2;
        public void DoSomething()
        {
            int nextAge = Age %=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 31)
                }
            });
    }

    [Fact]
    public void AndAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age &= 2;
        public void DoSomething()
        {
            int nextAge = Age &= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AndAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age
            &= 2;
        public void DoSomething()
        {
            int nextAge = Age
                &= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void AndAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public static int _age;
        public int Age { get; set; } = _age &=
            2;
        public void DoSomething()
        {
            int nextAge = Age &=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 31)
                }
            });
    }

    [Fact]
    public void OrAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age |= 2;
        public void DoSomething()
        {
            int nextAge = _age |= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void OrAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age
            |= 2;
        public void DoSomething()
        {
            int nextAge = _age
                |= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void OrAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age |=
            2;
        public void DoSomething()
        {
            int nextAge = _age |=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 32)
                }
            });
    }

    [Fact]
    public void ExclusiveOrAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age ^= 2;
        public void DoSomething()
        {
            int nextAge = Age ^= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ExclusiveOrAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age
            ^= 2;
        public void DoSomething()
        {
            int nextAge = Age
                ^= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ExclusiveOrAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age ^=
            2;
        public void DoSomething()
        {
            int nextAge = Age ^=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 31)
                }
            });
    }

    [Fact]
    public void LeftShiftAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age <<= 2;
        public void DoSomething()
        {
            int nextAge = _age <<= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LeftShiftAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age
            <<= 2;
        public void DoSomething()
        {
            int nextAge = _age
                <<= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LeftShiftAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age <<=
            2;
        public void DoSomething()
        {
            int nextAge = _age <<=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 32)
                }
            });
    }

    [Fact]
    public void RightShiftAssignmentOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age >>= 2;
        public void DoSomething()
        {
            int nextAge = _age >>= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void RightShiftAssignmentOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age
            >>= 2;
        public void DoSomething()
        {
            int nextAge = _age
                >>= 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void RightShiftAssignmentOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        private static int _age;
        public int Age { get; set; } = _age >>=
            2;
        public void DoSomething()
        {
            int nextAge = _age >>=
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 45)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 32)
                }
            });
    }

    [Fact]
    public void LambdaOperatorsOnSameLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Func<int, int> GetAge { get; set; } = i => i;
        public void DoSomething()
        {
            GetAge = (i) => i;
            Func<int> getNextAge = () => 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LambdaOperatorsStartingOnNewLineDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Func<int, int> GetAge { get; set; } = i
            => i;
        public void DoSomething()
        {
            GetAge = (i)
                => i;
            Func<int> getNextAge = ()
                => 1;
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void LambdaOperatorsEndingALineProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Func<int, int> GetAge { get; set; } = i =>
            i;
        public void DoSomething()
        {
            GetAge = (i) =>
                i;
            Func<int> getNextAge = () =>
                1;
        }
    }
}";

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 56)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 26)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3540",
                Message = "Multi-line expressions should not be split after a binary operator",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 12, 39)
                }
            });
    }

    [Fact]
    public void LambdaOperatorsEndingALineFollowedByACodeBlockDoNotProduceDiagnostics()
    {
        string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public Func<int, int> GetAge { get; set; } = i =>
            { return i; };
        public void DoSomething()
        {
            GetAge = (i) =>
                { return i; };
            Func<int> getNextAge = () =>
            {
                return 1;
            };
        }
    }
}";

        VerifyCSharpDiagnostic(code);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new BinaryOperatorLayoutAnalyzer();
    }
}
