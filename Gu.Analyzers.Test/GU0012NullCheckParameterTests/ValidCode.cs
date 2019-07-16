namespace Gu.Analyzers.Test.GU0012NullCheckParameterTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [TestFixture(typeof(SimpleAssignmentAnalyzer))]
    [TestFixture(typeof(ParameterAnalyzer))]
    internal static class ValidCode<T>
        where T : DiagnosticAnalyzer, new()
    {
        private static readonly T Analyzer = new T();

        [Test]
        public static void WhenPrivate()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        private readonly string text;

        private C(string text)
        {
            this.text = text;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenDefaultValue()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        private readonly string text;

        public C(string text = null)
        {
            this.text = text;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenValueType()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        private readonly int i;

        public C(int i)
        {
            this.i = i;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenOutParameter()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public bool M(out string text)
        {
            text = string.Empty;
            return true;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenThrowing()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        private readonly string text;

        public C(string text)
        {
            this.text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenThrowingOnLineAbove()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        private readonly string text;

        public C(string text)
        {
#pragma warning disable GU0015 // Don't assign same more than once.
            this.text = text ?? throw new ArgumentNullException(nameof(text));
            this.text = text;
#pragma warning restore GU0015 // Don't assign same more than once.
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("text == null")]
        [TestCase("text is null")]
        public static void WhenOldStyleNullCheckAbove(string check)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        private readonly string text;

        public C(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            this.text = text;
        }
    }
}".AssertReplace("text == null", check);

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
