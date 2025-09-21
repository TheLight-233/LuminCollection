using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminDeque<T> : IDisposable where T : unmanaged
    {
        private UnsafeCollection.LuminDeque<T> _deque;

        public int Count => _deque.Count;
        public int Capacity => _deque.Capacity;
        public bool IsCreated => _deque.IsCreated;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _deque[index];
        }
        
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _deque[index];
        }

        public ref T Front
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
                if (_deque.Count == 0)
                    throw new InvalidOperationException("Deque is empty");
                
                return ref _deque.Front;
            }
        }

        public ref T Back
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
                if (_deque.Count == 0)
                    throw new InvalidOperationException("Deque is empty");
                
                return ref _deque.Back;
            }
        }

        #region Ctor
        public LuminDeque(int capacity = 0)
        {
            _deque = new UnsafeCollection.LuminDeque<T>(capacity);
        }

        public LuminDeque(scoped in LuminDeque<T>? source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!source.IsCreated) 
                throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (source.Count == 0)
                throw new ArgumentException("Source deque must be non-empty", nameof(source));

            _deque = new UnsafeCollection.LuminDeque<T>(source._deque);
        }

        public LuminDeque(scoped in T[]? sourceArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            
            _deque = new UnsafeCollection.LuminDeque<T>(sourceArray.Length);
            foreach (var v in sourceArray)
                _deque.PushBack(v);
        }

        public LuminDeque(scoped in Span<T> sourceSpan)
        {
            _deque = new UnsafeCollection.LuminDeque<T>(sourceSpan.Length);
            foreach (ref readonly var v in sourceSpan)
                _deque.PushBack(v);
        }

        public LuminDeque(scoped in ReadOnlySpan<T> sourceSpan)
        {
            _deque = new UnsafeCollection.LuminDeque<T>(sourceSpan.Length);
            foreach (ref readonly var v in sourceSpan)
                _deque.PushBack(v);
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushBack(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            _deque.PushBack(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushFront(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            _deque.PushFront(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PopBack()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (_deque.Count == 0)
                throw new InvalidOperationException("Deque is empty");
            
            return _deque.PopBack();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PopFront()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (_deque.Count == 0)
                throw new InvalidOperationException("Deque is empty");
            
            return _deque.PopFront();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPopBack(out T result)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return _deque.TryPopBack(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPopFront(out T result)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return _deque.TryPopFront(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekBack(out T result)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return _deque.TryPeekBack(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekFront(out T result)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return _deque.TryPeekFront(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            _deque.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return _deque.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[]? array, int arrayIndex)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (Count < 0)
                throw new ArgumentOutOfRangeException(nameof(Count));
            if (arrayIndex + Count > array.Length)
                throw new ArgumentException("Destination array is not long enough");
            
            _deque.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return _deque.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return _deque.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return _deque.AsReadOnlySpan();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds deque count {Count}");
            
            return _deque.AsSpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds deque count {Count}");
            
            return _deque.AsSpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds deque count {Count}");
            
            return _deque.AsReadOnlySpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds deque count {Count}");
            
            return _deque.AsReadOnlySpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminDeque<T> Slice(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds deque count {Count}");
            
            var slice = new LuminDeque<T>();
            slice._deque = _deque.Slice(start);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminDeque<T> Slice(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds deque count {Count}");
            
            var slice = new LuminDeque<T>();
            slice._deque = _deque.Slice(start, length);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminDeque<TTo> Cast<TTo>() where TTo : unmanaged
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            var casted = new LuminDeque<TTo>();
            casted._deque = _deque.Cast<TTo>();
            return casted;
        }

        public static LuminDeque<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            var deque = new LuminDeque<T>();
            deque._deque = UnsafeCollection.LuminDeque<T>.Create(buffer, alignment, out byteOffset);
            return deque;
        }

        public static LuminDeque<T> Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<T>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminDeque<T>));
#endif
            
            return new Enumerator(_deque);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _deque.Dispose();
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private UnsafeCollection.LuminDeque<T>.Enumerator _e;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] internal Enumerator(UnsafeCollection.LuminDeque<T> d) => _e = d.GetEnumerator();
            
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