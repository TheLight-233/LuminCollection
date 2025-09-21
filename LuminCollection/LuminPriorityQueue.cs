using System.Collections;
using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminPriorityQueue<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : unmanaged, IComparable<TKey>
        where TValue : unmanaged
    {
        private UnsafeCollection.LuminPriorityQueue<TKey, TValue> _priorityQueue;

        public int Count => _priorityQueue.Count;
        public int Capacity => _priorityQueue.Capacity;
        public bool IsCreated => _priorityQueue.IsCreated;

        #region Ctor
        public LuminPriorityQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
            
            _priorityQueue = new UnsafeCollection.LuminPriorityQueue<TKey, TValue>(capacity);
        }

        public LuminPriorityQueue() : this(0) { }

        public LuminPriorityQueue(scoped in LuminPriorityQueue<TKey, TValue> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            
            if (!source.IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            _priorityQueue = new UnsafeCollection.LuminPriorityQueue<TKey, TValue>(in source._priorityQueue);
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(scoped in TValue value, scoped in TKey priority)
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            _priorityQueue.Enqueue(value, priority);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Dequeue()
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            return _priorityQueue.Dequeue();
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TValue value)
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            return _priorityQueue.TryDequeue(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out TValue value)
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            return _priorityQueue.TryPeek(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            _priorityQueue.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            _priorityQueue.EnsureCapacity(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            _priorityQueue.TrimExcess();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            return new Enumerator(_priorityQueue);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _priorityQueue.Dispose();
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private UnsafeCollection.LuminPriorityQueue<TKey, TValue>.Enumerator _e;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeCollection.LuminPriorityQueue<TKey, TValue> priorityQueue) => _e = priorityQueue.GetEnumerator();
            
            public KeyValuePair<TKey, TValue> Current => _e.Current;
            
            object IEnumerator.Current => Current;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _e.MoveNext();
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _e.Reset();
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _e.Dispose();
        }
        #endregion
    }
}