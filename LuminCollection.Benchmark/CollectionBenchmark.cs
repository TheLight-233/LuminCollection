using BenchmarkDotNet.Attributes;
using LuminCollection;
using NativeCollections;
using System.Collections.Generic;

namespace LuminCollection.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
    public class CollectionBenchmark
    {
        private const int N = 1024 * 1024;
        
        /* ======================== 顺序添加 ======================== */
        [Benchmark]
        public void SequentialAdd_LuminList()
        {
            using var luminList = new LuminCollection.UnsafeCollection.LuminList<int>();
            for (int i = 0; i < N; i++)
                luminList.Add(i);
        }
        
        [Benchmark]
        public void SequentialAdd_LuminSafeList()
        {
            using var luminSafeList = new LuminList<int>();
            for (int i = 0; i < N; i++)
                luminSafeList.Add(i);
        }

        [Benchmark]
        public void SequentialAdd_UnsafeList()
        {
            using var unsafeList = new UnsafeList<int>();
            for (int i = 0; i < N; i++)
                unsafeList.Add(i);
        }

        [Benchmark]
        public void SequentialAdd_BclList()
        {
            var bclList = new List<int>();
            for (int i = 0; i < N; i++)
                bclList.Add(i);
        }
        
    }
}