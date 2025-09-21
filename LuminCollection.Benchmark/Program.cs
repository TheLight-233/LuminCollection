
using LuminMemoryAllocator;

namespace LuminCollection.Benchmark;

class Program
{
    static unsafe void Main(string[] args)
    {
        BenchmarkDotNet.Running.BenchmarkRunner.Run<CollectionBenchmark>();
    }
}