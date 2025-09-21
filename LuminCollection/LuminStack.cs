using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminStack<T> : IDisposable where T : unmanaged
    {
        private UnsafeCollection.LuminStack<T> _stack;

        public int Count => _stack.Count;
        public int Capacity => _stack.Capacity;
        public bool IsCreated => _stack.IsCreated;

        #region Ctor
        
        public LuminStack() : this(4){}
        
        public LuminStack(int capacity)
        {
            _stack = new UnsafeCollection.LuminStack<T>(capacity);
        }

        public LuminStack(scoped in LuminStack<T> source)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!source.IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (source.Count == 0)
                throw new ArgumentException("Source stack must be non-empty", nameof(source));

            _stack = new UnsafeCollection.LuminStack<T>(source._stack);
        }

        public LuminStack(scoped in T[] sourceArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));

            _stack = new UnsafeCollection.LuminStack<T>(sourceArray.Length);
            for (int i = sourceArray.Length - 1; i >= 0; --i)
                _stack.Push(sourceArray[i]);
        }

        public LuminStack(scoped in Span<T> sourceSpan)
        {
            _stack = new UnsafeCollection.LuminStack<T>(sourceSpan.Length);
            for (int i = sourceSpan.Length - 1; i >= 0; --i)
                _stack.Push(sourceSpan[i]);
        }

        public LuminStack(scoped in ReadOnlySpan<T> sourceSpan)
        {
            _stack = new UnsafeCollection.LuminStack<T>(sourceSpan.Length);
            for (int i = sourceSpan.Length - 1; i >= 0; --i)
                _stack.Push(sourceSpan[i]);
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            _stack.Push(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (_stack.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            return _stack.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (_stack.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            return _stack.Peek();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            return _stack.TryPop(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            return _stack.TryPeek(out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            _stack.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            return _stack.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[] array, int arrayIndex)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (Count < 0)
                throw new ArgumentOutOfRangeException(nameof(Count));
            if (arrayIndex + Count > array.Length)
                throw new ArgumentException("Destination array is not long enough");

            _stack.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            return _stack.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            return _stack.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            return _stack.AsReadOnlySpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds stack count {Count}");

            return _stack.AsSpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds stack count {Count}");

            return _stack.AsSpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds stack count {Count}");

            return _stack.AsReadOnlySpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds stack count {Count}");

            return _stack.AsReadOnlySpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminStack<T> Slice(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start > Count)
                throw new ArgumentException($"Range [{start}, {start + 0}) exceeds stack count {Count}");

            var slice = new LuminStack<T>();
            slice._stack = _stack.Slice(start);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminStack<T> Slice(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > Count)
                throw new ArgumentException($"Range [{start}, {start + length}) exceeds stack count {Count}");

            var slice = new LuminStack<T>();
            slice._stack = _stack.Slice(start, length);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminStack<TTo> Cast<TTo>() where TTo : unmanaged
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            var casted = new LuminStack<TTo>();
            casted._stack = _stack.Cast<TTo>();
            return casted;
        }

        public static LuminStack<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            var stack = new LuminStack<T>();
            stack._stack = UnsafeCollection.LuminStack<T>.Create(buffer, alignment, out byteOffset);
            return stack;
        }

        public static LuminStack<T> Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<T>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminStack<T>));
#endif
            return new Enumerator(_stack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _stack.Dispose();
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private UnsafeCollection.LuminStack<T>.Enumerator _e;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] internal Enumerator(UnsafeCollection.LuminStack<T> s) => _e = s.GetEnumerator();

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