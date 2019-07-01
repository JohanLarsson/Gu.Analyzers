namespace Gu.Analyzers.Test.GU0050IgnoreEventsWhenSerializingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal static class ValidCode
    {
        private static readonly GU0050IgnoreEventsWhenSerializing Analyzer = new GU0050IgnoreEventsWhenSerializing();

        [Test]
        public static void IgnoredEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    [Serializable]
    public class Foo
    {
        public Foo(int a, int b, int c, int d)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d;
        }

        [field:NonSerialized]
        public event EventHandler SomeEvent;

        public int A { get; }

        public int B { get; protected set;}

        public int C { get; internal set; }

        public int D { get; set; }

        public int E => A;
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void IgnoredEventSimple()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    [Serializable]
    public class Foo
    {
        [field:NonSerialized]
        public event EventHandler SomeEvent;
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void IgnoredEventHandler()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    [Serializable]
    public class Foo
    {
        [NonSerialized]
        private EventHandler someEvent;

        public event EventHandler SomeEvent
        {
            add { this.someEvent += value; }
            remove { this.someEvent -= value; }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void NotSerializable()
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

        public event EventHandler SomeEvent;

        public int A { get; }

        public int B { get; protected set;}

        public int C { get; internal set; }

        public int D { get; set; }

        public int E => A;
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
