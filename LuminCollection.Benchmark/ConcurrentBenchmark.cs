using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using LuminCollection;
using NativeCollections;

[MemoryDiagnoser]               // 显示内存分配
[ThreadingDiagnoser]            // 显示线程竞争情况
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)] // 可改成你需要的 TFM
public class ConcurrentQueueBenchmark
{
    private const int OpsPerInvoke = 1_000_000; // 每次操作总量
    
    private LuminCollection.UnsafeCollection.LuminConcurrentQueue<int> _luminUnSafeQueue;
    private ConcurrentQueue<int> _bclQueue;

    [GlobalSetup]
    public void Setup()
    {
        _luminUnSafeQueue = new LuminCollection.UnsafeCollection.LuminConcurrentQueue<int>(4);
        _bclQueue = new ConcurrentQueue<int>();
    }

    [GlobalCleanup]
    public void CleanUp()
    {
        _luminUnSafeQueue.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void BclEnqueueDequeue()
    {
        for (int i = 0; i < OpsPerInvoke; i++)
        {
            _bclQueue.Enqueue(i);
            _bclQueue.TryDequeue(out _);
        }
    }
    
    [Benchmark]
    public void LuminUnsafeEnqueueDequeue()
    {
        for (int i = 0; i < OpsPerInvoke; i++)
        {
            _luminUnSafeQueue.Enqueue(i);
            _luminUnSafeQueue.TryDequeue(out _);
        }
    }
    

    // --------- 多线程场景 ---------
    [Benchmark]
    public void BclParallelEnqueueDequeue()
    {
        Parallel.For(0, OpsPerInvoke, i =>
        {
            _bclQueue.Enqueue(i);
            _bclQueue.TryDequeue(out _);
        });
    }
    
    
    [Benchmark]
    public void LuminUnsafeParallelEnqueueDequeue()
    {
        Parallel.For(0, OpsPerInvoke, i =>
        {
            _luminUnSafeQueue.Enqueue(i);
            _luminUnSafeQueue.TryDequeue(out _);
        });
    }
    
}