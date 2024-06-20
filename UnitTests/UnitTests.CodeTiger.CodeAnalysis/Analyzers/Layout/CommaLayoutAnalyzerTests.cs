using CodeTiger.CodeAnalysis.Analyzers.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Layout;

public class CommaLayoutAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void ParameterListsWithCommasOnSameLineDoNotProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    public Class1(int value)
                    {
                    }
                    public Class1(int first, int second)
                        : this(Add(first, second))
                    {
                    }
                    private static int Add(int first, int second)
                    {
                        return first + second;
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ParameterListsWithCommasOnNewLinesProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    public Class1(int value)
                    {
                    }
                    public Class1(int first
                        , int second)
                        : this(Add(first, second))
                    {
                    }
                    private static int Add(int first
                        , int second)
                    {
                        return first + second;
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 13)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 15, 13)
                }
            });
    }

    [Fact]
    public void ArgumentListsWithCommasOnSameLineDoNotProduceDiagnostic()
    {
        string code = """
            using System;
            using System.Globalization;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    public void DoSomething()
                    {
                        var time = new DateTime(2010, 1, 13);
                        string timeText = time.ToString("D", CultureInfo.InvariantCulture);
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void ArgumentListsWithCommasOnNewLinesProduceDiagnostic()
    {
        string code = """
            using System;
            using System.Globalization;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    public void DoSomething()
                    {
                        var time = new DateTime(2010, 1
                            , 13);
                        string timeText = time.ToString("D"
                            , CultureInfo.InvariantCulture);
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 12, 17)
                }
            });
    }

    [Fact]
    public void TypeParameterListsWithCommasOnSameLineDoNotProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Class1<TFirst, TSecond,
                    TThird>
                {
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TypeParameterListsWithCommasOnNewLinesProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Class1<TFirst
                    , TSecond
                    , TThird>
                {
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 5, 9)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 9)
                }
            });
    }

    [Fact]
    public void TypeParameterConstraintClausesWithCommasOnSameLineDoNotProduceDiagnostic()
    {
        string code = """
            using System;
            using System.Collections.Generic;
            namespace ClassLibrary1
            {
                public class Class1<TFirst, TSecond, TThird>
                    where TFirst : IList<string>, new()
                    where TThird : TFirst, TSecond
                {
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TypeParameterConstraintClausesWithCommasOnNewLinesProduceDiagnostic()
    {
        string code = """
            using System;
            using System.Collections.Generic;
            namespace ClassLibrary1
            {
                public class Class1<TFirst, TSecond, TThird>
                    where TFirst : IList<string>
                        , new()
                    where TThird : TFirst
                        , TSecond
                {
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 13)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 13)
                }
            });
    }

    [Fact]
    public void TypeArgumentListsWithCommasOnSameLineDoNotProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    Tuple<int, string, object> values = Tuple.Create<int, string, object>(1, "", null);
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TypeArgumentListsWithCommasOnNewLinesProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    Tuple<int, string
                        , object> values = Tuple.Create<int
                        , string
                        , object>(1, "", null);
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 13)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 13)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 13)
                }
            });
    }

    [Fact]
    public void EnumMemberDeclarationsWithCommasOnSameLineDoNotProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public enum Enum1
                {
                    FirstValue,
                    SecondValue,
                    ThirdValue,
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void EnumMemberDeclarationsWithCommasOnNewLinesProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public enum Enum1
                {
                    FirstValue
                    ,
                    SecondValue
                    , ThirdValue
                    ,
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 9)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 9)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 9)
                }
            });
    }

    [Fact]
    public void VariableDeclarationsWithCommasOnSameLineDoNotProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    public void DoSomething()
                    {
                        int x = 1, y = 2, z = 3;
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void VariableDeclarationsWithCommasOnNewLinesProduceDiagnostic()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    public void DoSomething()
                    {
                        int x = 1
                            , y = 2
                            , z = 3;
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 17)
                }
            });
    }

    [Fact]
    public void InitializerExpressionsWithCommasOnSameLineDoNotProduceDiagnostic()
    {
        string code = """
            using System;
            using System.Collections.Generic;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    public void DoSomething()
                    {
                        int[] numberArray = new[] { 1, 2, 3 };
                        numberArray = new[]
                        {
                            1,
                            2,
                            3,
                        };
                        var numberList = new List<int> { 1, 2, 3 };
                        numberList = new List<int>
                        {
                            1,
                            2,
                            3,
                        };
                        var numberObject = new
                        {
                            One = 1,
                            Two = 2,
                            Three = 3,
                        };
                        var numberDictionary = new Dictionary<string, int>
                        {
                            { "One", 1 }, { "Two",2 },{ "Three",3 }
                        };
                        numberDictionary = new Dictionary<string, int>
                        {
                            {
                                "One",
                                1
                            },
                            {
                                "Two",
                                2
                            },
                            {
                                "Three",
                                3
                            },
                        };
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void InitializerExpressionsWithCommasOnNewLinesProduceDiagnostic()
    {
        string code = """
            using System;
            using System.Collections.Generic;
            namespace ClassLibrary1
            {
                public class Class1
                {
                    public void DoSomething()
                    {
                        int[] numberArray = new[] { 1
                            , 2
                            ,3 };
                        numberArray = new[]
                        {
                            1
                            , 2
                            ,3
                            ,
                        };
                        var numberList = new List<int> { 1
                            , 2
                            ,3 };
                        numberList = new List<int>
                        {
                            1
                            , 2
                            ,3
                            ,
                        };
                        var numberObject = new
                        {
                            One = 1
                            , Two = 2
                            ,Three = 3
                            ,
                        };
                        var numberDictionary = new Dictionary<string, int>
                        {
                            { "One"
                                , 1 }
                            , { "Two"
                                , 2 }
                            ,{ "Three"
                                , 3 }
                            ,
                        };
                        numberDictionary = new Dictionary<string, int>
                        {
                            {
                                "One"
                                ,1
                            }
                            ,{
                                "Two"
                                ,2
                            }
                            ,{
                                "Three"
                                ,3
                            }
                            ,
                        };
                    }
                }
            }
            """; 

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 15, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 16, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 17, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 20, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 21, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 25, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 26, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 27, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 32, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 33, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 34, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 39, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 40, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 41, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 42, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 43, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 44, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 50, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 52, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 54, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 56, 17)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 58, 21)
                }
            },
            new DiagnosticResult
            {
                Id = "CT3537",
                Message = "Commas should be on the same line as the preceding element",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 60, 17)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new CommaLayoutAnalyzer();
    }
}
