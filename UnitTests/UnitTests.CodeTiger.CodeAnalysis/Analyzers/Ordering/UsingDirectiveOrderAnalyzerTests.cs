using CodeTiger.CodeAnalysis.Analyzers.Ordering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Ordering
{
    public class UsingDirectiveOrderAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ValidUsingDirectivesDoNotProduceDiagnostics()
        {
            string code = @"using System;
using System.Collections;
using System.Collections.Generic;
#if !PORTABLE
using System.Reflection;
#else
using System.Runtime;
#endif
using Microsoft.Win32;
using static System.Threading.Tasks.Task;";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void UsingDirectivesWithNonSystemDirectivesFirstProduceDiagnostics()
        {
            string code = @"using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
#if !PORTABLE
using System.Reflection;
#else
using System.Runtime;
#endif";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3211",
                    Message = "Using directives for System namespaces should be before other namespaces.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 2, 1)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3211",
                    Message = "Using directives for System namespaces should be before other namespaces.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 3, 1)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3211",
                    Message = "Using directives for System namespaces should be before other namespaces.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 1)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3211",
                    Message = "Using directives for System namespaces should be before other namespaces.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 1)
                    }
                }
            );
        }

        [Fact]
        public void UsingDirectivesNotSortedAlphabeticallyProduceDiagnostics()
        {
            string code = @"using System;
using System.Collections;
#if !PORTABLE
using System.Reflection;
#else
using System.Runtime;
#endif
using System.Collections.Generic;
using Microsoft.Win32;";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3213",
                    Message = "Using directives should be ordered alphabetically.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 1)
                    }
                }
            );
        }

        [Fact]
        public void UsingDirectivesSeparatedByBlankLinesProduceDiagnostics()
        {
            string code = @"using System;
using System.Collections;

using System.Collections.Generic;

#if !PORTABLE
using System.Reflection;
#else
using System.Runtime;
#endif

using Microsoft.Win32;";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3214",
                    Message = "Using directives should not be separated by any lines.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 1)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3214",
                    Message = "Using directives should not be separated by any lines.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 7, 1)
                    }
                },
                new DiagnosticResult
                {
                    Id = "CT3214",
                    Message = "Using directives should not be separated by any lines.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 12, 1)
                    }
                }
            );
        }

        [Fact]
        public void NonStaticUsingDirectivesAfterStaticUsingDirectiveProduceDiagnostics()
        {
            string code = @"using System;
using System.Collections;
using System.Collections.Generic;
#if !PORTABLE
using System.Reflection;
#else
using System.Runtime;
#endif
using static System.Threading.Tasks.Task;
using Microsoft.Win32;";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT3218",
                    Message = "Non-static using directives should be before static using directives.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 10, 1)
                    }
                }
            );
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UsingDirectiveOrderAnalyzer();
        }
    }
}
