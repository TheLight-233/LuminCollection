using System.Collections;
using System.Runtime.CompilerServices;

#pragma warning disable CA2208
#pragma warning disable CS8632


namespace LuminCollection
{
    public sealed unsafe class LuminHashSet<T> : IDisposable, IEnumerable<T>
        where T : unmanaged, IEquatable<T>
    {
        private UnsafeCollection.LuminHashSet<T> _hashSet;

        public int Count => _hashSet.Count;
        public int Capacity => _hashSet.Capacity;
        public bool IsCreated => _hashSet.IsCreated;

        #region Ctor
        public LuminHashSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
            
            _hashSet = new UnsafeCollection.LuminHashSet<T>(capacity);
        }

        public LuminHashSet() : this(0) { }

        public LuminHashSet(scoped in LuminHashSet<T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!source.IsCreated, source);
#else
            if (!source.IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            _hashSet = new UnsafeCollection.LuminHashSet<T>(in source._hashSet);
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
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            if (!_hashSet.Add(item))
                throw new ArgumentException("An element with the same value already exists");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            return _hashSet.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            return _hashSet.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(scoped in T item)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            return _hashSet.Remove(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(scoped in T equalValue, out T actualValue)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            return _hashSet.Remove(equalValue, out actualValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(scoped in T equalValue, out T actualValue)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            return _hashSet.TryGetValue(equalValue, out actualValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            _hashSet.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            _hashSet.EnsureCapacity(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            _hashSet.TrimExcess();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> buffer)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            _hashSet.CopyTo(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminHashSet<T>));
#endif
            
            return new Enumerator(_hashSet);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _hashSet.Dispose();
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator : IEnumerator<T>
        {
            private UnsafeCollection.LuminHashSet<T>.Enumerator _e;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeCollection.LuminHashSet<T> hashSet) => _e = hashSet.GetEnumerator();
            
            public T Current => _e.Current;
            
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