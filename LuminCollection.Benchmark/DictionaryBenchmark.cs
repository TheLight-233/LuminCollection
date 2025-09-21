// ------------------------------------------------------------------------------
//  DictionaryBenchmark.cs
//  dotnet run -c Release
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LuminCollection;
using NativeCollections;

namespace LuminCollection.Benchmark
{
    // 可根据需要切换 Job，例如：
    // [SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 3, iterationCount: 5)]
    [MemoryDiagnoser]                       // 打印内存分配
    [MarkdownExporterAttribute.GitHub]      // 生成 Markdown 报告
    public class DictionaryBenchmark
    {
        /* ---------------------------------------------------------
         *  数据规模
         * --------------------------------------------------------- */
        private const int N = 1_000_000;     // 100 万条
        
        private LuminCollection.UnsafeCollection.LuminDictionary<int, int> luminDictionary;
        private LuminDictionary<int, int> luminSafeDictionary;
        private LuminCollection.UnsafeCollection.LuminHashSet<int> luminHashSet;
        private LuminHashSet<int> luminHashSetSafe;
        private LuminCollection.UnsafeCollection.LuminPriorityQueue<int, int> luminPriorityQueue;
        private LuminPriorityQueue<int, int> luminPriorityQueueSafe;

        private NativeDictionary<int, int> nativeDictionary;
        private UnsafeDictionary<int, int> unsafeDictionary;
        private NativeHashSet<int> nativeHashSet;
        private UnsafeHashSet<int> unsafeHashSet;
        private NativePriorityQueue<int, int> nativePriorityQueue;
        private UnsafePriorityQueue<int, int> unsafePriorityQueue;

        private Dictionary<int, int> dictionary;
        private HashSet<int> hashSet;
        private PriorityQueue<int, int> priorityQueue;

        /* ---------------------------------------------------------
         *  测试数据
         * --------------------------------------------------------- */
        private int[] keys;
        private int[] values;

        /* ---------------------------------------------------------
         *  全局初始化：每轮基准仅执行一次
         * --------------------------------------------------------- */
        [GlobalSetup]
        public void Setup()
        {
            // 1. 生成不重复键 & 对应值
            keys = Enumerable.Range(0, 2 * N + 1).ToArray();
            values = keys.Select(i => i + 1).ToArray();

            // 2. 初始化 BCL
            dictionary = new Dictionary<int, int>(2*N);
            hashSet = new HashSet<int>(2*N);
            priorityQueue = new PriorityQueue<int, int>(2*N);

            // 3. 初始化 Lumin*
            luminDictionary = new LuminCollection.UnsafeCollection.LuminDictionary<int, int>(2*N);
            luminSafeDictionary = new LuminDictionary<int, int>(2*N);
            luminHashSet = new LuminCollection.UnsafeCollection.LuminHashSet<int>(2*N);
            luminHashSetSafe = new LuminHashSet<int>(2*N);
            luminPriorityQueue = new LuminCollection.UnsafeCollection.LuminPriorityQueue<int, int>(2*N);
            luminPriorityQueueSafe = new LuminPriorityQueue<int, int>(2*N);

            // 4. 初始化 Native*
            nativeDictionary = new NativeDictionary<int, int>(2*N);
            unsafeDictionary = new UnsafeDictionary<int, int>(2*N);
            nativeHashSet = new NativeHashSet<int>(2*N);
            unsafeHashSet = new UnsafeHashSet<int>(2*N);
            nativePriorityQueue = new NativePriorityQueue<int, int>(2*N);
            unsafePriorityQueue = new UnsafePriorityQueue<int, int>(2*N);
        }

        [GlobalCleanup]
        public void CleanUp()
        {
            luminDictionary.Dispose();
            luminSafeDictionary.Dispose();
            luminHashSet.Dispose();
            luminHashSetSafe.Dispose();
            luminPriorityQueue.Dispose();
            luminPriorityQueueSafe.Dispose();
            
            nativeDictionary.Dispose();
            unsafeDictionary.Dispose();
            nativeHashSet.Dispose();
            unsafeHashSet.Dispose();
            nativePriorityQueue.Dispose();
            unsafePriorityQueue.Dispose();
        }

