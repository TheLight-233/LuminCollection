using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection;

[Intrinsic]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LuminHashSet<T> : IDisposable
    where T : unmanaged, IEquatable<T>
{
    private struct Entry
    {
        public uint HashCode;
        public int Next;
        public T Value;
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

    #region Ctor
    
    public LuminHashSet()
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
    
    public LuminHashSet(int capacity)
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

    public LuminHashSet(scoped in LuminHashSet<T> src)
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
    public bool Add(T item)
    {
        uint hashCode = (uint)item.GetHashCode();
        ref int bucket = ref GetBucket(hashCode);
        int collisionCount = 0;
        int i = bucket - 1;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Value.Equals(item))
                return false;

            i = entry.Next;
            collisionCount++;
            
            if (collisionCount > _capacity)
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
        newEntry.Value = item;
            
        bucket = index + 1;
        _version++;
            
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        uint hashCode = (uint)item.GetHashCode();
        int i = GetBucket(hashCode) - 1;
        int collisionCount = 0;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Value.Equals(item))
                return true;

            i = entry.Next;
            collisionCount++;
                
            if (collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        uint hashCode = (uint)item.GetHashCode();
        ref int bucket = ref GetBucket(hashCode);
        int last = -1;
        int i = bucket - 1;
        int collisionCount = 0;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Value.Equals(item))
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
                entry.Value = default;
                    
                _freeList = i;
                _freeCount++;
                _version++;
                    
                return true;
            }

            last = i;
            i = entry.Next;
            collisionCount++;
                
            if (collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T equalValue, out T actualValue)
    {
        uint hashCode = (uint)equalValue.GetHashCode();
        ref int bucket = ref GetBucket(hashCode);
        int last = -1;
        int i = bucket - 1;
        int collisionCount = 0;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Value.Equals(equalValue))
            {
                if (last < 0)
                    bucket = entry.Next + 1;
                else
                {
                    ref Entry lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), last);
                    lastEntry.Next = entry.Next;
                }

                actualValue = entry.Value;
                entry.HashCode = 0;
                entry.Next = _freeList;
                entry.Value = default;
                    
                _freeList = i;
                _freeCount++;
                _version++;
                    
                return true;
            }

            last = i;
            i = entry.Next;
            collisionCount++;
                
            if (collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        actualValue = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(T equalValue, out T actualValue)
    {
        uint hashCode = (uint)equalValue.GetHashCode();
        int i = GetBucket(hashCode) - 1;
        int collisionCount = 0;

        while (i >= 0)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode == hashCode && entry.Value.Equals(equalValue))
            {
                actualValue = entry.Value;
                return true;
            }

            i = entry.Next;
            collisionCount++;
                
            if (collisionCount > _capacity)
                ThrowHelpers.ThrowInvalidOperationException("Concurrent operations not supported");
        }

        actualValue = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (_count > 0)
        {
            Unsafe.InitBlockUnaligned(_buckets, 0, (uint)(_capacity * sizeof(int)));
                
            for (int i = 0; i < _count; i++)
            {
                ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
                entry.HashCode = 0;
                entry.Next = 0;
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
    public void CopyTo(Span<T> buffer)
    {
        if (buffer.Length < Count)
            throw new ArgumentException("Buffer too small");
            
        int index = 0;
        for (int i = 0; i < _count && index < buffer.Length; i++)
        {
            ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), i);
            if (entry.HashCode != 0)
            {
                buffer[index] = entry.Value;
                index++;
            }
        }
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
        private readonly LuminHashSet<T> _hashSet;
        private readonly int _version;
        private int _index;
        private T _current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(LuminHashSet<T> hashSet)
        {
            _hashSet = hashSet;
            _version = hashSet._version;
            _index = 0;
            _current = default;
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (_version != _hashSet._version)
                ThrowHelpers.ThrowInvalidOperationException("Collection was modified");
                
            while (_index < _hashSet._count)
            {
                ref Entry entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_hashSet._entries), _index);
                _index++;
                    
                if (entry.HashCode != 0)
                {
                    _current = entry.Value;
                    return true;
                }
            }
                
            _current = default;
            return false;
        }
            
        public T Current
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