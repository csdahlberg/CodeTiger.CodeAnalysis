using System;
using CodeTiger.CodeAnalysis.Analyzers.Naming;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Naming
{
    public class FileNamingAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void FileWithPascalCasedNameAndNoTypeDeclarationsDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
}";

            VerifyCSharpDiagnostic(Tuple.Create("TestFile.cs", code));
        }

        [Fact]
        public void FileWithGeneratedMultiPartPascalCasedNameDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
}";

            VerifyCSharpDiagnostic(Tuple.Create("TestFile.designer.cs", code));
        }

        [Fact]
        public void FileWithGeneratedMultiPartNonPascalCasedNameDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
}";

            VerifyCSharpDiagnostic(Tuple.Create("TESTFILE.designer.cs", code));
        }

        [Fact]
        public void FileWithUppercaseNameProducesDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
}";

            VerifyCSharpDiagnostic(Tuple.Create("TESTFILE.cs", code),
                new DiagnosticResult
                {
                    Id = "CT1701",
                    Message = "Source file names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("TESTFILE.cs", 0, 0)
                    }
                });
        }

        [Fact]
        public void FileWithMultiPartUppercaseNameProducesDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
}";

            VerifyCSharpDiagnostic(Tuple.Create("TESTFILE.SecondPart.cs", code),
                new DiagnosticResult
                {
                    Id = "CT1701",
                    Message = "Source file names should use pascal casing.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("TESTFILE.SecondPart.cs", 0, 0)
                    }
                });
        }

        [Fact]
        public void FileWithMatchingTypeDeclarationDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public partial class TestType
    {
    }
}";

            VerifyCSharpDiagnostic(Tuple.Create("TestType.cs", code));
        }

        [Fact]
        public void FileWithMatchingPartialTypeDeclarationDoesNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public partial class TestType
    {
    }
}";

            VerifyCSharpDiagnostic(Tuple.Create("TestType.PartOne.cs", code));
        }

        [Fact]
        public void FileWithNonMatchingTypeDeclarationProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class TestType
    {
    }
}";

            VerifyCSharpDiagnostic(Tuple.Create("TestFile.cs", code),
                new DiagnosticResult
                {
                    Id = "CT1729",
                    Message = "Source file names should match the primary type name.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("TestFile.cs", 0, 0)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FileNamingAnalyzer();
        }
    }
}