        [Benchmark]
        public int BCL_Dictionary_Add()
        {
            for (int i = N + 1; i < 2 * N; i++) dictionary.TryAdd(keys[i], values[i]);
            return dictionary.Count;
        }

        [Benchmark]
        public int LuminSafe_Dictionary_Add()                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
        {
            for (int i = N + 1; i < 2 * N; i++) luminSafeDictionary.TryAdd(keys[i], values[i]);
            return luminSafeDictionary.Count;
        }

        [Benchmark]
        public int LuminUnsafe_Dictionary_Add()
        {
            for (int i = N + 1; i < 2 * N; i++) luminDictionary.TryAdd(keys[i], values[i]);
            return luminDictionary.Count;
        }

        [Benchmark]
        public int NativeDictionary_Add()
        {
            for (int i = N + 1; i < 2 * N; i++) nativeDictionary.TryAdd(keys[i], values[i]);
            return nativeDictionary.Count;
        }

        [Benchmark]
        public int UnsafeDictionary_Add()
        {
            for (int i = N + 1; i < 2 * N; i++) unsafeDictionary.TryAdd(keys[i], values[i]);
            return unsafeDictionary.Count;
        }

        
        /* ---------------------------------------------------------
         *  基准方法：TryGetValue
         * --------------------------------------------------------- */
        [Benchmark]
        public int BCL_Dictionary_TryGet()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (dictionary.TryGetValue(keys[i], out var v)) sum += v;
            return sum;
        }

        [Benchmark]
        public int LuminUnsafe_Dictionary_TryGet()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (luminDictionary.TryGetValue(keys[i], out var v)) sum += v;
            return sum;
        }

        [Benchmark]
        public int LuminSafe_Dictionary_TryGet()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (luminSafeDictionary.TryGetValue(keys[i], out var v)) sum += v;
            return sum;
        }

        [Benchmark]
        public int NativeDictionary_TryGet()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (nativeDictionary.TryGetValue(keys[i], out var v)) sum += v;
            return sum;
        }

        [Benchmark]
        public int UnsafeDictionary_TryGet()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (unsafeDictionary.TryGetValue(keys[i], out var v)) sum += v;
            return sum;
        }

        /* ---------------------------------------------------------
         *  基准方法：ContainsKey
         * --------------------------------------------------------- */
        [Benchmark]
        public int BCL_Dictionary_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (dictionary.ContainsKey(keys[i])) sum++;
            return sum;
        }

        [Benchmark]
        public int LuminUnsafe_Dictionary_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (luminDictionary.ContainsKey(keys[i])) sum++;
            return sum;
        }

        [Benchmark]
        public int LuminSafe_Dictionary_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (luminSafeDictionary.ContainsKey(keys[i])) sum++;
            return sum;
        }

        [Benchmark]
        public int NativeDictionary_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (nativeDictionary.ContainsKey(keys[i])) sum++;
            return sum;
        }

        [Benchmark]
        public int UnsafeDictionary_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (unsafeDictionary.ContainsKey(keys[i])) sum++;
            return sum;
        }

        /* ---------------------------------------------------------
         *  基准方法：Remove
         * --------------------------------------------------------- */
        [Benchmark]
        public int BCL_Dictionary_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (dictionary.Remove(keys[i])) cnt++;
            return cnt;
        }

        [Benchmark]
        public int LuminUnsafe_Dictionary_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (luminDictionary.Remove(keys[i])) cnt++;
            return cnt;
        }

        [Benchmark]
        public int LuminSafe_Dictionary_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (luminSafeDictionary.Remove(keys[i])) cnt++;
            return cnt;
        }

        [Benchmark]
        public int NativeDictionary_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (nativeDictionary.Remove(keys[i])) cnt++;
            return cnt;
        }

        [Benchmark]
        public int UnsafeDictionary_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (unsafeDictionary.Remove(keys[i])) cnt++;
            return cnt;
        }
        
        /* ==========================================================
         *  HashSet 基准
         * ========================================================== */
        [Benchmark]
        public int BCL_HashSet_Add()
        {
            for (int i = 0; i < N; i++) hashSet.Add(keys[i]);
            return hashSet.Count;
        }

        [Benchmark]
        public int LuminUnsafe_HashSet_Add()
        {
            for (int i = 0; i < N; i++) luminHashSet.Add(keys[i]);
            return luminHashSet.Count;
        }

        [Benchmark]
        public int LuminSafe_HashSet_Add()
        {
            for (int i = 0; i < N; i++) luminHashSetSafe.TryAdd(keys[i]);
            return luminHashSetSafe.Count;
        }

        [Benchmark]
        public int NativeHashSet_Add()
        {
            for (int i = 0; i < N; i++) nativeHashSet.Add(keys[i]);
            return nativeHashSet.Count;
        }

        [Benchmark]
        public int UnsafeHashSet_Add()
        {
            for (int i = 0; i < N; i++) unsafeHashSet.Add(keys[i]);
            return unsafeHashSet.Count;
        }

