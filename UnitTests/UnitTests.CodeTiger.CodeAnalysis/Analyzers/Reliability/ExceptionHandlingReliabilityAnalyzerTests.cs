using CodeTiger.CodeAnalysis.Analyzers.Reliability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Reliability;

public class ExceptionHandlingReliabilityAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void TryFinallyDoesNotProduceDiagnostics()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Thing
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
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TryCatchWithCodeInUnfilteredCatchDoesNotProduceDiagnostics()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public void DoSomething()
                    {
                        try
                        {
                        }
                        catch
                        {
                            Console.WriteLine("An exception was thrown.");
                        }
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TryCatchWithCodeInFilteredCatchDoesNotProduceDiagnostics()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public void DoSomething()
                    {
                        try
                        {
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An exception was thrown: {ex}");
                        }
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TryCatchWithCommentInUnfilteredCatchDoesNotProduceDiagnostics()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public void DoSomething()
                    {
                        try
                        {
                        }
                        catch
                        {
                            // Ignored
                        }
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TryCatchWithCommentInFilteredCatchDoesNotProduceDiagnostics()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public void DoSomething()
                    {
                        try
                        {
                        }
                        catch (Exception ex)
                        {
                            // Ignored
                        }
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void TryCatchWithEmptyUnfilteredCatchProducesDiagnostics()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public void DoSomething()
                    {
                        try
                        {
                        }
                        catch
                        {
                        }
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2007",
                Message = "Empty catch blocks should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 13)
                }
            });
    }

    [Fact]
    public void TryCatchWithEmptyFilteredCatchProducesDiagnostics()
    {
        string code = """
            using System;
            namespace ClassLibrary1
            {
                public class Thing
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
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT2007",
                Message = "Empty catch blocks should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 13)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new ExceptionHandlingReliabilityAnalyzer();
    }
}
