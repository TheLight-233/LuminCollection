using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection;

[Intrinsic]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LuminDictionary<TKey, TValue> : IDisposable
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    private struct Entry
    {
        public uint HashCode;
        public int Next;
        public TKey Key;
        public TValue Value;
    }

    private int* _buckets;
    private Entry* _entries;
    private int _count;
    private int _version;
    private int _freeList;
    private int _freeCount;
    private int _capacity;
    private ulong _fastModMultiplier;

    public int Count => _count - _freeCount;
    public int Capacity => _capacity;
    public bool IsCreated => _buckets != null;

    public ref TValue this[scoped in TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref GetValueRef(key);
    }
        
    #region Ctor
    
    public LuminDictionary()
    {
        _capacity = HashHelpers.GetPrime(0);
        _count = 0;
        _freeList = -1;
        _freeCount = 0;
        _version = 0;

        nuint bucketAlign = LuminMemoryHelper.AlignOf<int>();
        nuint entryAlign = LuminMemoryHelper.AlignOf<Entry>();
        
        var bucketSize = _capacity * sizeof(int);
        var entriesSize = _capacity * sizeof(Entry);
        
        _buckets = (int*)LuminAllocator.AlignedAlloc(bucketAlign, (nuint)bucketSize);
        _entries = (Entry*)LuminAllocator.AlignedAlloc(entryAlign, (nuint)entriesSize);
        
        Unsafe.InitBlock(_buckets, 0, (uint)(_capacity * sizeof(int)));
        Unsafe.InitBlock(_entries, 0, (uint)(_capacity * sizeof(Entry)));

#if DEBUG
        LuminMemoryHelper.AddMemoryPressure(bucketSize);
        LuminMemoryHelper.AddMemoryPressure(entriesSize);
#endif
        
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)_capacity);
    }
    
    public LuminDictionary(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            
        _capacity = HashHelpers.GetPrime(capacity);
        _count = 0;
        _freeList = -1;
        _freeCount = 0;
        _version = 0;

        nuint bucketAlign = LuminMemoryHelper.AlignOf<int>();
        nuint entryAlign = LuminMemoryHelper.AlignOf<Entry>();
        
        var bucketSize = _capacity * sizeof(int);
        var entriesSize = _capacity * sizeof(Entry);
        
        _buckets = (int*)LuminAllocator.AlignedAlloc(bucketAlign, (nuint)bucketSize);
        _entries = (Entry*)LuminAllocator.AlignedAlloc(entryAlign, (nuint)entriesSize);
        
        Unsafe.InitBlock(_buckets, 0, (uint)(_capacity * sizeof(int)));
        Unsafe.InitBlock(_entries, 0, (uint)(_capacity * sizeof(Entry)));

#if DEBUG
        LuminMemoryHelper.AddMemoryPressure(bucketSize);
        LuminMemoryHelper.AddMemoryPressure(entriesSize);
#endif

        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)_capacity);
    }

    public LuminDictionary(scoped in LuminDictionary<TKey, TValue> src)
    {
        if (!src.IsCreated || src._capacity == 0)
        {
            this = default;
            return;
        }

        _capacity = src._capacity;
        _count = src._count;
        _freeList = src._freeList;
        _freeCount = src._freeCount;
        _version = src._version;
        _fastModMultiplier = src._fastModMultiplier;

        nuint bucketAlign = LuminMemoryHelper.AlignOf<int>();
        nuint entryAlign = LuminMemoryHelper.AlignOf<Entry>();
        
        var bucketSize = _capacity * sizeof(int);
        var entriesSize = _capacity * sizeof(Entry);
        
        _buckets = (int*)LuminAllocator.AlignedAlloc(bucketAlign, (nuint)bucketSize);
        _entries = (Entry*)LuminAllocator.AlignedAlloc(entryAlign, (nuint)entriesSize);

#if DEBUG
        LuminMemoryHelper.AddMemoryPressure(bucketSize);
        LuminMemoryHelper.AddMemoryPressure(entriesSize);
#endif
        
        Unsafe.CopyBlock(_buckets, src._buckets, (uint)(_capacity * sizeof(int)));
        Unsafe.CopyBlock(_entries, src._entries, (uint)(_capacity * sizeof(Entry)));
    }
    #endregion

    #region Basic API
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value)
    {
        if (!TryAdd(key, value))
            throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(TKey key, TValue value)
    {
        uint hashCode = (uint)key.GetHashCode();
        ref int bucket = ref GetBucket(hashCode);
        int collisionCount = 0;
        int i = bucket - 1;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Key.Equals(key))
                return false;

            i = entry.Next;
            if (++collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            ref Entry freeEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), index);
            _freeList = freeEntry.Next;
            _freeCount--;
        }
        else
        {
            if (_count == _capacity)
            {
                Resize();
                bucket = ref GetBucket(hashCode);
            }
            index = _count;
            _count++;
        }
        
        ref Entry newEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), index);
        newEntry.HashCode = hashCode;
        newEntry.Next = bucket - 1;
        newEntry.Key = key;
        newEntry.Value = value;
            
        bucket = index + 1;
        _version++;
            
        return true;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue GetValueRefOrAddDefault(scoped in TKey key, out bool exists)
    {
        uint hashCode = (uint)key.GetHashCode();
        ref int bucket = ref GetBucket(hashCode);
        int collisionCount = 0;
        int i = bucket - 1;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Key.Equals(key))
            {
                exists = true;
                return ref entry.Value;
            }

            i = entry.Next;
            if (++collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            ref Entry freeEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), index);
            _freeList = freeEntry.Next;
            _freeCount--;
        }
        else
        {
            if (_count == _capacity)
            {
                Resize();
                bucket = ref GetBucket(hashCode);
            }
            index = _count;
            _count++;
        }
        
        ref Entry newEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), index);
        newEntry.HashCode = hashCode;
        newEntry.Next = bucket - 1;
        newEntry.Key = key;
        newEntry.Value = default;
            
        bucket = index + 1;
        _version++;

        exists = false;
        return ref newEntry.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(scoped in TKey key, out TValue value)
    {
        uint hashCode = (uint)key.GetHashCode();
        int i = GetBucket(hashCode) - 1;
        int collisionCount = 0;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Key.Equals(key))
            {
                value = entry.Value;
                return true;
            }

            i = entry.Next;
            if (++collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue GetValueRef(scoped in TKey key)
    {
        uint hashCode = (uint)key.GetHashCode();
        int i = GetBucket(hashCode) - 1;
        int collisionCount = 0;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Key.Equals(key))
                return ref entry.Value;

            i = entry.Next;
            if (++collisionCount > _capacity)
                break;
        }

        ThrowHelpers.ThrowKeyNotFoundException(key);
        return ref Unsafe.NullRef<TValue>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(scoped in TKey key)
    {
        uint hashCode = (uint)key.GetHashCode();
        int i = GetBucket(hashCode) - 1;
        int collisionCount = 0;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Key.Equals(key))
                return true;

            i = entry.Next;
            if (++collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        return false;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsValue(scoped in TValue value)
    {
        for (int i = 0; i < _count; i++)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode != 0 && entry.Value.Equals(value))
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(scoped in TKey key)
    {
        uint hashCode = (uint)key.GetHashCode();
        ref int bucket = ref GetBucket(hashCode);
        int last = -1;
        int i = bucket - 1;
        int collisionCount = 0;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Key.Equals(key))
            {
                if (last < 0)
                    bucket = entry.Next + 1;
                else
                {
                    ref Entry lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), last);
                    lastEntry.Next = entry.Next;
                }

                entry.HashCode = 0;
                entry.Next = _freeList;
                entry.Key = default;
                entry.Value = default;
                    
                _freeList = i;
                _freeCount++;
                _version++;
                    
                return true;
            }

            last = i;
            i = entry.Next;
            if (++collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (_count > 0)
        {
            Unsafe.InitBlock(_buckets, 0, (uint)(_capacity * sizeof(int)));
                
            for (int i = 0; i < _count; i++)
            {
                ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
                entry.HashCode = 0;
                entry.Next = 0;
                entry.Key = default;
                entry.Value = default;
            }

            _count = 0;
            _freeList = -1;
            _freeCount = 0;
            _version++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int capacity)
    {
        if (capacity > _capacity)
            Resize(HashHelpers.GetPrime(capacity));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess()
    {
        int newCapacity = HashHelpers.GetPrime(_count - _freeCount);
        if (newCapacity < _capacity)
            Resize(newCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new Enumerator(this);
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_buckets != null)
        {
            LuminAllocator.FreeAligned(_buckets, LuminMemoryHelper.AlignOf<int>());
#if DEBUG
            LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(int));
#endif
            _buckets = null;
        }
            
        if (_entries != null)
        {
            LuminAllocator.FreeAligned(_entries, LuminMemoryHelper.AlignOf<Entry>());
#if DEBUG
            LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(Entry));
#endif
            _entries = null;
        }
            
        _count = 0;
        _freeList = -1;
        _freeCount = 0;
        _version = 0;
        _capacity = 0;
    }
    #endregion

    #region Helpers
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Resize() => Resize(HashHelpers.ExpandPrime(_count));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Resize(int newCapacity)
    {
        nuint bucketAlign = LuminMemoryHelper.AlignOf<int>();
        nuint entryAlign = LuminMemoryHelper.AlignOf<Entry>();
        
        var bucketSize = newCapacity * sizeof(int);
        var entriesSize = newCapacity * sizeof(Entry);
        
        var newBuckets = (int*)LuminAllocator.AlignedAlloc(bucketAlign, (nuint)bucketSize);
        var newEntries = (Entry*)LuminAllocator.AlignedAlloc(entryAlign, (nuint)entriesSize);
        
        Unsafe.InitBlock(newBuckets, 0, (uint)(newCapacity * sizeof(int)));
        Unsafe.InitBlock(newEntries, 0, (uint)(newCapacity * sizeof(Entry)));

#if DEBUG
        LuminMemoryHelper.AddMemoryPressure(bucketSize);
        LuminMemoryHelper.AddMemoryPressure(entriesSize);
#endif
        
        ulong newFastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newCapacity);
            
        int count = _count;
        if (count > 0)
        {
            Unsafe.CopyBlock(newEntries, _entries, (uint)(count * sizeof(Entry)));
                
            for (int i = 0; i < count; i++)
            {
                ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(newEntries), i);
                if (entry.HashCode != 0)
                {
                    ref int bucket = ref GetBucketRef(entry.HashCode, newBuckets, newCapacity, newFastModMultiplier);
                    entry.Next = bucket - 1;
                    bucket = i + 1;
                }
            }
        }
        
        if (newCapacity > count)
        {
            for (int i = count; i < newCapacity - 1; i++)
            {
                ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(newEntries), i);
                entry.Next = i + 1;
            }

            ref Entry lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(newEntries), newCapacity - 1);
            lastEntry.Next = -1;
            _freeList = count;
            _freeCount = newCapacity - count;
        }
        else
        {
            _freeList = -1;
            _freeCount = 0;
        }

        if (_buckets != null)
        {
            LuminAllocator.FreeAligned(_buckets, LuminMemoryHelper.AlignOf<int>());
#if DEBUG
            LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(int));
