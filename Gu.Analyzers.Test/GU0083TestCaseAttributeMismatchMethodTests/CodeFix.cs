namespace Gu.Analyzers.Test.GU0083TestCaseAttributeMismatchMethodTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using NUnit.Framework;

    internal static class CodeFix
    {
        private static readonly TestMethodAnalyzer Analyzer = new TestMethodAnalyzer();
        private static readonly CodeFixProvider Fix = new TestMethodParametersFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(GU0083TestCaseAttributeMismatchMethod.Descriptor);

        [Test]
        public static void SingleArgument()
        {
            var before = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(↓1)]
        public void Test(string str)
        {
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(1)]
        public void Test(int str)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("[TestCase(\"a\", ↓1, null)]")]
        [TestCase("[TestCase(null, \"a\", ↓1)]")]
        [TestCase("[TestCase(↓1, null, \"b\")]")]
        [TestCase("[TestCase(null, null, ↓1)]")]
        public static void NullArgument(string testCase)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(""x"", ""y"", null)]
        public void Test(string x, string y, string z)
        {
        }
    }
}".AssertReplace("[TestCase(\"x\", \"y\", null)]", testCase);

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void TestCaseAttribute_IfMultipleParametersAreWrong()
        {
            var before = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(1, ↓2)]
        public void Test(int i, string str)
        {
        }
    }
}";
            var after = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(1, 2)]
        public void Test(int i, int str)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ArgumentIsNullAndParameterIsInt()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(↓null)]
        public void Test(int obj)
        {
        }
    }
}";
            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void WrongArrayType()
        {
            var before = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(↓new double[] {3, 5})]
        public void Test(int[] array)
        {
        }
    }
}";
            var after = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(new double[] {3, 5})]
        public void Test(double[] array)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DoubleToInt()
        {
            var before = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(↓1.0)]
        public void Test(int i)
        {
        }
    }
}";
            var after = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class FooTests
    {
        [TestCase(1.0)]
        public void Test(double i)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void TestCaseParams()
        {
            var before = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    class Foo
    {
        [TestCase(1, 2, ↓3.0)]
        public void Test(int i, int j, params int[] ints)
        {
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    class Foo
    {
        [TestCase(1, 2, 3.0)]
        public void Test(int i, int j, params double[] ints)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
