﻿namespace Gu.Analyzers.Test.GU0084AssertExceptionMessageTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AssertAnalyzer();
        private static readonly CodeFixProvider Fix = new AssertFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.GU0084AssertExceptionMessage);

        [Test]
        public static void ExplicitDiscard()
        {
            var before = @"
namespace N
{
    using NUnit.Framework;

    public class C
    {
        [Test]
        public void M()
        {
            _ = ↓Assert.Throws<SuccessException>(() => { });
        }
    }
}";

            var after = @"
namespace N
{
    using NUnit.Framework;

    public class C
    {
        [Test]
        public void M()
        {
            var exception = Assert.Throws<SuccessException>(() => { });
            Assert.AreEqual(""EXPECTED"", exception.Message);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Assert exception message via local variable.");
        }

        [Test]
        public static void AssertThrowsExplicitDiscardToInline()
        {
            var before = @"
namespace N
{
    using NUnit.Framework;

    public class C
    {
        [Test]
        public void M()
        {
            _ = ↓Assert.Throws<SuccessException>(() => { });
        }
    }
}";

            var after = @"
namespace N
{
    using NUnit.Framework;

    public class C
    {
        [Test]
        public void M()
        {
            Assert.AreEqual(""EXPECTED"", Assert.Throws<SuccessException>(() => { }).Message);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Assert exception message inline.");
        }

        [Test]
        public static void AssertThrowsAsyncExplicitDiscardToInline()
        {
            var before = @"
namespace N
{
    using NUnit.Framework;

    public class C
    {
        [Test]
        public void M()
        {
            _ = ↓Assert.ThrowsAsync<SuccessException>(async () => { });
        }
    }
}";

            var after = @"
namespace N
{
    using NUnit.Framework;

    public class C
    {
        [Test]
        public void M()
        {
            Assert.AreEqual(""EXPECTED"", Assert.ThrowsAsync<SuccessException>(async () => { }).Message);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Assert exception message inline.");
        }

        [Test]
        public static void ImplicitDiscard()
        {
            var before = @"
namespace N
{
    using NUnit.Framework;

    public class C
    {
        [Test]
        public void M()
        {
            ↓Assert.Throws<SuccessException>(() => { });
        }
    }
}";

            var after = @"
namespace N
{
    using NUnit.Framework;

    public class C
    {
        [Test]
        public void M()
        {
            var exception = Assert.Throws<SuccessException>(() => { });
            Assert.AreEqual(""EXPECTED"", exception.Message);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Assert exception message via local variable.");
        }
    }
}
