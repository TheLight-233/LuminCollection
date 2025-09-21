using System.Collections;
using System.Runtime.CompilerServices;

#pragma warning disable CA2208
#pragma warning disable CS8632


namespace LuminCollection
{
    public sealed class LuminDictionary<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        private UnsafeCollection.LuminDictionary<TKey, TValue> _dictionary;

        public int Count => _dictionary.Count;
        public int Capacity => _dictionary.Capacity;
        public bool IsCreated => _dictionary.IsCreated;

        public ref TValue this[scoped in TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated)
                    throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
                return ref _dictionary.GetValueRef(key);
            }
        }

        #region Ctor
        public LuminDictionary(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
            
            _dictionary = new UnsafeCollection.LuminDictionary<TKey, TValue>(capacity);
        }

        public LuminDictionary() : this(0) { }

        public LuminDictionary(scoped in LuminDictionary<TKey, TValue> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!source.IsCreated, source);
#else
            if (!source.IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            _dictionary = new UnsafeCollection.LuminDictionary<TKey, TValue>(in source._dictionary);
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(scoped in TKey key, scoped in TValue value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            if (!_dictionary.TryAdd(key, value))
                throw new ArgumentException("An element with the same key already exists");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(scoped in TKey key, scoped in TValue value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            return _dictionary.TryAdd(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(scoped in TKey key, out TValue value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            return _dictionary.TryGetValue(key, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(TKey key, out bool exists)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            return ref _dictionary.GetValueRefOrAddDefault(key, out exists);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(scoped in TKey key)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            return _dictionary.ContainsKey(key);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(scoped in TValue value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            return _dictionary.ContainsValue(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            return _dictionary.Remove(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            _dictionary.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            _dictionary.EnsureCapacity(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            _dictionary.TrimExcess();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminDictionary<TKey, TValue>));
#endif
            
            return new Enumerator(_dictionary);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _dictionary.Dispose();
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private UnsafeCollection.LuminDictionary<TKey, TValue>.Enumerator _e;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeCollection.LuminDictionary<TKey, TValue> dictionary) => _e = dictionary.GetEnumerator();
            
            public KeyValuePair<TKey, TValue> Current => _e.Current;
            
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