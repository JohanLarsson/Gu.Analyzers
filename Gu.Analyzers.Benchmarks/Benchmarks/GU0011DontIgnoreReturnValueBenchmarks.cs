// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming
namespace Gu.Analyzers.Benchmarks.Benchmarks
{
    [BenchmarkDotNet.Attributes.MemoryDiagnoser]
    public class GU0011DontIgnoreReturnValueBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new Gu.Analyzers.GU0011DontIgnoreReturnValue());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnGuAnalyzersProject()
        {
            Benchmark.Run();
        }
    }
}
