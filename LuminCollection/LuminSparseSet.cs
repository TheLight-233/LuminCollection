using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminSparseSet<T> : IDisposable where T : unmanaged
    {
        private UnsafeCollection.LuminSparseSet<T> _sparseSet;

        private nuint _version;

        public int Count => _sparseSet.Count;
        public int Capacity => _sparseSet.Capacity;
        public bool IsCreated => _sparseSet.IsCreated;

        public T this[int key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sparseSet[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _sparseSet.Insert(key, value);
        }

        public KeyCollection Keys => new KeyCollection(this);
        public ValueCollection Values => new ValueCollection(this);

        public LuminSparseSet(int capacity = 0)
        {
            _sparseSet = new UnsafeCollection.LuminSparseSet<T>(capacity);
            _version = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _version = 0;
                _sparseSet.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            _version++;
            _sparseSet.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            _sparseSet.EnsureCapacity(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(int key, T value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            var res = _sparseSet.Add(key, value);
            if (res) _version++;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeCollection.LuminSparseSet<T>.InsertResult Insert(int key, T value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            var res = _sparseSet.Insert(key, value);
            if (res is UnsafeCollection.LuminSparseSet<T>.InsertResult.Success) _version++;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            var res = _sparseSet.Remove(key);
            if (res) _version++;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key, out T value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            var res = _sparseSet.Remove(key, out value);
            if (res) _version++;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int key)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            return _sparseSet.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int key, out T value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            return _sparseSet.TryGetValue(key, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueReference(int key)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            return ref _sparseSet.GetValueReference(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyAt(int index)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _sparseSet.GetKeyAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueAt(int index)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ref _sparseSet.GetValueAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<int, T> GetAt(int index)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _sparseSet.GetAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _sparseSet.RemoveAt(index);
            _version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<int, T> keyValuePair)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _sparseSet.RemoveAt(index, out keyValuePair);
            _version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            return _sparseSet.AsReadOnlySpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            if (start < 0 || start > Count)
                throw new ArgumentOutOfRangeException(nameof(start));
            return _sparseSet.AsReadOnlySpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            if (start < 0 || length < 0 || start + length > Count)
                throw new ArgumentOutOfRangeException();
            return _sparseSet.AsReadOnlySpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminSparseSet<T>));
#endif
            return new Enumerator(this);
        }
        

        public struct Enumerator
        {
            private UnsafeCollection.LuminSparseSet<T>.Enumerator _e;
            private readonly LuminSparseSet<T> _sparseSet;
            private readonly nuint _version;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(LuminSparseSet<T> sparseSet)
            {
                _sparseSet = sparseSet;
                _version = sparseSet._version;
                _e = sparseSet._sparseSet.GetEnumerator();
            }

            public KeyValuePair<int, T> Current => _e.Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_version != _sparseSet._version)
                    ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                
                return _e.MoveNext();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                if (_version != _sparseSet._version)
                    ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                
                _e.Reset();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _e.Dispose();
        }

        public struct KeyCollection
        {
            private readonly LuminSparseSet<T> _sparseSet;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(LuminSparseSet<T> sparseSet) => _sparseSet = sparseSet;

            public int Count => _sparseSet.Count;

            public KeyEnumerator GetEnumerator() => new KeyEnumerator(_sparseSet);

            public struct KeyEnumerator
            {
                private UnsafeCollection.LuminSparseSet<T>.KeyCollection.KeyEnumerator _e;
                private readonly LuminSparseSet<T> _sparseSet;
                private readonly nuint _version;
                
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal KeyEnumerator(LuminSparseSet<T> sparseSet)
                {
                    _sparseSet = sparseSet;
                    _version = sparseSet._version;
                    _e = sparseSet._sparseSet.Keys.GetEnumerator();
                }

                public int Current => _e.Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    if (_version != _sparseSet._version)
                        ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                    
                    return _e.MoveNext();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset()
                {
                    if (_version != _sparseSet._version)
                        ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                    
                    _e.Reset();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() => _e.Dispose();
            }
        }

        public struct ValueCollection
        {
            private readonly LuminSparseSet<T> _sparseSet;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(LuminSparseSet<T> sparseSet) => _sparseSet = sparseSet;

            public int Count => _sparseSet.Count;

            public ValueEnumerator GetEnumerator() => new ValueEnumerator(_sparseSet);

            public struct ValueEnumerator
            {
                private UnsafeCollection.LuminSparseSet<T>.ValueCollection.ValueEnumerator _e;
                private readonly LuminSparseSet<T> _sparseSet;
                private readonly nuint _version;
                
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal ValueEnumerator(LuminSparseSet<T> sparseSet)
                {
                    _sparseSet = sparseSet;
                    _version = sparseSet._version;
                    _e = sparseSet._sparseSet.Values.GetEnumerator();
                }

                public T Current => _e.Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    if (_version != _sparseSet._version)
                        ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                    
                    return _e.MoveNext();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset()
                {
                    if (_version != _sparseSet._version)
                        ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                    
                    _e.Reset();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() => _e.Dispose();
            }
        }
    }
}
