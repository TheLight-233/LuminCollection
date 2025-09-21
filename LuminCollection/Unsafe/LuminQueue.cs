using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LuminQueue<T> : IDisposable where T : unmanaged
    {
        private T* _array;
        private int _head;
        private int _tail;
        private int _size;
        private int _capacity;

        public int Count => _size;
        public int Capacity => _capacity;
        public bool IsCreated => _array != null;

        #region Ctor
        public LuminQueue(int capacity = 0)
        {
            _head = 0; _tail = 0; _size = 0;
            if (capacity <= 0) { _array = null; _capacity = 0; return; }
            _capacity = capacity;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            
        }

        public LuminQueue(scoped in LuminQueue<T> src)
        {
            if (!src.IsCreated || src._size == 0)
            { _array = null; _head = 0; _tail = 0; _size = 0; _capacity = 0; return; }

            _size = src._size; _capacity = _size; _head = 0; _tail = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif

            int srcIdx = src._head;
            for (int i = 0; i < _size; i++)
            {
                _array[i] = src._array[srcIdx];
                srcIdx = (srcIdx + 1) % src._capacity;
            }
        }

        public LuminQueue(scoped in T[]? src)
        {
            if (src == null || src.Length == 0)
            { _array = null; _head = 0; _tail = 0; _size = 0; _capacity = 0; return; }

            _size = src.Length; _capacity = _size; _head = 0; _tail = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            fixed (T* p = src) Unsafe.CopyBlock(_array, p, (uint)(_size * sizeof(T)));
        }

        public LuminQueue(scoped in Span<T> src)
        {
            if (src.Length == 0)
            { _array = null; _head = 0; _tail = 0; _size = 0; _capacity = 0; return; }

            _size = src.Length; _capacity = _size; _head = 0; _tail = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            fixed (T* p = src) Unsafe.CopyBlock(_array, p, (uint)(_size * sizeof(T)));
        }

        public LuminQueue(scoped in ReadOnlySpan<T> src)
        {
            if (src.Length == 0)
            { _array = null; _head = 0; _tail = 0; _size = 0; _capacity = 0; return; }

            _size = src.Length; _capacity = _size; _head = 0; _tail = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            ref T r = ref MemoryMarshal.GetReference(src);
            fixed (T* p = &r) Unsafe.CopyBlock(_array, p, (uint)(_size * sizeof(T)));
        }
        #endregion
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(scoped in T item)
        {
            if (_size == _capacity) Grow(_size + 1);
            _array[_tail] = item;
            MoveNext(ref _tail);
            _size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            T item = _array[_head];
            MoveNext(ref _head);
            _size--;
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek() => _array[_head];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result)
        {
            if (_size == 0) { result = default; return false; }
            result = Dequeue(); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result)
        {
            if (_size == 0) { result = default; return false; }
            result = Peek(); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _head = 0; _tail = 0; _size = 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(scoped in T item)
        {
            int idx = _head;
            for (int i = 0; i < _size; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_array[idx], item)) return true;
                MoveNext(ref idx);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[]? dest, int destIndex)
        {
            if (_size == 0 || dest is null) return;
            fixed (T* p = &dest[destIndex])
            {
                if (_head < _tail)
                    Unsafe.CopyBlock(p, _array + _head, (uint)(_size * sizeof(T)));
                else
                {
                    int first = _capacity - _head;
                    Unsafe.CopyBlock(p, _array + _head, (uint)(first * sizeof(T)));
                    Unsafe.CopyBlock(p + first, _array, (uint)(_tail * sizeof(T)));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (_size == 0) return [];
            T[] arr = new T[_size];
            CopyTo(arr, 0);
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => new(_array + _head, _size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => new(_array + _head, _size);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => Slice(start).AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => Slice(start, length).AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => Slice(start).AsReadOnlySpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => Slice(start, length).AsReadOnlySpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminQueue<T> Slice(int start)
        {
            if (start < 0 || start > _size) throw new ArgumentOutOfRangeException(nameof(start));
            int len = _size - start;
            var slice = new LuminQueue<T>(len);
            int idx = (_head + start) % _capacity;
            for (int i = 0; i < len; i++)
            {
                slice.Enqueue(_array[idx]);
                MoveNext(ref idx);
            }
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminQueue<T> Slice(int start, int length)
        {
            if (start < 0 || length < 0 || start + length > _size) throw new ArgumentOutOfRangeException();
            var slice = new LuminQueue<T>(length);
            int idx = (_head + start) % _capacity;
            for (int i = 0; i < length; i++)
            {
                slice.Enqueue(_array[idx]);
                MoveNext(ref idx);
            }
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminQueue<TTo> Cast<TTo>() where TTo : unmanaged
        {
            int byteLen = _size * sizeof(T);
            int newCount = byteLen / sizeof(TTo);
            var casted = new LuminQueue<TTo>(newCount);
            int idx = _head;
            for (int i = 0; i < _size; i++)
            {
                ref T src = ref _array[idx];
                ref TTo dst = ref Unsafe.As<T, TTo>(ref src);
                casted.Enqueue(dst);
                MoveNext(ref idx);
            }
            return casted;
        }

        public static LuminQueue<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            ref byte b = ref MemoryMarshal.GetReference(buffer);
            nint ptr = (nint)Unsafe.AsPointer(ref b);
            nint aligned = (nint)LuminMemoryHelper.AlignUp((nuint)ptr, alignment);
            byteOffset = aligned - ptr;
            var cast = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            var q = new LuminQueue<T>(cast.Length);
            foreach (ref readonly var v in cast)
                q.Enqueue(v);
            return q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminQueue<T> Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<T>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        #region Grow & Dispose
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int required)
        {
            int newCap = _capacity == 0 ? 4 : _capacity * 2;
            if (newCap < required) newCap = required;
            SetCapacity(newCap);
        }

        private void SetCapacity(int newCap)
        {
            if (newCap == _capacity) return;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = newCap * sizeof(T);
            T* newArr = newCap > 0 ? (T*)LuminAllocator.AlignedAlloc(align, (nuint)size) : null;
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Unsafe.CopyBlock(newArr, _array + _head, (uint)(_size * sizeof(T)));
                }
                else
                {
                    int firstPart = _capacity - _head;
                    Unsafe.CopyBlock(newArr, _array + _head, (uint)(firstPart * sizeof(T)));
                    Unsafe.CopyBlock(newArr + firstPart, _array, (uint)(_tail * sizeof(T)));
                }
            }
            if (_array != null)
            {
                LuminAllocator.FreeAligned(_array, align);
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(T));
#endif
            }
            _array = newArr; _capacity = newCap; _head = 0; _tail = _size;
        }

        public void Dispose()
        {
            if (_array != null)
            {
                LuminAllocator.FreeAligned(_array, LuminMemoryHelper.AlignOf<T>());
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(T));
#endif
                _array = null; _size = 0; _capacity = 0; _head = 0; _tail = 0;
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private LuminQueue<T> _q;
            private int _count;
            private int _idx;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] internal Enumerator(LuminQueue<T> q) { _q = q; _count = 0; _idx = -1; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool MoveNext()
            {
                if (_count >= _q._size) return false;
                _idx = _idx == -1 ? _q._head : _q.MoveNext(_idx);
                _count++; return true;
            }
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _q._array[_idx];
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Reset() { _idx = -1; _count = 0; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Dispose() { }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int MoveNext(int index)
        {
            int next = index + 1;
            if (next == _capacity)
                next = 0;
            return next;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveNext(ref int index)
        {
            int next = index + 1;
            if (next == _capacity)
                next = 0;
            index = next;
        }
    }
}