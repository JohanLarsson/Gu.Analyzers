namespace Gu.Analyzers.Test.GU0001NameArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal static class ValidCode
    {
        private static readonly ArgumentListAnalyzer Analyzer = new ArgumentListAnalyzer();

        [TestCase("new Foo(a, b)")]
        [TestCase("new Foo(a: a, b: b)")]
        public static void ConstructorCallWithTwoArguments(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int a, int b)
        {
            this.A = a;
            this.B = b;
        }

        public int A { get; }

        public int B { get; }

        private Foo Create(int a, int b)
        {
            return new Foo(a, b);
        }
    }
}".AssertReplace("new Foo(a, b)", call);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("new Foo(a, b)")]
        [TestCase("new Foo(a: a, b: b)")]
        public static void ConstructorCallWithTwoArgumentsStruct(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    public struct Foo
    {
        public Foo(int a, int b)
        {
            this.A = a;
            this.B = b;
        }

        public int A { get; }

        public int B { get; }

        private Foo Create(int a, int b)
        {
            return new Foo(a, b);
        }
    }
}".AssertReplace("new Foo(a, b)", call);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ConstructorCallWithNamedArgumentsOnSameRow()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int a, int b, int c, int d)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d;
        }

        public int A { get; }

        public int B { get; }

        public int C { get; }

        public int D { get; }

        private Foo Create(int a, int b, int c, int d)
        {
            return new Foo(a: a, b: b, c: c, d: d);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ConstructorCallWithArgumentsOnSameRow()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int a, int b, int c, int d)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d;
        }

        public int A { get; }

        public int B { get; }

        public int C { get; }

        public int D { get; }

        private Foo Create(int a, int b, int c, int d)
        {
            return new Foo(a, b, c, d);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ConstructorCallWithNamedArgumentsOnSeparateRows()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int a, int b, int c, int d)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d;
        }

        public int A { get; }

        public int B { get; }

        public int C { get; }

        public int D { get; }

        private Foo Create(int a, int b, int c, int d)
        {
            return new Foo(
                a: a, 
                b: b, 
                c: c, 
                d: d);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresStringFormat()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Globalization;

    public static class Foo
    {
        private static string Bar(int a, int b, int c, int d)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                ""{0}{1}{2}{3}"",
                a,
                b,
                c,
                d);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresTuple()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public static class Foo
    {
        private static Tuple<int,int,int,int> Bar(int a, int b, int c, int d)
        {
            return Tuple.Create(
                a,
                b,
                c,
                d);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ImmutableArrayCreate()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Collections.Immutable;

    public class Foo
    {
        public Foo()
        {
            var ints = ImmutableArray.Create(
                1,
                2,
                3,
                4);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresParams()
        {
            var code = @"
namespace RoslynSandbox
{
    public static class Foo
    {
        public static void Bar(params int[] args)
        {
        }

        public static void Meh()
        {
            Bar(
                1,
                2,
                3,
                4,
                5,
                6);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresWhenDifferentTypes()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int a, double b, string c, int d)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d;
        }

        public int A { get; }

        public double B { get; }

        public string C { get; }

        public int D { get; }

        private Foo Create(int a, double b, string c, int d)
        {
            return new Foo(
                a, 
                b, 
                c, 
                d);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresWhenInExpressionTree()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Linq.Expressions;

    public class Foo
    {
        public Foo(int a, int b, int c, int d)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d;
        }

        public int A { get; }

        public int B { get; }

        public int C { get; }

        public int D { get; }

        private Expression<Func<Foo>> Create(int a, int b, int c, int d)
        {
            return () => new Foo(
                a,
                b,
                c,
                d);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
