``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical cores and 8 physical cores
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                  Method |     Mean |     Error |    StdDev |  Gen 0 | Allocated |
|------------------------ |---------:|----------:|----------:|-------:|----------:|
| RunOnGuAnalyzersProject | 2.317 ms | 0.0528 ms | 0.1531 ms | 3.9063 |  46.21 KB |
