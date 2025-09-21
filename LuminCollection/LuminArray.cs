using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminArray<T> : IDisposable where T : unmanaged
    {
        internal UnsafeCollection.LuminArray<T> _unsafeArray;

        public int Length => _unsafeArray.Length;
        public bool IsCreated => _unsafeArray.IsCreated;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _unsafeArray[index];
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _unsafeArray[index];
        }

        #region Ctor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");

            _unsafeArray = new UnsafeCollection.LuminArray<T>(length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(int length, bool zeroed) : this(length)
        {
            if (zeroed)
                _unsafeArray.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in UnsafeCollection.LuminArray<T> source)
        {
            if (source.Length is 0)
                throw new ArgumentException("LuminArray cannot be empty", nameof(UnsafeCollection.LuminArray<T>));

            _unsafeArray = new UnsafeCollection.LuminArray<T>(source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in T[] source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            _unsafeArray = new UnsafeCollection.LuminArray<T>(source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in Span<T> span)
        {
            if (span.IsEmpty)
                throw new ArgumentException("Span cannot be empty", nameof(span));

            _unsafeArray = new UnsafeCollection.LuminArray<T>(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in ReadOnlySpan<T> span)
        {
            if (span.IsEmpty)
                throw new ArgumentException("Span cannot be empty", nameof(span));

            _unsafeArray = new UnsafeCollection.LuminArray<T>(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe LuminArray(scoped in T* pointer, int length)
        {
            if (pointer == null)
                throw new ArgumentNullException(nameof(pointer));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");

            _unsafeArray = new UnsafeCollection.LuminArray<T>(pointer, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in LuminArray<T> other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!other.IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            _unsafeArray = new UnsafeCollection.LuminArray<T>(other._unsafeArray);
        }
        #endregion

        #region Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
                _unsafeArray.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            _unsafeArray.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[] dest, int destIndex)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));
            if (destIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(destIndex));
            if (Length < 0)
                throw new ArgumentOutOfRangeException(nameof(Length));
            if (destIndex + Length > dest.Length)
                throw new ArgumentException("Destination array is not long enough");

            _unsafeArray.CopyTo(dest, destIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(scoped in T[] src, int srcIndex)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (src is null)
                throw new ArgumentNullException(nameof(src));
            if (srcIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(srcIndex));
            if (Length < 0)
                throw new ArgumentOutOfRangeException(nameof(Length));
            if (srcIndex + Length > src.Length)
                throw new ArgumentException("Source array is not long enough");

            _unsafeArray.CopyFrom(src, srcIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in LuminArray<T> dest, int destIndex = 0, int srcIndex = 0, int count = -1)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!dest.IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            count = count < 0 ? Length - srcIndex : count;
            if (srcIndex < 0 || count < 0 || srcIndex + count > Length)
                throw new ArgumentOutOfRangeException(nameof(srcIndex));
            if (destIndex < 0 || destIndex + count > dest.Length)
                throw new ArgumentOutOfRangeException(nameof(destIndex));

            _unsafeArray.CopyTo(dest._unsafeArray, destIndex, srcIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            return _unsafeArray.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            return _unsafeArray.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            return _unsafeArray.AsReadOnlySpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            return _unsafeArray.AsSpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int len)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (start < 0 || len < 0 || start + len > Length)
                throw new ArgumentOutOfRangeException();
            return _unsafeArray.AsSpan(start, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            return _unsafeArray.AsReadOnlySpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int len)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (start < 0 || len < 0 || start + len > Length)
                throw new ArgumentOutOfRangeException();
            return _unsafeArray.AsReadOnlySpan(start, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray<T> Slice(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            return new LuminArray<T>(_unsafeArray.Slice(start));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray<T> Slice(int start, int len)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (start < 0 || len < 0 || start + len > Length)
                throw new ArgumentOutOfRangeException();
            return new LuminArray<T>(_unsafeArray.Slice(start, len));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray<TTo> Cast<TTo>() where TTo : unmanaged
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            return new LuminArray<TTo>(_unsafeArray.Cast<TTo>());
        }

        public static LuminArray<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            var unsafeArray = UnsafeCollection.LuminArray<T>.Create(buffer, alignment, out byteOffset);
            return new LuminArray<T>(unsafeArray);
        }

        public static LuminArray<T> Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<T>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            _unsafeArray.Sort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(scoped in IComparer<T> comparer)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (comparer is null) throw new ArgumentNullException(nameof(comparer));
            _unsafeArray.Sort(comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(scoped in Comparison<T> comparison)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            if (comparison is null) throw new ArgumentNullException(nameof(comparison));
            _unsafeArray.Sort(comparison);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
            return new Enumerator(this);
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private UnsafeCollection.LuminArray<T>.Enumerator _e;
            private LuminArray<T> _parent;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(LuminArray<T> parent)
            {
                _parent = parent;
                _e = parent._unsafeArray.GetEnumerator();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _e.MoveNext();

            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _e.Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(!_parent.IsCreated, this);
#else
                if (!_parent.IsCreated)
                    throw new ObjectDisposedException(nameof(LuminArray<T>));
#endif
                throw new NotSupportedException("Enumerator reset is not supported");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }
        #endregion
    }
}