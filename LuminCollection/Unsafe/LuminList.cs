using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LuminList<T> : IDisposable where T : unmanaged
    {
        private T* _items;
        private int _size;
        private int _capacity;

        public int Count => _size;
        public int Capacity => _capacity;
        public bool IsCreated => _items is not null;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
        }
        
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
        }

        #region Ctor
        public LuminList(int capacity = 0)
        {
            _size = 0;
            if (capacity <= 0)
            {
                _items = null;
                _capacity = 0;
                return;
            }
            _capacity = capacity;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = capacity * sizeof(T);
            _items = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            
        }

        public LuminList(scoped in LuminList<T> src)
        {
            if (!src.IsCreated || src._size == 0)
            {
                _items = null; _size = 0; _capacity = 0; return;
            }
            _size = src._size;
            _capacity = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _items = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            Unsafe.CopyBlock(_items, src._items, (uint)(_size * sizeof(T)));
        }

        public LuminList(scoped in T[]? src)
        {
            if (src is null || src.Length == 0) { _items = null; _size = 0; _capacity = 0; return; }
            _size = src.Length; _capacity = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _items = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            fixed (T* p = src) Unsafe.CopyBlock(_items, p, (uint)(_size * sizeof(T)));
        }

        public LuminList(scoped in Span<T> src)
        {
            if (src.Length == 0) { _items = null; _size = 0; _capacity = 0; return; }
            _size = src.Length; _capacity = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _items = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            fixed (T* p = src) Unsafe.CopyBlock(_items, p, (uint)(_size * sizeof(T)));
        }

        public LuminList(scoped in ReadOnlySpan<T> src)
        {
            if (src.Length == 0) { _items = null; _size = 0; _capacity = 0; return; }
            _size = src.Length; _capacity = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _items = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            ref T r = ref MemoryMarshal.GetReference(src);
            fixed (T* p = &r) Unsafe.CopyBlock(_items, p, (uint)(_size * sizeof(T)));
        }
        #endregion
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(scoped in T item)
        {
            if (_size == _capacity) Grow(_size + 1);
            _items[_size++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(scoped in T[]? src)
        {
            if (src == null || src.Length == 0) return;
            int req = _size + src.Length;
            if (req > _capacity) Grow(req);
            fixed (T* p = src) Unsafe.CopyBlock(_items + _size, p, (uint)(src.Length * sizeof(T)));
            _size += src.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(scoped in Span<T> src)
        {
            if (src.Length == 0) return;
            int req = _size + src.Length;
            if (req > _capacity) Grow(req);
            fixed (T* p = src) Unsafe.CopyBlock(_items + _size, p, (uint)(src.Length * sizeof(T)));
            _size += src.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(scoped in ReadOnlySpan<T> src)
        {
            if (src.Length == 0) return;
            int req = _size + src.Length;
            if (req > _capacity) Grow(req);
            ref T r = ref MemoryMarshal.GetReference(src);
            fixed (T* p = &r) Unsafe.CopyBlock(_items + _size, p, (uint)(src.Length * sizeof(T)));
            _size += src.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, scoped in T item)
        {
            if (_size == _capacity) Grow(_size + 1);
            if (index < _size)
                Unsafe.CopyBlock(_items + index + 1, _items + index, (uint)((_size - index) * sizeof(T)));
            _items[index] = item;
            _size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(scoped in T item)
        {
            int i = IndexOf(item);
            if (i < 0) return false;
            RemoveAt(i);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            _size--;
            if (index < _size)
                Unsafe.CopyBlock(_items + index, _items + index + 1, (uint)((_size - index) * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count)
        {
            _size -= count;
            if (index < _size)
                Unsafe.CopyBlock(_items + index, _items + index + count, (uint)((_size - index) * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _size = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(scoped in T item) => IndexOf(item) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(scoped in T item)
        {
            for (int i = 0; i < _size; i++)
                if (EqualityComparer<T>.Default.Equals(_items[i], item)) return i;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(scoped in T item)
        {
            for (int i = _size - 1; i >= 0; i--)
                if (EqualityComparer<T>.Default.Equals(_items[i], item)) return i;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
            for (int i = 0; i < _size / 2; i++)
                (_items[i], _items[_size - i - 1]) = (_items[_size - i - 1], _items[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort() => QuickSort(0, _size - 1, Comparer<T>.Default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(scoped in IComparer<T>? c) => QuickSort(0, _size - 1, c ?? Comparer<T>.Default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(scoped in Comparison<T> cmp) => QuickSort(0, _size - 1, Comparer<T>.Create(cmp));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int cap) { if (cap > _capacity) Grow(cap); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            int th = (int)(_capacity * 0.9);
            if (_size < th) SetCapacity(_size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[]? dest, int destIndex)
        {
            if (dest is null) return;
            fixed (T* p = &dest[destIndex]) Unsafe.CopyBlock(p, _items, (uint)(_size * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (_size == 0) return [];
            T[] arr = new T[_size];
            fixed (T* p = arr) Unsafe.CopyBlock(p, _items, (uint)(_size * sizeof(T)));
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => new(_items, _size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => new(_items, _size);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => new(_items + start, _size - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => new(_items + start, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => new(_items + start, _size - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => new(_items + start, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminList<T> Slice(int start)
        {
            var slice = new LuminList<T>();
            slice._size = _size - start;
            slice._capacity = slice._size;
            if (slice._size > 0)
            {
                nuint align = LuminMemoryHelper.AlignOf<T>();
                var size = slice._size * sizeof(T);
                slice._items = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
                LuminMemoryHelper.AddMemoryPressure(size);
#endif
                
                Unsafe.CopyBlock(slice._items, _items + start, (uint)(slice._size * sizeof(T)));
            }
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminList<T> Slice(int start, int length)
        {
            var slice = new LuminList<T>();
            slice._size = length;
            slice._capacity = length;
            if (length > 0)
            {
                nuint align = LuminMemoryHelper.AlignOf<T>();
                var size = length * sizeof(T);
                slice._items = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
                LuminMemoryHelper.AddMemoryPressure(size);
#endif
                Unsafe.CopyBlock(slice._items, _items + start, (uint)(length * sizeof(T)));
            }
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminList<TTo> Cast<TTo>() where TTo : unmanaged
        {
            int byteLen = _size * sizeof(T);
            int newCount = byteLen / sizeof(TTo);
            var casted = new LuminList<TTo>();
            casted._size = newCount;
            casted._capacity = newCount;
            var size = newCount * sizeof(TTo);
            if (newCount > 0)
            {
                nuint align = LuminMemoryHelper.AlignOf<TTo>();
                casted._items = (TTo*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
                LuminMemoryHelper.AddMemoryPressure(size);
#endif
                Unsafe.CopyBlock(casted._items, _items, (uint)byteLen);
            }
            return casted;
        }

        public static LuminList<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            ref byte b = ref MemoryMarshal.GetReference(buffer);
            nint ptr = (nint)Unsafe.AsPointer(ref b);
            nint aligned = (nint)LuminMemoryHelper.AlignUp((nuint)ptr, alignment);
            byteOffset = aligned - ptr;
            var cast = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            var list = new LuminList<T>();
            list._size = list._capacity = cast.Length;
            if (list._size > 0)
            {
                nuint align = LuminMemoryHelper.AlignOf<T>();
                var size = list._size * sizeof(T);
                list._items = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
                LuminMemoryHelper.AddMemoryPressure(size);
#endif
                Unsafe.CopyBlock(list._items, Unsafe.AsPointer(ref MemoryMarshal.GetReference(cast)), (uint)(list._size * sizeof(T)));
            }
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminList<T> Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<T>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int req)
        {
            int newCap = _capacity == 0 ? 4 : _capacity * 2;
            if (newCap < req) newCap = req;
            SetCapacity(newCap);
        }

        private void SetCapacity(int newCap)
        {
            if (newCap == _capacity) return;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = newCap * sizeof(T);
            T* newItems = newCap > 0 ? (T*)LuminAllocator.AlignedAlloc(align, (nuint)size) : null;
#if DEBUG
                LuminMemoryHelper.AddMemoryPressure(size);
#endif
            if (_size > 0) Unsafe.CopyBlock(newItems, _items, (uint)(_size * sizeof(T)));
            if (_items != null)
            {
                LuminAllocator.FreeAligned(_items, align);
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_size * sizeof(T));
#endif
            }
            _items = newItems;
            _capacity = newCap;
        }

        public void Dispose()
        {
            if (_items != null)
            {
                LuminAllocator.FreeAligned(_items, LuminMemoryHelper.AlignOf<T>());
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_size * sizeof(T));
#endif
                _items = null;
                _size = 0;
                _capacity = 0;
            }
        }

        #region Sort
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QuickSort(int lo, int hi, IComparer<T> c)
        {
            if (hi - lo <= 16) { InsertionSort(lo, hi, c); return; }
            int p = MedianOfThree(lo, hi, c);
            Swap(p, hi);
            int i = lo - 1, j = hi;
            while (true)
            {
                while (c.Compare(_items[++i], _items[hi]) < 0) { }
                while (c.Compare(_items[--j], _items[hi]) > 0 && j > lo) { }
                if (i >= j) break;
                Swap(i, j);
            }
            Swap(i, hi);
            if (lo < i - 1) QuickSort(lo, i - 1, c);
            if (i + 1 < hi) QuickSort(i + 1, hi, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InsertionSort(int lo, int hi, IComparer<T> c)
        {
            for (int i = lo + 1; i <= hi; i++)
            {
                T k = _items[i]; int j = i - 1;
                while (j >= lo && c.Compare(_items[j], k) > 0) _items[j + 1] = _items[j--];
                _items[j + 1] = k;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int MedianOfThree(int lo, int hi, IComparer<T> c)
        {
            int m = lo + (hi - lo >> 1);
            if (c.Compare(_items[lo], _items[m]) > 0) Swap(lo, m);
            if (c.Compare(_items[lo], _items[hi]) > 0) Swap(lo, hi);
            if (c.Compare(_items[m], _items[hi]) > 0) Swap(m, hi);
            return m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int j) => (_items[i], _items[j]) = (_items[j], _items[i]);
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private readonly LuminList<T> _list;
            
            private int _index;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] internal Enumerator(LuminList<T> list) { _list = list; _index = -1; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool MoveNext() => ++_index < _list._size;
            
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _list._items[_index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Reset() => _index = -1;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Dispose() { }
        }
        #endregion
    }
}