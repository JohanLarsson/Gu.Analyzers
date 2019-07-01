namespace Gu.Analyzers.Test.CodeFixes
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using NUnit.Framework;

    internal static class MakeStaticFixTests
    {
        private static readonly CodeFixProvider Fix = new MakeStaticFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0708");

        [Test]
        public static void SimpleMethod()
        {
            var code = @"public static class C
{
    public void M()
    {
    }
}";

            var fixedCode = @"public static class C
{
    public static void M()
    {
    }
}";
            RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void TwoSimpleMethods()
        {
            var code = @"public static class C
{
    public void M1()
    {
    }

    public void M2()
    {
    }
}";

            var fixedCode = @"public static class C
{
    public static void M1()
    {
    }

    public static void M2()
    {
    }
}";
            RoslynAssert.FixAll(Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}