namespace Gu.Analyzers.Test.GU0031DisposeMembersTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal class HappyPath : HappyPathVerifier<GU0031DisposeMembers>
    {
        [TestCase("this.stream.Dispose();")]
        [TestCase("stream.Dispose();")]
        public async Task DisposingField(string disposeCall)
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        private readonly Stream stream = File.OpenRead("""");
        
        public void Dispose()
        {
            this.stream.Dispose();
        }
    }";
            testCode = testCode.AssertReplace("this.stream.Dispose();", disposeCall);
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task ConditionalDisposingField()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        private readonly Stream stream = File.OpenRead("""");
        
        public void Dispose()
        {
            this.stream?.Dispose();
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldInVirtualDispose()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo : IDisposable
    {
        private readonly Stream stream = File.OpenRead("""");
        private bool disposed;

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.stream.Dispose();
            }
        }

        protected void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingPropertyWhenInitializedInProperty()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        public Foo()
        {
            this.Stream = File.OpenRead("""");
        }

        public Stream Stream { get; set; }
        
        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }";

            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingPropertyWhenInitializedInline()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        public Stream Stream { get; set; } = File.OpenRead("""");
        
        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }";

            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnorePassedInViaCtor()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo
    {
        private readonly IDisposable bar;
        
        public Foo(IDisposable bar)
        {
            this.bar = bar;
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredWhenNotAssigned()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo
    {
        private readonly IDisposable bar;
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredWhenBackingField()
        {
            var testCode = @"
    using System.IO;

    public sealed class Foo
    {
        private Stream stream;

        public Stream Stream
        {
            get { return this.stream; }
            set { this.stream = value; }
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }
    }
}