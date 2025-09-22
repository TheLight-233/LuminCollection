using System.Collections;
using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminPriorityQueue<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : unmanaged, IComparable<TKey>
        where TValue : unmanaged
    {
        private UnsafeCollection.LuminPriorityQueue<TKey, TValue> _priorityQueue;

        private nuint _version;

        public int Count => _priorityQueue.Count;
        public int Capacity => _priorityQueue.Capacity;
        public bool IsCreated => _priorityQueue.IsCreated;

        #region Ctor
        public LuminPriorityQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
            
            _priorityQueue = new UnsafeCollection.LuminPriorityQueue<TKey, TValue>(capacity);
            _version = 0;
        }

        public LuminPriorityQueue() : this(0) { }

        public LuminPriorityQueue(scoped in LuminPriorityQueue<TKey, TValue> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            
            if (!source.IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            _priorityQueue = new UnsafeCollection.LuminPriorityQueue<TKey, TValue>(in source._priorityQueue);
            _version = source._version;
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(scoped in TValue value, scoped in TKey priority)
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            _priorityQueue.Enqueue(value, priority);
            _version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Dequeue()
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            _version++;
            return _priorityQueue.Dequeue();
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TValue value)
        {
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminPriorityQueue<TKey, TValue>));
            
            
            var res = _priorityQueue.TryDequeue(out value);
            if (res) _version++;
            return res;
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
            _version++;
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
            
            return new Enumerator(this);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _priorityQueue.Dispose();
                _version = 0;
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private UnsafeCollection.LuminPriorityQueue<TKey, TValue>.Enumerator _e;
            private readonly LuminPriorityQueue<TKey, TValue> _priorityQueue;
            private readonly nuint _version;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(LuminPriorityQueue<TKey, TValue> priorityQueue)
            {
                _priorityQueue = priorityQueue;
                _version = priorityQueue._version;
                _e = priorityQueue._priorityQueue.GetEnumerator();
            }

            public KeyValuePair<TKey, TValue> Current => _e.Current;
            
            object IEnumerator.Current => Current;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_version != _priorityQueue._version)
                    ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                
                return _e.MoveNext();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                if (_version != _priorityQueue._version)
                    ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                
                _e.Reset();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _e.Dispose();
        }
        #endregion
    }
}
