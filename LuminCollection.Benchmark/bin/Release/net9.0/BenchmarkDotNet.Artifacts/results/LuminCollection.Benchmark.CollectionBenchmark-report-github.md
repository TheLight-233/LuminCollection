```

BenchmarkDotNet v0.15.3, Windows 11 (10.0.26100.6584/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13650HX 2.60GHz, 1 CPU, 20 logical and 14 physical cores
  [Host]   : .NET 9.0.3, X64 NativeAOT x86-64-v3
  .NET 9.0 : .NET 9.0.3 (9.0.3, 9.0.325.11113), X64 RyuJIT x86-64-v3

Job=.NET 9.0  Runtime=.NET 9.0  

```
| Method                      | Mean       | Error    | StdDev   | Gen0      | Gen1      | Gen2      | Allocated |
|---------------------------- |-----------:|---------:|---------:|----------:|----------:|----------:|----------:|
| SequentialAdd_LuminList     |   594.4 μs |  9.23 μs |  8.64 μs |         - |         - |         - |         - |
| SequentialAdd_LuminSafeList |   756.1 μs | 14.95 μs | 25.79 μs |         - |         - |         - |      32 B |
| SequentialAdd_UnsafeList    | 1,722.1 μs |  7.38 μs |  6.55 μs |         - |         - |         - |         - |
| SequentialAdd_BclList       | 2,367.7 μs | 21.80 μs | 20.40 μs | 1968.7500 | 1968.7500 | 1968.7500 | 8389692 B |
