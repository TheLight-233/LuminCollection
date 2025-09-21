using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LuminStack<T> : IDisposable where T : unmanaged
    {
        private T* _array;
        private int _size;
        private int _capacity;

        public int Count => _size;
        public int Capacity => _capacity;
        public bool IsCreated => _array != null;

        #region Ctor
        public LuminStack(int capacity = 0)
        {
            _size = 0;
            if (capacity <= 0) { _array = null; _capacity = 0; return; }
            _capacity = capacity;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
        }

        public LuminStack(scoped in LuminStack<T> src)
        {
            if (!src.IsCreated || src._size == 0)
            { _array = null; _size = 0; _capacity = 0; return; }

            _size = src._size; _capacity = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            Unsafe.CopyBlock(_array, src._array, (uint)(_size * sizeof(T)));
        }

        public LuminStack(scoped in T[]? src)
        {
            if (src == null || src.Length == 0)
            { _array = null; _size = 0; _capacity = 0; return; }

            _size = src.Length; _capacity = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            fixed (T* p = src) Unsafe.CopyBlock(_array, p, (uint)(_size * sizeof(T)));
        }

        public LuminStack(scoped in Span<T> src)
        {
            if (src.Length == 0)
            { _array = null; _size = 0; _capacity = 0; return; }

            _size = src.Length; _capacity = _size;
            nuint align = LuminMemoryHelper.AlignOf<T>();
            var size = _capacity * sizeof(T);
            _array = (T*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            fixed (T* p = src) Unsafe.CopyBlock(_array, p, (uint)(_size * sizeof(T)));
        }

        public LuminStack(scoped in ReadOnlySpan<T> src)
        {
            if (src.Length == 0)
            { _array = null; _size = 0; _capacity = 0; return; }

            _size = src.Length; _capacity = _size;
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
        public void Push(scoped in T item)
        {
            if (_size == _capacity) Grow(_size + 1);
            _array[_size++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop() => _array[--_size];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek() => _array[_size - 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            if (_size == 0) { result = default; return false; }
            result = Pop(); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result)
        {
            if (_size == 0) { result = default; return false; }
            result = Peek(); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _size = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(scoped in T item)
        {
            for (int i = 0; i < _size; i++)
                if (EqualityComparer<T>.Default.Equals(_array[i], item)) return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in T[]? dest, int destIndex)
        {
            if (_size == 0 || dest is null) return;
            fixed (T* p = &dest[destIndex]) Unsafe.CopyBlock(p, _array, (uint)(_size * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (_size == 0) return [];
            T[] arr = new T[_size];
            fixed (T* p = arr) Unsafe.CopyBlock(p, _array, (uint)(_size * sizeof(T)));
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => new(_array, _size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => new(_array, _size);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => Slice(start).AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => Slice(start, length).AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => Slice(start).AsReadOnlySpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => Slice(start, length).AsReadOnlySpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminStack<T> Slice(int start)
        {
            if (start < 0 || start > _size) throw new ArgumentOutOfRangeException(nameof(start));
            int len = _size - start;
            var slice = new LuminStack<T>(len);
            Unsafe.CopyBlock(slice._array, _array + start, (uint)(len * sizeof(T)));
            slice._size = len;
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminStack<T> Slice(int start, int length)
        {
            if (start < 0 || length < 0 || start + length > _size) throw new ArgumentOutOfRangeException();
            var slice = new LuminStack<T>(length);
            Unsafe.CopyBlock(slice._array, _array + start, (uint)(length * sizeof(T)));
            slice._size = length;
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminStack<TTo> Cast<TTo>() where TTo : unmanaged
        {
            int byteLen = _size * sizeof(T);
            int newCount = byteLen / sizeof(TTo);
            var casted = new LuminStack<TTo>(newCount);
            Unsafe.CopyBlock(casted._array, _array, (uint)byteLen);
            casted._size = newCount;
            return casted;
        }

        public static LuminStack<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            ref byte b = ref MemoryMarshal.GetReference(buffer);
            nint ptr = (nint)Unsafe.AsPointer(ref b);
            nint aligned = (nint)LuminMemoryHelper.AlignUp((nuint)ptr, alignment);
            byteOffset = aligned - ptr;
            var cast = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            var stack = new LuminStack<T>(cast.Length);
            for (int i = cast.Length - 1; i >= 0; --i)
                stack.Push(cast[i]);
            return stack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminStack<T> Create(scoped in Span<byte> buffer)
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
            if (_size > 0) Unsafe.CopyBlock(newArr, _array, (uint)(_size * sizeof(T)));
            if (_array != null)
            {
                LuminAllocator.FreeAligned(_array, align);
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(T));
#endif
            }
            _array = newArr; _capacity = newCap;
        }

        public void Dispose()
        {
            if (_array != null)
            {
                LuminAllocator.FreeAligned(_array, LuminMemoryHelper.AlignOf<T>());
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(T));
#endif
                _array = null; _size = 0; _capacity = 0;
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private readonly LuminStack<T> _s;
            private int _idx;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] internal Enumerator(LuminStack<T> s) { _s = s; _idx = -1; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool MoveNext() => ++_idx < _s._size;
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _s._array[_s._size - 1 - _idx];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Reset() => _idx = -1;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Dispose() { }
        }
        #endregion
    }
}