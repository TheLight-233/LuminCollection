using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminQueue<T> : IDisposable where T : unmanaged
    {
        private UnsafeCollection.LuminQueue<T> _queue;

        public int Count => _queue.Count;
        public int Capacity => _queue.Capacity;
        public bool IsCreated => _queue.IsCreated;

        #region Ctor
        
        public LuminQueue() : this(4){}
        
        public LuminQueue(int capacity)
        {
            _queue = new UnsafeCollection.LuminQueue<T>(capacity);
        }

        public LuminQueue(scoped in LuminQueue<T>? source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!source.IsCreated, this);
#else
            if (!source.IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (source.Count == 0)
                throw new ArgumentException("Source queue must be non-empty", nameof(source));

            _queue = new UnsafeCollection.LuminQueue<T>(source._queue);
        }

        public LuminQueue(scoped in T[]? sourceArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));

            _queue = new UnsafeCollection.LuminQueue<T>(sourceArray.Length);
            foreach (var v in sourceArray)
                _queue.Enqueue(v);
        }

        public LuminQueue(scoped in Span<T> sourceSpan)
        {
            _queue = new UnsafeCollection.LuminQueue<T>(sourceSpan.Length);
            foreach (ref readonly var v in sourceSpan)
                _queue.Enqueue(v);
        }

        public LuminQueue(scoped in ReadOnlySpan<T> sourceSpan)
        {
            _queue = new UnsafeCollection.LuminQueue<T>(sourceSpan.Length);
            foreach (ref readonly var v in sourceSpan)
                _queue.Enqueue(v);
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            _queue.Enqueue(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (_queue.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _queue.Dequeue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (_queue.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _queue.Peek();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            return _queue.TryDequeue(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            return _queue.TryPeek(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            _queue.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            return _queue.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[]? array, int arrayIndex)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (Count < 0)
                throw new ArgumentOutOfRangeException(nameof(Count));
            if (arrayIndex + Count > array.Length)
                throw new ArgumentException("Destination array is not long enough");

            _queue.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            return _queue.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            return _queue.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            return _queue.AsReadOnlySpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds queue count {Count}");

            return _queue.AsSpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds queue count {Count}");

            return _queue.AsSpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds queue count {Count}");

            return _queue.AsReadOnlySpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds queue count {Count}");

            return _queue.AsReadOnlySpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminQueue<T> Slice(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds queue count {Count}");

            var slice = new LuminQueue<T>();
            slice._queue = _queue.Slice(start);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminQueue<T> Slice(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds queue count {Count}");

            var slice = new LuminQueue<T>();
            slice._queue = _queue.Slice(start, length);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminQueue<TTo> Cast<TTo>() where TTo : unmanaged
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            var casted = new LuminQueue<TTo>();
            casted._queue = _queue.Cast<TTo>();
            return casted;
        }

        public static LuminQueue<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            var queue = new LuminQueue<T>();
            queue._queue = UnsafeCollection.LuminQueue<T>.Create(buffer, alignment, out byteOffset);
            return queue;
        }

        public static LuminQueue<T> Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<T>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminQueue<T>));
#endif
            return new Enumerator(_queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _queue.Dispose();
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private UnsafeCollection.LuminQueue<T>.Enumerator _e;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] internal Enumerator(UnsafeCollection.LuminQueue<T> q) => _e = q.GetEnumerator();

            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _e.Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool MoveNext() => _e.MoveNext();
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Reset() => _e.Reset();
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Dispose() => _e.Dispose();
        }
        #endregion
    }
}