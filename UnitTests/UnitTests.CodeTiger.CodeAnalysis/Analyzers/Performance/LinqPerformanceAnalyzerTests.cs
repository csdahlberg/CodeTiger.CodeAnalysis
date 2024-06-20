using CodeTiger.CodeAnalysis.Analyzers.Performance;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Performance;

public class LinqPerformanceAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void WhereClauseDoesNotProduceDiagnostic()
    {
        string code = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public IEnumerable<Thing> GetThings(IEnumerable<Thing> things)
                    {
                        return things.Where(x => x != null);
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void WhereClauseFollowedByMethodWithPredicateDoesNotProduceDiagnostic()
    {
        string code = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public Thing GetThing(IEnumerable<Thing> things)
                    {
                        return things.Where(x => x != null).FirstOrDefault(x => x != null);
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void WhereClauseFollowedByMethodWithoutPredicateProducesDiagnostic()
    {
        string code = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    public Thing GetThing(IEnumerable<Thing> things)
                    {
                        return things.Where(x => x != null).Last();
                    }
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1802",
                Message = "Unnecessary where clauses should be simplified",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 27)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new LinqPerformanceAnalyzer();
    }
}
