﻿namespace Gu.Analyzers.Test.GU0075PreferReturnNullable
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ParameterAnalyzer();
        private static readonly CodeFixProvider Fix = new ReturnNullableFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.GU0075PreferReturnNullable);

        [Test]
        public static void InstanceMethodSingleParameter()
        {
            var before = @"
#nullable enable
namespace N
{
    class C
    {
        bool M(↓out string? s)
        {
            if (nameof(C).Length > 1)
            {
                s = string.Empty;
                return true;
            }

            s = null;
            return false;
        }
    }
}";

            var after = @"
#nullable enable
namespace N
{
    class C
    {
        string? M()
        {
            if (nameof(C).Length > 1)
            {
                return string.Empty;
            }

            return null;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void InstanceMethodSingleGenericParameter()
        {
            var before = @"
#nullable enable
namespace N
{
    class C
    {
        bool M<T>(↓out T s)
            where T : class, new()
        {
            if (nameof(C).Length > 1)
            {
                s = new T();
                return true;
            }

            s = null;
            return false;
        }
    }
}";

            var after = @"
#nullable enable
namespace N
{
    class C
    {
        T? M<T>()
            where T : class, new()
        {
            if (nameof(C).Length > 1)
            {
                return new T();
            }

            return null;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void InstanceMethodTwoParameters()
        {
            var before = @"
#nullable enable
namespace N
{
    class C
    {
        bool M(string text, ↓out string? s)
        {
            if (text.Length > 1)
            {
                s = string.Empty;
                return true;
            }

            s = null;
            return false;
        }
    }
}";

            var after = @"
#nullable enable
namespace N
{
    class C
    {
        string? M(string text)
        {
            if (text.Length > 1)
            {
                return string.Empty;
            }

            return null;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Return nullable");
        }

        [Ignore("tbd")]
        [TestCase("s != null")]
        [TestCase("s is { }")]
        [TestCase("s is object")]
        public static void ReturnNotNull(string expression)
        {
            var before = @"
#nullable enable
namespace N
{
    class C
    {
        bool M1(↓out string? s)
        {
            M2(out s);
            return s != null;
        }

        void M2(out string? s)
        {
            if (nameof(C).Length > 1)
            {
                s = string.Empty;
                return;
            }

            s = null;
            return;
        }
    }
}".AssertReplace("s != null", expression);

            var after = @"
#nullable enable
namespace N
{
    class C
    {
        bool M1(↓out string? s)
        {
            M2(out s);
            return s;
        }

        void M2(out string? s)
        {
            if (nameof(C).Length > 1)
            {
                s = string.Empty;
                return;
            }

            s = null;
            return;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Return nullable");
        }

        [Test]
        public static void ReturnAssignedViaOut()
        {
            var before = @"
#nullable enable
namespace N
{
    class C
    {
        bool M1(↓out string? s)
        {
            return M2(out s);
        }

#pragma warning disable GU0075
        bool M2(out string? s)
        {
            if (nameof(C).Length > 1)
            {
                s = string.Empty;
                return true;
            }

            s = null;
            return false;
        }
    }
}";

            var after = @"
#nullable enable
namespace N
{
    class C
    {
        string? M1()
        {
            return M2(out s) ? s : null;
        }

#pragma warning disable GU0075
        bool M2(out string? s)
        {
            if (nameof(C).Length > 1)
            {
                s = string.Empty;
                return true;
            }

            s = null;
            return false;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "UNSAFE Return nullable", AllowCompilationErrors.Yes);
        }

        [TestCase("s = null")]
        [TestCase("s = default")]
        public static void AssignedNullInFirstStatement(string expression)
        {
            var before = @"
#nullable enable
namespace N
{
    class C
    {
        bool M1(bool b, ↓out string? s)
        {
            s = null;
            return b &&
                   M2(out s);
        }

#pragma warning disable GU0075
        bool M2(out string? s)
        {
            if (nameof(C).Length > 1)
            {
                s = string.Empty;
                return true;
            }

            s = null;
            return false;
        }
    }
}".AssertReplace("s = null", expression);

            var after = @"
#nullable enable
namespace N
{
    class C
    {
        string? M1(bool b)
        {
            return b &&
                   M2(out s) ? s : null;
        }

#pragma warning disable GU0075
        bool M2(out string? s)
        {
            if (nameof(C).Length > 1)
            {
                s = string.Empty;
                return true;
            }

            s = null;
            return false;
        }
    }
}".AssertReplace("s = null", expression);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "UNSAFE Return nullable", AllowCompilationErrors.Yes);
        }

        [Test]
        public static void LocalFunction()
        {
            var before = @"
#nullable enable
namespace N
{
    class C
    {
        void Outer()
        {
            bool M(↓out string? s)
            {
                if (nameof(C).Length > 1)
                {
                    s = string.Empty;
                    return true;
                }

                s = null;
                return false;
            }
        }
    }
}";

            var after = @"
#nullable enable
namespace N
{
    class C
    {
        void Outer()
        {
            string? M()
            {
                if (nameof(C).Length > 1)
                {
                    return string.Empty;
                }

                return null;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Return nullable");
        }
    }
}
