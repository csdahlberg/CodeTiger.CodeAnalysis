using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design;

public class NonGenericCollectionDesignAnalyzerTests : DiagnosticVerifier
{
    [Fact]
    public void GenericCollectionsAndPrivateNonGenericStatelessMembersDoNotProduceDiagnostics()
    {
        string code = """
            using System;
            using System.Collections;
            using System.Collections.Generic;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    private List<object> _values = new List<object>();
                    private IList<object> GenericValues => _values;
                    private IList NonGenericValues => _values;
                }
            }
            """;

        VerifyCSharpDiagnostic(code);
    }

    [Fact]
    public void NonGenericCollectionsProduceDiagnostics()
    {
        string code = """
            using System;
            using System.Collections;
            using System.Collections.Generic;
            namespace ClassLibrary1
            {
                public class Thing
                {
                    private IList _values = new ArrayList();
                    public IList NonGenericValues => _values;
                    public ICollection NonGenericCollection { get; set; }
                }
            }
            """;

        VerifyCSharpDiagnostic(code,
            new DiagnosticResult
            {
                Id = "CT1004",
                Message = "Non-generic collections should not be held as state",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 23)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1005",
                Message = "Non-generic collections should not be exposed",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 16)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1005",
                Message = "Non-generic collections should not be exposed",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 16)
                }
            },
            new DiagnosticResult
            {
                Id = "CT1004",
                Message = "Non-generic collections should not be held as state",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 28)
                }
            });
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new NonGenericCollectionDesignAnalyzer();
    }
}
