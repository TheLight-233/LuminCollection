using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LuminCollection.UnsafeCollection
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public struct LuminSparseSet<T> : IDisposable where T : unmanaged
    {
        private LuminList<Entry> _dense;
        private LuminList<int> _sparse;

        public int Count => _dense.Count;
        public int Capacity => _sparse.Capacity;
        public bool IsCreated => _dense.IsCreated && _sparse.IsCreated;

        public T this[int key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (TryGetValue(key, out T value))
                    return value;
                ThrowKeyNotFoundException(key);
                return default;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Insert(key, value);
        }

        public KeyCollection Keys => new KeyCollection(this);
        public ValueCollection Values => new ValueCollection(this);

        public LuminSparseSet(int capacity = 0)
        {
            if (capacity < 4)
                capacity = 4;

            _dense = new LuminList<Entry>(capacity);
            _sparse = new LuminList<int>(capacity);
            
            for (int i = 0; i < capacity; i++)
            {
                _sparse.Add(-1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _dense.Dispose();
            _sparse.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (int i = 0; i < _sparse.Count; i++)
            {
                _sparse[i] = -1;
            }
            _dense.Clear();
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
            if (capacity <= _sparse.Capacity)
                return;

            int oldCapacity = _sparse.Capacity;
            _sparse.EnsureCapacity(capacity);
            
            for (int i = oldCapacity; i < capacity; i++)
            {
                _sparse.Add(-1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(int key, T value)
        {
            if (key < 0)
                ThrowNegativeKeyException(key);
                
            if (key >= _sparse.Count)
                EnsureCapacity(key + 1);

            if (_sparse[key] != -1)
                return false;

            int index = _dense.Count;
            _dense.Add(new Entry { Key = key, Value = value });
            _sparse[key] = index;
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult Insert(int key, T value)
        {
            if (key < 0)
                ThrowNegativeKeyException(key);
                
            if (key >= _sparse.Count)
                EnsureCapacity(key + 1);

            int index = _sparse[key];
            if (index != -1)
            {
                _dense[index] = new Entry { Key = key, Value = value };
                return InsertResult.Overwritten;
            }

            index = _dense.Count;
            _dense.Add(new Entry { Key = key, Value = value });
            _sparse[key] = index;
            
            return InsertResult.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key)
        {
            if (key < 0 || key >= _sparse.Count)
                return false;

            int index = _sparse[key];
            if (index == -1)
                return false;

            int lastIndex = _dense.Count - 1;
            if (index != lastIndex)
            {
                Entry lastEntry = _dense[lastIndex];
                _dense[index] = lastEntry;
                _sparse[lastEntry.Key] = index;
            }

            _dense.RemoveAt(lastIndex);
            _sparse[key] = -1;
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key, out T value)
        {
            value = default;
            
            if (key < 0 || key >= _sparse.Count)
                return false;

            int index = _sparse[key];
            if (index == -1)
                return false;

            value = _dense[index].Value;

            int lastIndex = _dense.Count - 1;
            if (index != lastIndex)
            {
                Entry lastEntry = _dense[lastIndex];
                _dense[index] = lastEntry;
                _sparse[lastEntry.Key] = index;
            }

            _dense.RemoveAt(lastIndex);
            _sparse[key] = -1;
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int key)
        {
            return key >= 0 && key < _sparse.Count && _sparse[key] != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int key, out T value)
        {
            value = default;
            
            if (key < 0 || key >= _sparse.Count)
                return false;

            int index = _sparse[key];
            if (index == -1)
                return false;

            value = _dense[index].Value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueReference(int key)
        {
            if (key < 0 || key >= _sparse.Count)
                ThrowKeyNotFoundException(key);

            int index = _sparse[key];
            if (index == -1)
                ThrowKeyNotFoundException(key);

            return ref _dense[index].Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyAt(int index)
        {
            if (index < 0 || index >= _dense.Count)
                ThrowIndexOutOfRangeException(index);

            return _dense[index].Key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueAt(int index)
        {
            if (index < 0 || index >= _dense.Count)
                ThrowIndexOutOfRangeException(index);

            return ref _dense[index].Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<int, T> GetAt(int index)
        {
            if (index < 0 || index >= _dense.Count)
                ThrowIndexOutOfRangeException(index);

            Entry entry = _dense[index];
            return new KeyValuePair<int, T>(entry.Key, entry.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _dense.Count)
                ThrowIndexOutOfRangeException(index);

            int key = _dense[index].Key;

            int lastIndex = _dense.Count - 1;
            if (index != lastIndex)
            {
                Entry lastEntry = _dense[lastIndex];
                _dense[index] = lastEntry;
                _sparse[lastEntry.Key] = index;
            }

            _dense.RemoveAt(lastIndex);
            _sparse[key] = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<int, T> keyValuePair)
        {
            if (index < 0 || index >= _dense.Count)
                ThrowIndexOutOfRangeException(index);

            Entry entry = _dense[index];
            keyValuePair = new KeyValuePair<int, T>(entry.Key, entry.Value);
            int key = entry.Key;

            int lastIndex = _dense.Count - 1;
            if (index != lastIndex)
            {
                Entry lastEntry = _dense[lastIndex];
                _dense[index] = lastEntry;
                _sparse[lastEntry.Key] = index;
            }

            _dense.RemoveAt(lastIndex);
            _sparse[key] = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan()
        {
            return MemoryMarshal.Cast<Entry, KeyValuePair<int, T>>(_dense.AsReadOnlySpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start)
        {
            return MemoryMarshal.Cast<Entry, KeyValuePair<int, T>>(_dense.AsReadOnlySpan(start));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start, int length)
        {
            return MemoryMarshal.Cast<Entry, KeyValuePair<int, T>>(_dense.AsReadOnlySpan(start, length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(this);

        public struct Enumerator
        {
            private LuminSparseSet<T> _sparseSet;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(LuminSparseSet<T> sparseSet)
            {
                _sparseSet = sparseSet;
                _index = -1;
            }

            public KeyValuePair<int, T> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _sparseSet.GetAt(_index);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_index < _sparseSet.Count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }

        public struct KeyCollection
        {
            private LuminSparseSet<T> _sparseSet;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(LuminSparseSet<T> sparseSet) => _sparseSet = sparseSet;

            public int Count => _sparseSet.Count;

            public KeyEnumerator GetEnumerator() => new KeyEnumerator(_sparseSet);

            public struct KeyEnumerator
            {
                private LuminSparseSet<T> _sparseSet;
                private int _index;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal KeyEnumerator(LuminSparseSet<T> sparseSet)
                {
                    _sparseSet = sparseSet;
                    _index = -1;
                }

                public int Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _sparseSet.GetKeyAt(_index);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    return ++_index < _sparseSet.Count;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() { }
            }
        }

        public struct ValueCollection
        {
            private LuminSparseSet<T> _sparseSet;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(LuminSparseSet<T> sparseSet) => _sparseSet = sparseSet;

            public int Count => _sparseSet.Count;

            public ValueEnumerator GetEnumerator() => new ValueEnumerator(_sparseSet);

            public struct ValueEnumerator
            {
                private LuminSparseSet<T> _sparseSet;
                private int _index;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal ValueEnumerator(LuminSparseSet<T> sparseSet)
                {
                    _sparseSet = sparseSet;
                    _index = -1;
                }

                public ref T Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => ref _sparseSet.GetValueAt(_index);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    return ++_index < _sparseSet.Count;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() { }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowKeyNotFoundException(int key) => 
            throw new KeyNotFoundException($"The key '{key}' was not found in the sparse set.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowNegativeKeyException(int key) => 
            throw new ArgumentOutOfRangeException(nameof(key), $"Key '{key}' cannot be negative.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIndexOutOfRangeException(int index) => 
            throw new ArgumentOutOfRangeException(nameof(index), $"Index '{index}' is out of range.");

        private struct Entry
        {
            public int Key;
            public T Value;
        }

        public enum InsertResult
        {
            Success,
            Overwritten
        }
    }
}
