namespace Gu.Analyzers.Test
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidAll
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> AllAnalyzers = typeof(KnownSymbol).Assembly
                                                                                                     .GetTypes()
                                                                                                     .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                                     .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                                                                                     .ToImmutableArray();

        private static readonly Solution AnalyzerProjectSln = CodeFactory.CreateSolution(
            ProjectFile.Find("Gu.Analyzers.csproj"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        private static readonly Solution ValidCodeProjectSln = CodeFactory.CreateSolution(
            ProjectFile.Find("ValidCode.csproj"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        [Test]
        public void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Length}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void AnalyzerProject(DiagnosticAnalyzer analyzer)
        {
            if (analyzer is SimpleAssignmentAnalyzer ||
                analyzer is ParameterAnalyzer ||
                analyzer is GU0007PreferInjecting)
            {
                Analyze.GetDiagnostics(AnalyzerProjectSln, analyzer);
            }
            else
            {
                RoslynAssert.Valid(analyzer, AnalyzerProjectSln);
            }
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public async Task ValidCodeProject(DiagnosticAnalyzer analyzer)
        {
            if (analyzer is SimpleAssignmentAnalyzer ||
                analyzer is ParameterAnalyzer)
            {
                await Analyze.GetDiagnosticsAsync(ValidCodeProjectSln, analyzer)
                             .ConfigureAwait(false);
            }
            else
            {
                RoslynAssert.Valid(analyzer, ValidCodeProjectSln);
            }
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void WithSyntaxErrors(DiagnosticAnalyzer analyzer)
        {
            var code = @"
namespace N
{
    using System;
    using System.IO;

    internal class C : SyntaxError
    {
        private readonly Stream stream = File.SyntaxError(string.Empty);
        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (this.syntaxError)
            {
                return;
            }

            this.disposed = true;
            if (disposing)
            {
                this.stream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}";
            var sln = CodeFactory.CreateSolution(code, CodeFactory.DefaultCompilationOptions(analyzer, SuppressWarnings.FromAttributes()), MetadataReferences.FromAttributes());
            var diagnostics = Analyze.GetDiagnostics(analyzer, sln);
            RoslynAssert.NoDiagnostics(diagnostics);
        }
    }
}
