namespace Gu.Analyzers.Test.GU0002NamedArgumentPositionMatchesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal static class CodeFix
    {
        private static readonly ArgumentListAnalyzer Analyzer = new ArgumentListAnalyzer();
        private static readonly MoveArgumentFix Fix = new MoveArgumentFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(GU0002NamedArgumentPositionMatches.Descriptor);

        [Test]
        public static void Message()
        {
            var testCode = @"
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
            return new Foo↓(
                b: b,
                a: a,
                c: c,
                d: d);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("The position of a named arguments and parameters should match."), testCode);
        }

        [Test]
        public static void Constructor()
        {
            var testCode = @"
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
            return new Foo↓(
                b: b,
                a: a,
                c: c,
                d: d);
        }
    }
}";

            var fixedCode = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void ConstructorInArrayInitializer()
        {
            var testCode = @"
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

        private Foo[] Create(int a, int b, int c, int d)
        {
            return new[]
                       {
                            new Foo↓(
                                b: b,
                                a: a,
                                c: c,
                                d: d)
                       };
        }
    }
}";

            var fixedCode = @"
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

        private Foo[] Create(int a, int b, int c, int d)
        {
            return new[]
                       {
                            new Foo(
                                a: a,
                                b: b,
                                c: c,
                                d: d)
                       };
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void ConstructorInFunc()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

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

        private Func<Foo> Create(int a, int b, int c, int d)
        {
            return () => new Foo↓(
                b: b,
                a: a,
                c: c,
                d: d);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

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

        private Func<Foo> Create(int a, int b, int c, int d)
        {
            return () => new Foo(
                a: a,
                b: b,
                c: c,
                d: d);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void ConstructorIgnoredIfNonWhitespaceTrivia()
        {
            var testCode = @"
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
            return new Foo↓(
                b: b, // some comment
                a: a,
                c: c,
                d: d);
        }
    }
}";
            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, testCode);
        }
    }
}
