using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminList<T> : IDisposable where T : unmanaged
    {
        private UnsafeCollection.LuminList<T> _list;

        public int Count => _list.Count;
        public int Capacity => _list.Capacity;
        public bool IsCreated => _list.IsCreated;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list[index];
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list[index];
        }

        #region Ctor

        public LuminList() : this(4){ }

        public LuminList(int capacity)
        {
            _list = new UnsafeCollection.LuminList<T>(capacity);
        }

        public LuminList(scoped in LuminList<T> sourceList)
        {
            if (sourceList is null)
                throw new ArgumentNullException(nameof(sourceList));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!sourceList.IsCreated, this);
#else
            if (!sourceList.IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (sourceList.Count == 0)
                throw new ArgumentException("Source list must be non-empty", nameof(sourceList));

            _list = new UnsafeCollection.LuminList<T>(sourceList._list);
        }

        public LuminList(scoped in T[] sourceArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));

            _list = new UnsafeCollection.LuminList<T>(sourceArray);
        }

        public LuminList(scoped in Span<T> sourceSpan)
        {
            _list = new UnsafeCollection.LuminList<T>(sourceSpan);
        }

        public LuminList(scoped in ReadOnlySpan<T> sourceSpan)
        {
            _list = new UnsafeCollection.LuminList<T>(sourceSpan);
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(scoped in T[] array)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.AddRange(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(scoped in Span<T> span)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.AddRange(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(scoped in ReadOnlySpan<T> span)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.AddRange(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _list.Insert(index, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            return _list.Remove(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _list.RemoveAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (index < 0 || count < 0 || index + count > Count)
                throw new ArgumentOutOfRangeException();

            _list.RemoveRange(index, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            return _list.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            return _list.IndexOf(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            return _list.LastIndexOf(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.Reverse();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.Sort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(scoped in IComparer<T> comparer)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.Sort(comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(scoped in Comparison<T> comparison)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.Sort(comparison);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.EnsureCapacity(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            _list.TrimExcess();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[]? array, int arrayIndex)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex + Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            _list.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            return _list.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            return _list.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            return _list.AsReadOnlySpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (start < 0 || start > Count)
                throw new ArgumentOutOfRangeException(nameof(start));
            return _list.AsSpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (start < 0 || length < 0 || start + length > Count)
                throw new ArgumentOutOfRangeException();
            return _list.AsSpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (start < 0 || start > Count)
                throw new ArgumentOutOfRangeException(nameof(start));
            return _list.AsReadOnlySpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (start < 0 || length < 0 || start + length > Count)
                throw new ArgumentOutOfRangeException();
            return _list.AsReadOnlySpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminList<T> Slice(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (start < 0 || start > Count)
                throw new ArgumentOutOfRangeException(nameof(start));
            var slice = new LuminList<T>();
            slice._list = _list.Slice(start);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminList<T> Slice(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            if (start < 0 || length < 0 || start + length > Count)
                throw new ArgumentOutOfRangeException();
            var slice = new LuminList<T>();
            slice._list = _list.Slice(start, length);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminList<TTo> Cast<TTo>() where TTo : unmanaged
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            var casted = new LuminList<TTo>();
            casted._list = _list.Cast<TTo>();
            return casted;
        }

        public static LuminList<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            var list = new LuminList<T>();
            list._list = UnsafeCollection.LuminList<T>.Create(buffer, alignment, out byteOffset);
            return list;
        }

        public static LuminList<T> Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<T>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminList<T>));
#endif
            return new Enumerator(_list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated) _list.Dispose();
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private UnsafeCollection.LuminList<T>.Enumerator _e;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(UnsafeCollection.LuminList<T> list) => _e = list.GetEnumerator();

            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _e.Current;
            }

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