/* ------- Contains ------- */
        [Benchmark]
        public int BCL_HashSet_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (hashSet.Contains(keys[i])) sum++;
            return sum;
        }

        [Benchmark]
        public int LuminUnsafe_HashSet_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (luminHashSet.Contains(keys[i])) sum++;
            return sum;
        }

        [Benchmark]
        public int LuminSafe_HashSet_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (luminHashSetSafe.Contains(keys[i])) sum++;
            return sum;
        }

        [Benchmark]
        public int NativeHashSet_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (nativeHashSet.Contains(keys[i])) sum++;
            return sum;
        }

        [Benchmark]
        public int UnsafeHashSet_Contains()
        {
            int sum = 0;
            for (int i = 0; i < N; i++) if (unsafeHashSet.Contains(keys[i])) sum++;
            return sum;
        }

/* ------- Remove ------- */
        [Benchmark]
        public int BCL_HashSet_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (hashSet.Remove(keys[i])) cnt++;
            return cnt;
        }

        [Benchmark]
        public int LuminUnsafe_HashSet_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (luminHashSet.Remove(keys[i])) cnt++;
            return cnt;
        }

        [Benchmark]
        public int LuminSafe_HashSet_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (luminHashSetSafe.Remove(keys[i])) cnt++;
            return cnt;
        }

        [Benchmark]
        public int NativeHashSet_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (nativeHashSet.Remove(keys[i])) cnt++;
            return cnt;
        }

        [Benchmark]
        public int UnsafeHashSet_Remove()
        {
            int cnt = 0;
            for (int i = 0; i < N; i++) if (unsafeHashSet.Remove(keys[i])) cnt++;
            return cnt;
        }
        
        [Benchmark]
        public long BCL_Dictionary_Foreach()
        {
            long sum = 0;
            foreach (var kv in dictionary) sum += kv.Value;
            return sum;
        }

        [Benchmark]
        public long LuminSafe_Dictionary_Foreach()
        {
            long sum = 0;
            foreach (var kv in luminSafeDictionary)
            {
                sum += kv.Value;
            }
            return sum;
        }

        [Benchmark]
        public long LuminUnsafe_Dictionary_Foreach()
        {
            long sum = 0;
            foreach (var kv in luminDictionary)
            {
                sum += kv.Value;
            }
            return sum;
        }

        [Benchmark]
        public long NativeDictionary_Foreach()
        {
            long sum = 0;
            foreach (var kv in nativeDictionary)
            {
                sum += kv.Value;
            }
            return sum;
        }

        [Benchmark]
        public long UnsafeDictionary_Foreach()
        {
            long sum = 0;
            foreach (var kv in unsafeDictionary)
            {
                sum += kv.Value;
            }
            return sum;
        }
    }
}