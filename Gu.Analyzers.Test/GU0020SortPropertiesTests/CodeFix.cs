namespace Gu.Analyzers.Test.GU0020SortPropertiesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GU0020SortProperties();
        private static readonly CodeFixProvider Fix = new SortPropertiesFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("GU0020");

        [Test]
        public void MutableBeforeGetOnlySimple()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        ↓public int B { get; set; }

        public int A { get; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int A { get; }

        public int B { get; set; }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void MutableBeforeGetOnlySimpleWithDocs()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary>
        /// B
        /// </summary>
        ↓public int B { get; set; }

        /// <summary>
        /// A
        /// </summary>
        public int A { get; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary>
        /// A
        /// </summary>
        public int A { get; }

        /// <summary>
        /// B
        /// </summary>
        public int B { get; set; }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void ExplicitImplementation()
        {
            var interfaceCode = @"
namespace RoslynSandbox
{
    interface IValue
    {
        object Value { get; }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    public class Foo : IValue
    {
        ↓private int Value { get; } = 5;

        object IValue.Value { get; } = 5;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : IValue
    {
        object IValue.Value { get; } = 5;

        private int Value { get; } = 5;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { interfaceCode, testCode }, fixedCode);
        }

        [Test]
        public void MutableBeforeGetOnlyFirst()
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

        ↓public int A { get; set; }

        public int B { get; }

        public int C { get; }

        public int D { get; }
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

        public int B { get; }

        public int C { get; }

        public int D { get; }

        public int A { get; set; }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void MutableBeforeGetOnlyFirstWithNamespaces()
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

        ↓public int A { get; set; }

        public int B { get; }

        public int C { get; }

        public int D { get; }
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

        public int B { get; }

        public int C { get; }

        public int D { get; }

        public int A { get; set; }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void MutableBeforeGetOnlyLast()
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

        ↓public int C { get; set; }

        public int D { get; }
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

        public int D { get; }

        public int C { get; set; }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void PrivateSetAfterPublicSet()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int c;
        private int d;

        public int A { get; }

        public int B
        {
            get
            {
                return this.A;
            }
        }

        ↓public int C
        {
            get
            {
                return this.c;
            }
            set
            {
                this.c = value;
            }
        }

        public int D
        {
            get
            {
                return this.d;
            }
            private set
            {
                this.d = value;
            }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int c;
        private int d;

        public int A { get; }

        public int B
        {
            get
            {
                return this.A;
            }
        }

        public int D
        {
            get
            {
                return this.d;
            }
            private set
            {
                this.d = value;
            }
        }

        public int C
        {
            get
            {
                return this.c;
            }
            set
            {
                this.c = value;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void MutableBeforeGetOnlyFirstWithInitializers()
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

        ↓public int A { get; set; } = 1;

        public int B { get; } = 2;

        public int C { get; } = 3;

        public int D { get; } = 4;
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

        public int B { get; } = 2;

        public int C { get; } = 3;

        public int D { get; } = 4;

        public int A { get; set; } = 1;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void MutableBeforeGetOnlyWithComments()
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

        /// <summary>
        /// C
        /// </summary>
        ↓public int C { get; set; }

        /// <summary>
        /// D
        /// </summary>
        public int D { get; }
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

        /// <summary>
        /// D
        /// </summary>
        public int D { get; }

        /// <summary>
        /// C
        /// </summary>
        public int C { get; set; }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void ExpressionBodyBeforeGetOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int b)
        {
            this.B = b;
        }

        ↓public int A => B;

        public int B { get; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int b)
        {
            this.B = b;
        }

        public int B { get; }

        public int A => B;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void CalculatedBeforeGetOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int b)
        {
            this.B = b;
        }

        ↓public int A
        {
            get
            {
                return B;
            }
        }

        public int B { get; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int b)
        {
            this.B = b;
        }

        public int B { get; }

        public int A
        {
            get
            {
                return B;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void IndexerBeforeMutable()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class Foo : IReadOnlyList<int>
    {
        public int Count { get; }

        ↓public int this[int index]
        {
            get
            {
                if (index != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, ""message"");
                }

                return A;
            }
        }

        public int A { get; set; }

        public IEnumerator<int> GetEnumerator()
        {
            yield return A;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class Foo : IReadOnlyList<int>
    {
        public int Count { get; }

        public int A { get; set; }

        public int this[int index]
        {
            get
            {
                if (index != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, ""message"");
                }

                return A;
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            yield return A;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void PublicSetBeforePrivateSetFirst()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int a, int b)
        {
            this.A = a;
            this.B = b;
        }

        ↓public int A { get; set; }

        public int B { get; private set; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int a, int b)
        {
            this.A = a;
            this.B = b;
        }

        public int B { get; private set; }

        public int A { get; set; }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void NestedClass()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public sealed class Foo
    {
        public Foo()
        {
        }

        public int Bar
        {
            get { return this.Bar; }
            set { this.Bar = value; }
        }

        public class Nested
        {
            ↓public int Value1 { get; set; }
            
            public int Value2 { get; private set; }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public sealed class Foo
    {
        public Foo()
        {
        }

        public int Bar
        {
            get { return this.Bar; }
            set { this.Bar = value; }
        }

        public class Nested
        {
            public int Value2 { get; private set; }

            public int Value1 { get; set; }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
