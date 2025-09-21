using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LuminArray<T> : IDisposable where T : unmanaged
    {
        private T* _data;
        
        private int _length;

        public int Length => _length;
        public bool IsCreated => _data != null;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_data), (nint)index);
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_data), (nint)index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(int length)
        {
            _length = length;
            var size = (uint)(length * sizeof(T));
            _data = (T*)LuminAllocator.AlignedAlloc(LuminMemoryHelper.AlignOf<T>(), size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
           
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(int length, bool zeroed) : this(length)
        {
            if (zeroed) Unsafe.InitBlockUnaligned(_data, 0, (uint)(_length * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in T[] source)
        {
            _length = source.Length;
            var size = (uint)(_length * sizeof(T));
            _data = (T*)LuminAllocator.AlignedAlloc(LuminMemoryHelper.AlignOf<T>(), size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            fixed (T* p = source) Unsafe.CopyBlock(_data, p, (uint)(_length * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in Span<T> span)
        {
            _length = span.Length;
            var size = (uint)(_length * sizeof(T));
            _data = (T*)LuminAllocator.AlignedAlloc(LuminMemoryHelper.AlignOf<T>(), size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            fixed (T* p = span) Unsafe.CopyBlock(_data, p, (uint)(_length * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in ReadOnlySpan<T> span)
        {
            _length = span.Length;
            var size = (uint)(_length * sizeof(T));
            _data = (T*)LuminAllocator.AlignedAlloc(LuminMemoryHelper.AlignOf<T>(), size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            ref T r = ref MemoryMarshal.GetReference(span);
            fixed (T* p = &r) Unsafe.CopyBlock(_data, p, (uint)(_length * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in T* pointer, int length)
        {
            _data = pointer;
            _length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in LuminArray<T> other)
        {
            _length = other._length;
            var size = (uint)(_length * sizeof(T));
            _data = (T*)LuminAllocator.AlignedAlloc(LuminMemoryHelper.AlignOf<T>(), size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            Unsafe.CopyBlock(_data, other._data, (uint)(_length * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray(scoped in LuminCollection.LuminArray<T> other) : this(other._unsafeArray)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_data != null)
            {
                LuminAllocator.FreeAligned(_data, LuminMemoryHelper.AlignOf<T>());
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_length * sizeof(T));
#endif
                
                _data = null;
                _length = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Unsafe.InitBlockUnaligned(_data, 0, (uint)(_length * sizeof(T)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] dest, int destIndex)
        {
            fixed (T* p = &dest[destIndex]) Unsafe.CopyBlock(p, _data, (uint)(_length * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] src, int srcIndex)
        {
            fixed (T* p = &src[srcIndex]) Unsafe.CopyBlock(_data, p, (uint)(_length * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in LuminArray<T> dest, int destIndex = 0, int srcIndex = 0, int count = -1)
        {
            if (count < 0) count = _length - srcIndex;
            Unsafe.CopyBlock(dest._data + destIndex, _data + srcIndex, (uint)(count * sizeof(T)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
#if NET8_0_OR_GREATER
            var arr = GC.AllocateUninitializedArray<T>(_length);
#else
            var arr = new T[_length];
#endif
            CopyTo(arr, 0);
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => new(_data, _length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => new(_data, _length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => new(_data + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int len) => new(_data + start, len);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => new(_data + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int len) => new(_data + start, len);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray<T> Slice(int start) => new(_data + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray<T> Slice(int start, int len) => new(_data + start, len);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminArray<TTo> Cast<TTo>() where TTo : unmanaged
            => MemoryMarshal.Cast<T, TTo>(AsSpan()).ToLuminArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminArray<T> Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            ref byte b = ref MemoryMarshal.GetReference(buffer);
            nint ptr = (nint)Unsafe.AsPointer(ref b);
            nint aligned = (nint)LuminMemoryHelper.AlignUp((nuint)ptr, alignment);
            byteOffset = aligned - ptr;
            var cast = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            return new LuminArray<T>((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(cast)), cast.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminArray<T> Create(scoped in Span<byte> buffer) => 
            Create(buffer, (uint)LuminMemoryHelper.AlignOf<T>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort() => QuickSort(0, _length - 1, Comparer<T>.Default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(scoped in IComparer<T> c) => QuickSort(0, _length - 1, c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(scoped in Comparison<T> cmp) => QuickSort(0, _length - 1, Comparer<T>.Create(cmp));
        
        private void QuickSort(int lo, int hi, IComparer<T> c)
        {
            if (hi - lo <= 16) { InsertionSort(lo, hi, c); return; }
            int p = MedianOfThree(lo, hi, c);
            Swap(p, hi);
            int i = lo - 1, j = hi;
            while (true)
            {
                while (c.Compare(_data[++i], _data[hi]) < 0) { }
                while (c.Compare(_data[--j], _data[hi]) > 0 && j > lo) { }
                if (i >= j) break;
                Swap(i, j);
            }
            Swap(i, hi);
            if (lo < i - 1) QuickSort(lo, i - 1, c);
            if (i + 1 < hi) QuickSort(i + 1, hi, c);
        }

        private void InsertionSort(int lo, int hi, IComparer<T> c)
        {
            for (int i = lo + 1; i <= hi; i++)
            {
                T k = _data[i]; int j = i - 1;
                while (j >= lo && c.Compare(_data[j], k) > 0) _data[j + 1] = _data[j--];
                _data[j + 1] = k;
            }
        }

        private int MedianOfThree(int lo, int hi, IComparer<T> c)
        {
            int m = lo + (hi - lo >> 1);
            if (c.Compare(_data[lo], _data[m]) > 0) Swap(lo, m);
            if (c.Compare(_data[lo], _data[hi]) > 0) Swap(lo, hi);
            if (c.Compare(_data[m], _data[hi]) > 0) Swap(m, hi);
            return m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int j) => (_data[i], _data[j]) = (_data[j], _data[i]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        public struct Enumerator
        {
            private readonly LuminArray<T> _a;
            
            private int _i;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(LuminArray<T> a) { _a = a; _i = -1; }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++_i < _a._length;
            
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _a._data[_i];
            }
        }
    }

    internal static unsafe class LuminArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminArray<TTo> ToLuminArray<TTo>(this Span<TTo> span) where TTo : unmanaged
            => new((TTo*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);
    }
}