#endif
            
        }

        if (_entries != null)
        {
            LuminAllocator.FreeAligned(_entries, LuminMemoryHelper.AlignOf<Entry>());
#if DEBUG
            LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(Entry));
#endif
        }

        _buckets = newBuckets;
        _entries = newEntries;
        _capacity = newCapacity;
        _fastModMultiplier = newFastModMultiplier;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref int GetBucket(uint hashCode)
    {
        int index = (int)HashHelpers.FastMod(hashCode, (uint)_capacity, _fastModMultiplier);
        return ref Unsafe.Add(ref Unsafe.AsRef<int>(_buckets), index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref int GetBucketRef(uint hashCode, int* buckets, int capacity, ulong fastModMultiplier)
    {
        int index = (int)HashHelpers.FastMod(hashCode, (uint)capacity, fastModMultiplier);
        return ref Unsafe.Add(ref Unsafe.AsRef<int>(buckets), index);
    }
    #endregion

    #region Enumerator
    public struct Enumerator
    {
        private readonly LuminDictionary<TKey, TValue> _dictionary;
        private readonly int _version;
        private int _index;
        private KeyValuePair<TKey, TValue> _current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(LuminDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _version = dictionary._version;
            _index = 0;
            _current = default;
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (_version != _dictionary._version)
                ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                
            while (_index < _dictionary._count)
            {
                ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dictionary._entries), _index);
                _index++;
                    
                if (entry.HashCode != 0)
                {
                    _current = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                    return true;
                }
            }
                
            _current = default;
            return false;
        }
            
        public KeyValuePair<TKey, TValue> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _index = 0;
            _current = default;
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }
    }
    #endregion
}