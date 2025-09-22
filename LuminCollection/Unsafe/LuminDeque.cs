using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LuminDeque<T> : IDisposable where T : unmanaged
    {
        private T* _array;
        private int _head;
        private int _tail;
        private int _size;
        private int _capacity;

        public int Count => _size;
        public int Capacity => _capacity;
        public bool IsCreated => _array != null;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_array), (nuint)((_head + index) % _capacity));
        }
        
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_array), (nuint)((_head + index) % _capacity));
        }

        public ref T Front => ref _array[_head];
        public ref T Back => ref _array[(_tail - 1 + _capacity) % _capacity];

        #region Ctor
        public LuminDeque(int capacity = 0)
        {
            _head = 0; _tail = 0; _size = 0;
            if (capacity <= 0) { _array = null; _capacity = 0; return; }
            _capacity = capacity;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = (uint)(capacity * sizeof(T));
            _array = (T*)LuminAllocator.AlignedAlloc(align, size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            
        }

        public LuminDeque(scoped in LuminDeque<T> src)
        {
            if (!src.IsCreated || src._size == 0)
            { _array = null; _head = 0; _tail = 0; _size = 0; _capacity = 0; return; }

            _size = src._size; _capacity = _size; _head = 0; _tail = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = (uint)(_size * sizeof(T));
            _array = (T*)LuminAllocator.AlignedAlloc(align, size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            for (int i = 0; i < _size; i++) _array[i] = src[i];
        }
        #endregion
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushBack(scoped in T item)
        {
            if (_size == _capacity) Grow(_size + 1);
            _array[_tail] = item;
            _tail = (_tail + 1) % _capacity;
            _size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushFront(scoped in T item)
        {
            if (_size == _capacity) Grow(_size + 1);
            _head = (_head - 1 + _capacity) % _capacity;
            _array[_head] = item;
            _size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PopBack()
        {
            _tail = (_tail - 1 + _capacity) % _capacity;
            T item = _array[_tail];
            _size--;
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PopFront()
        {
            T item = _array[_head];
            _head = (_head + 1) % _capacity;
            _size--;
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPopBack(out T result)
        {
            if (_size == 0) { result = default; return false; }
            result = PopBack(); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPopFront(out T result)
        {
            if (_size == 0) { result = default; return false; }
            result = PopFront(); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekBack(out T result)
        {
            if (_size == 0) { result = default; return false; }
            result = Back; return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekFront(out T result)
        {
            if (_size == 0) { result = default; return false; }
            result = Front; return true;
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
                idx = (idx + 1) % _capacity;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[] dest, int destIndex)
        {
            if (_size == 0) return;
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
            if (_size == 0) return Array.Empty<T>();
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
        public LuminDeque<T> Slice(int start)
        {
            if (start < 0 || start > _size) throw new ArgumentOutOfRangeException(nameof(start));
            int len = _size - start;
            var slice = new LuminDeque<T>(len);
            for (int i = 0; i < len; i++) slice.PushBack(this[start + i]);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminDeque<T> Slice(int start, int length)
        {
            if (start < 0 || length < 0 || start + length > _size) throw new ArgumentOutOfRangeException();
            var slice = new LuminDeque<T>(length);
            for (int i = 0; i < length; i++) slice.PushBack(this[start + i]);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminDeque<TTo> Cast<TTo>() where TTo : unmanaged
        {
            int byteLen = _size * sizeof(T);
            int newCount = byteLen / sizeof(TTo);
            var casted = new LuminDeque<TTo>(newCount);
            for (int i = 0; i < _size; i++) casted.PushBack(Unsafe.As<T, TTo>(ref this[i]));
            return casted;
        }

        public static LuminDeque<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            ref byte b = ref MemoryMarshal.GetReference(buffer);
            nint ptr = (nint)Unsafe.AsPointer(ref b);
            nint aligned = (nint)LuminMemoryHelper.AlignUp((nuint)ptr, alignment);
            byteOffset = aligned - ptr;
            var cast = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            var deque = new LuminDeque<T>(cast.Length);
            foreach (ref readonly var v in cast) deque.PushBack(v);
            return deque;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminDeque<T> Create(scoped in Span<byte> buffer)
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
                    Unsafe.CopyBlock(newArr, _array + _head, (uint)(_size * sizeof(T)));
                else
                {
                    int first = _capacity - _head;
                    Unsafe.CopyBlock(newArr, _array + _head, (uint)(first * sizeof(T)));
                    Unsafe.CopyBlock(newArr + first, _array, (uint)(_tail * sizeof(T)));
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
            private readonly LuminDeque<T> _q;
            private int _idx;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] internal Enumerator(LuminDeque<T> q) { _q = q; _idx = -1; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool MoveNext() => ++_idx < _q._size;
            
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _q[_idx];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Reset() => _idx = -1;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Dispose() { }
        }
        #endregion
    }
}
