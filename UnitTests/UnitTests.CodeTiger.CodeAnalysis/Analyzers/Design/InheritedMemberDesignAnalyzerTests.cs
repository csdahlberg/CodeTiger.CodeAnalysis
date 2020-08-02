using CodeTiger.CodeAnalysis.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers.Design
{
    public class InheritedMemberDesignAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void ClassesWithBaseClassAndWithStaticMethodDoNotProduceDiagnostics()
        {
            string code = @"using System;
using System.Threading.Tasks;
namespace ClassLibrary1
{
    public class Class1
    {
        public object GetValue() { return new object(); }
        public Class1 Self { get { return this; } }
    }
    public class Class2 : Class1
    {
        public new int GetValue() { return 1; }
        public new Class2 Self { get { return this; } }
    }
    public class Class3<T> : Class1 where T : IDisposable
    {
        public new T GetValue() { return default(T); }
        public new Class3<T> Self { get { return this; } }
    }
    public class Class4 : Class3<IDisposable>
    {
        public new Task GetValue() { return default(Task); }
        public new Class4 Self { get { return this; } }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ClassesWithHiddenBaseMethodsProducesDiagnostic()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class Class1
    {
        public int GetValue() { return 1; }
        public Class1 Self { get { return this; } }
    }
    public class Class2 : Class1
    {
        public new int GetValue() { return 2; }
        public new string Self { get { return ""blah""; } }
    }
    public class Class3<T> : Class1 where T : IDisposable
    {
        public new int GetValue() { return 3; }
        public new string Self { get { return ""blah""; } }
    }
    public class Class4 : Class3<IDisposable>
    {
        public new IDisposable GetValue() { return default(IDisposable); }
        public new Class4 Self { get { return this; } }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 24) }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 27) }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 24) }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 17, 27) }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 21, 32) }
                },
                new DiagnosticResult
                {
                    Id = "CT1002",
                    Message
                        = "Members of base types should not be hidden except to return more specialized types.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 22, 27) }
                });
        }

        [Fact]
        public void ParametersThatMatchDefaultValuesOfBaseInterfaceDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IThing
    {
        void SetValue(bool flag, int i = 0, string str = default);
    }
    public class ThingClass : IThing
    {
        public void SetValue(bool flag, int i = 0, string str = default) { }
        public virtual void SetValue(bool flag, string str = default) { }
    }
    public class ThingClass2 : ThingClass
    {
        public override void SetValue(bool flag, string str = default) { }
    }
    public struct ThingStruct : IThing
    {
        public void SetValue(bool flag, int i = 0, string str = default) { }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ParametersWithDefaultValuesProduceDiagnosticsWhenBaseInterfaceDoesNotHaveDefaultValues()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IThing
    {
        void SetValue(bool flag, int i, string str);
    }
    public class ThingClass : IThing
    {
        public void SetValue(bool flag = true, int i = 1, string str = """") { }
    }
    public struct ThingStruct : IThing
    {
        public void SetValue(bool flag = false, int i = 0, string str = default) { }
    }
    public abstract class OtherThingClass : IThing
    {
        public abstract void SetValue(bool flag = true, int i = 1, string str = """");
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 40) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 54) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 70) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 40) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 55) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 71) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 49) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 63) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 79) },
                });
        }

        [Fact]
        public void ParametersWithoutDefaultValuesProduceDiagnosticsWhenBaseInterfaceHasDefaultValues()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IThing
    {
        void SetValue(bool flag = true, int i = -1, string str = """");
    }
    public class ThingClass : IThing
    {
        public void SetValue(bool flag, int i, string str) { }
    }
    public struct ThingStruct : IThing
    {
        public void SetValue(bool flag, int i, string str) { }
    }
    public abstract class OtherThingClass : IThing
    {
        public abstract void SetValue(bool flag, int i, string str);
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 35) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 45) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 55) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 35) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 45) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 55) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 44) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 54) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 64) },
                });
        }

        [Fact]
        public void ParametersWithDefaultValuesThatDoNotMatchBaseInterfaceProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IThing
    {
        void SetValue(bool flag = true, int i = -1, string str = """");
    }
    public class ThingClass : IThing
    {
        public void SetValue(bool flag = false, int i = 1, string str = null) { }
    }
    public struct ThingStruct : IThing
    {
        public void SetValue(bool flag = false, int i = int.MaxValue, string str = string.Empty) { }
    }
    public abstract class OtherThingClass : IThing
    {
        public abstract void SetValue(bool flag = false, int i = 4, string str = ""foo"");
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 40) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 55) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 71) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 40) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 55) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 82) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 49) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 64) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 80) },
                });
        }

        [Fact]
        public void ParametersThatMatchDefaultValuesOfBaseTypeDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class ThingClass : IThing
    {
        public abstract void SetValue(bool flag, int i = 0, string str = default);
        public virtual void SetValue(bool flag, string str = default) { }
    }
    public class ThingClass2 : ThingClass
    {
        public override void SetValue(bool flag, int i = 0, string str = default) { }
        public override void SetValue(bool flag, string str = default) { }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ParametersWithDefaultValuesProduceDiagnosticsWhenBaseTypeDoesNotHaveDefaultValues()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class ThingClass : IThing
    {
        public abstract void SetValue(bool flag, int i, string str);
        public virtual void SetValue(bool flag, string str) { }
    }
    public class ThingClass2 : ThingClass
    {
        public override void SetValue(bool flag = true, int i = 0, string str = default) { }
        public override void SetValue(bool flag = false, string str = """") { }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 49) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 63) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 79) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 49) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 69) },
                });
        }

        [Fact]
        public void ParametersWithoutDefaultValuesProduceDiagnosticsWhenBaseTypeHasDefaultValues()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class ThingClass : IThing
    {
        public abstract void SetValue(bool flag = true, int i = 0, string str = default);
        public virtual void SetValue(bool flag = false, string str = """") { }
    }
    public class ThingClass2 : ThingClass
    {
        public override void SetValue(bool flag, int i, string str) { }
        public override void SetValue(bool flag, string str) { }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 44) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 54) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 64) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 44) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 57) },
                });
        }

        [Fact]
        public void ParametersWithDefaultValuesThatDoNotMatchBaseTypeProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class ThingClass : IThing
    {
        public abstract void SetValue(bool flag = true, int i = 0, string str = default);
        public virtual void SetValue(bool flag = false, string str = ""foo"") { }
    }
    public class ThingClass2 : ThingClass
    {
        public override void SetValue(bool flag = false, int i = 1, string str = """") { }
        public override void SetValue(bool flag = true, string str = default) { }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 49) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 64) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 80) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 49) },
                },
                new DiagnosticResult
                {
                    Id = "CT1011",
                    Message = "Default values of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 68) },
                });
        }

        [Fact]
        public void ParametersWithParamsKeywordThatMatchBaseInterfaceDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IThing
    {
        void SetValue(params int[] values);
        void SetValue(string[] values);
    }
    public class ThingClass : IThing
    {
        public void SetValue(params int[] values) { }
        public virtual void SetValue(string[] values) { }
    }
    public struct ThingStruct : IThing
    {
        public void SetValue(params int[] values) { }
        public void SetValue(string[] values) { }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ParametersWithParamsKeywordThatDoesNotMatchBaseInterfaceProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public interface IThing
    {
        void SetValue(params int[] values);
        void SetValue(string[] values);
    }
    public class ThingClass : IThing
    {
        public void SetValue(int[] values) { }
        public virtual void SetValue(params string[] values) { }
    }
    public struct ThingStruct : IThing
    {
        public void SetValue(int[] values) { }
        public void SetValue(params string[] values) { }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1019",
                    Message = "Params modifier of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 36) },
                },
                new DiagnosticResult
                {
                    Id = "CT1019",
                    Message = "Params modifier of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 54) },
                },
                new DiagnosticResult
                {
                    Id = "CT1019",
                    Message = "Params modifier of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 36) },
                },
                new DiagnosticResult
                {
                    Id = "CT1019",
                    Message = "Params modifier of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 17, 46) },
                });
        }

        [Fact]
        public void ParametersWithParamsKeywordThatMatchBaseTypeDoNotProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class ThingClass
    {
        public virtual void SetValue(params int[] values) { }
        public virtual void SetValue(string[] values) { }
    }
    public class ThingClass2 : ThingClass
    {
        public override void SetValue(params int[] values) { }
        public override void SetValue(string[] values) { }
    }
}";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void ParametersWithParamsKeywordThatDoesNotMatchBaseTypeProduceDiagnostics()
        {
            string code = @"using System;
namespace ClassLibrary1
{
    public class ThingClass
    {
        public virtual void SetValue(params int[] values) { }
        public virtual void SetValue(string[] values) { }
    }
    public class ThingClass2 : ThingClass
    {
        public override void SetValue(int[] values) { }
        public override void SetValue(params string[] values) { }
    }
}";

            VerifyCSharpDiagnostic(code,
                new DiagnosticResult
                {
                    Id = "CT1019",
                    Message = "Params modifier of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 45) },
                },
                new DiagnosticResult
                {
                    Id = "CT1019",
                    Message = "Params modifier of parameters should match any base definitions.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 55) },
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new InheritedMemberDesignAnalyzer();
        }
    }
}
