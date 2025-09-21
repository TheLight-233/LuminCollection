using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LuminCollection;
using NativeCollections;
using System.Collections.Generic;

namespace LuminCollection.Benchmark
{
    // 可根据需要切换 Job
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class OtherCollectionBenchmark
    {
        private const int Ops = 1_000_000;

        /*------------------------------------ 队列 ------------------------------------*/
        private LuminQueue<int> _luminSafeQueue;
        private LuminCollection.UnsafeCollection.LuminQueue<int> _luminQueue;
        private Queue<int> _queue;
        private NativeQueue<int> _nativeQueue;
        private UnsafeQueue<int> _unsafeQueue;

        /*------------------------------------ 栈 ------------------------------------*/
        private LuminStack<int> _luminSafeStack;
        private LuminCollection.UnsafeCollection.LuminStack<int> _luminStack;
        private Stack<int> _stack;
        private NativeStack<int> _nativeStack;
        private UnsafeStack<int> _unsafeStack;

        [GlobalSetup]
        public void Setup()
        {
            _luminSafeQueue = new LuminQueue<int>(Ops);
            _luminQueue     = new LuminCollection.UnsafeCollection.LuminQueue<int>(Ops);
            _queue          = new Queue<int>(Ops);
            _nativeQueue    = new NativeQueue<int>(Ops);
            _unsafeQueue    = new UnsafeQueue<int>(Ops);

            _luminSafeStack = new LuminStack<int>(Ops);
            _luminStack     = new LuminCollection.UnsafeCollection.LuminStack<int>(Ops);
            _stack          = new Stack<int>(Ops);
            _nativeStack    = new NativeStack<int>(Ops);
            _unsafeStack    = new UnsafeStack<int>(Ops);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _nativeQueue .Dispose();
            _unsafeQueue .Dispose();
            _nativeStack .Dispose();
            _unsafeStack .Dispose();
        }

        /*------------------------------------ 队列测试 ------------------------------------*/
        [Benchmark(Baseline = true)]
        public void BCL_Queue()
        {
            for (int i = 0; i < Ops; i++) _queue.Enqueue(i);
            for (int i = 0; i < Ops; i++) _queue.Dequeue();
        }

        [Benchmark]
        public void LuminSafeQueue()
        {
            for (int i = 0; i < Ops; i++) _luminSafeQueue.Enqueue(i);
            for (int i = 0; i < Ops; i++) _luminSafeQueue.Dequeue();
        }

        [Benchmark]
        public void LuminUnsafeQueue()
        {
            for (int i = 0; i < Ops; i++) _luminQueue.Enqueue(i);
            for (int i = 0; i < Ops; i++) _luminQueue.Dequeue();
        }

        [Benchmark]
        public void NativeQueue()
        {
            for (int i = 0; i < Ops; i++) _nativeQueue.Enqueue(i);
            for (int i = 0; i < Ops; i++) _nativeQueue.Dequeue();
        }

        [Benchmark]
        public void UnsafeQueue()
        {
            for (int i = 0; i < Ops; i++) _unsafeQueue.Enqueue(i);
            for (int i = 0; i < Ops; i++) _unsafeQueue.Dequeue();
        }

        /*------------------------------------ 栈测试 ------------------------------------*/
        [Benchmark(Baseline = false)]
        public void BCL_Stack()
        {
            for (int i = 0; i < Ops; i++) _stack.Push(i);
            for (int i = 0; i < Ops; i++) _stack.Pop();
        }

        [Benchmark]
        public void LuminSafeStack()
        {
            for (int i = 0; i < Ops; i++) _luminSafeStack.Push(i);
            for (int i = 0; i < Ops; i++) _luminSafeStack.Pop();
        }

        [Benchmark]
        public void LuminUnsafeStack()
        {
            for (int i = 0; i < Ops; i++) _luminStack.Push(i);
            for (int i = 0; i < Ops; i++) _luminStack.Pop();
        }

        [Benchmark]
        public void NativeStack()
        {
            for (int i = 0; i < Ops; i++) _nativeStack.Push(i);
            for (int i = 0; i < Ops; i++) _nativeStack.Pop();
        }

        [Benchmark]
        public void UnsafeStack()
        {
            for (int i = 0; i < Ops; i++) _unsafeStack.Push(i);
            for (int i = 0; i < Ops; i++) _unsafeStack.Pop();
        }
    }
}