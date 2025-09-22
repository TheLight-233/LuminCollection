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

        private nuint _version;

        public int Count => _hashSet.Count;
        public int Capacity => _hashSet.Capacity;
        public bool IsCreated => _hashSet.IsCreated;

        #region Ctor
        public LuminHashSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
            
            _hashSet = new UnsafeCollection.LuminHashSet<T>(capacity);
            _version++;
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
            _version = source._version;
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
            
            _version++;
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
            
            var res = _hashSet.Add(item);
            _version++;
            return res;
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
            
            var res = _hashSet.Remove(item);
            _version++;
            return res;
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
            
            var res = _hashSet.Remove(equalValue, out actualValue);
            _version++;
            return res;
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
            _version++;
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
            
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _hashSet.Dispose();
                _version = 0;
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator : IEnumerator<T>
        {
            private UnsafeCollection.LuminHashSet<T>.Enumerator _e;
            private readonly LuminHashSet<T> _hashSet;
            private readonly nuint _version;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(LuminHashSet<T> hashSet)
            {
                _hashSet = hashSet;
                _version = hashSet._version;
                _e = hashSet._hashSet.GetEnumerator();
            }

            public T Current => _e.Current;
            
            object IEnumerator.Current => Current;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_version != _hashSet._version)
                    ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                
                return _e.MoveNext();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                if (_version != _hashSet._version)
                    ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                
                _e.Reset();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _e.Dispose();
        }
        #endregion
    }

    
}
