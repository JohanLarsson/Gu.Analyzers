namespace Gu.Analyzers.Test.GU0072AllTypesShouldBeInternalTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal static class CodeFix
    {
        private static readonly GU0072AllTypesShouldBeInternal Analyzer = new GU0072AllTypesShouldBeInternal();
        private static readonly MakeInternalFix Fix = new MakeInternalFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(GU0072AllTypesShouldBeInternal.Descriptor);

        [Test]
        public static void Class()
        {
            var before = @"
namespace N
{
    ↓public class Foo
    {
    }
}";

            var after = @"
namespace N
{
    internal class Foo
    {
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void Struct()
        {
            var before = @"
namespace N
{
    ↓public struct Foo
    {
    }
}";

            var after = @"
namespace N
{
    internal struct Foo
    {
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
