namespace Gu.Analyzers.Test.GU0004AssignAllReadOnlyMembersTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal static class Diagnostics
    {
        private static readonly ConstructorAnalyzer Analyzer = new ConstructorAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(GU0004AssignAllReadOnlyMembers.Descriptor);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
{
    public class Foo
    {
        public ↓Foo(int a)
        {
            this.A = a;
        }

        public int A { get; }

        public int B { get; }
    }
}";

            var message = "The following readonly members are not assigned:\r\n" +
                          "N.Foo.B";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public static void NotSettingGetOnlyProperty()
        {
            var code = @"
namespace N
{
    public class Foo
    {
        public ↓Foo(int a)
        {
            this.A = a;
        }

        public int A { get; }

        public int B { get; }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void NotSettingGetOnlyPropertyInOneCtor()
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Windows.Markup;

    public class DisplayPropertyNameExtension : MarkupExtension
    {
        public ↓DisplayPropertyNameExtension()
        {
        }

        public DisplayPropertyNameExtension(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        public Type Type { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // (This code has zero tolerance)
            var prop = Type.GetProperty(PropertyName);
            var attributes = prop.GetCustomAttributes(typeof(DisplayNameAttribute), inherit: false);
            return (attributes[0] as DisplayNameAttribute)?.DisplayName;
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void NotSettingReadOnlyField()
        {
            var code = @"
namespace N
{
    public class Foo
    {
        private readonly int a;
        private readonly int b;

        public ↓Foo(int a)
        {
            this.a = a;
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void StaticConstructorSettingProperties()
        {
            var code = @"
namespace N
{
    public class Foo
    {
        static ↓Foo()
        {
            A = 1;
        }

        public static int A { get; }

        public static int B { get; }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void StaticConstructorNotSettingField()
        {
            var code = @"
namespace N
{
    public class Foo
    {
        public static readonly int A;

        public static readonly int B;

        static ↓Foo()
        {
            A = 1;
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
