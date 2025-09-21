using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LuminBitArray : IDisposable
    {
        private bool* _bits;
        private int _length;
        private int _capacity; // bool 个数

        public int Length => _length;
        public int Capacity => _capacity;
        public bool IsCreated => _bits != null;

        public ref bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<bool>(_bits), (nint)index);
        }
        
        public ref bool this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<bool>(_bits), (nint)index);
        }

        #region Ctor
        public LuminBitArray(int length, bool defaultValue = false)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            _length = length;
            _capacity = length;
            nuint align = LuminMemoryHelper.AlignOf<bool>();
            var size = _capacity * sizeof(bool);
            _bits = (bool*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            
            Unsafe.InitBlock(_bits, (byte)(defaultValue ? 1 : 0), (uint)(_capacity * sizeof(bool)));
        }

        public LuminBitArray(scoped in bool[]? values)
        {
            if (values is null || values.Length == 0)
            { _bits = null; _length = 0; _capacity = 0; return; }

            _length = values.Length;
            _capacity = _length;
            nuint align = LuminMemoryHelper.AlignOf<bool>();
            var size = _capacity * sizeof(bool);
            _bits = (bool*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            
            fixed (bool* source = values)
            {
                Unsafe.CopyBlock(_bits, source, (uint)(_length * sizeof(bool)));
            }
        }

        public LuminBitArray(scoped in LuminBitArray src)
        {
            if (!src.IsCreated || src._length == 0)
            { _bits = null; _length = 0; _capacity = 0; return; }

            _length = src._length;
            _capacity = src._capacity;
            nuint align = LuminMemoryHelper.AlignOf<bool>();
            var size = _capacity * sizeof(bool);
            _bits = (bool*)LuminAllocator.AlignedAlloc(align, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            Unsafe.CopyBlock(_bits, src._bits, (uint)(_capacity * sizeof(bool)));
        }
        #endregion
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(bool value)
        {
            Unsafe.InitBlock(_bits, (byte)(value ? 1 : 0), (uint)(_capacity * sizeof(bool)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value) => _bits[index] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index) => _bits[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRange(int index, int count, bool value)
        {
            for (int i = index; i < index + count; i++)
                _bits[i] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flip(int index) => _bits[index] = !_bits[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FlipAll()
        {
            for (int i = 0; i < _length; i++)
                _bits[i] = !_bits[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountTrue()
        {
            int cnt = 0;
            for (int i = 0; i < _length; i++)
                if (_bits[i]) cnt++;
            return cnt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountFalse() => _length - CountTrue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any()
        {
            for (int i = 0; i < _length; i++)
                if (_bits[i]) return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool All()
        {
            for (int i = 0; i < _length; i++)
                if (!_bits[i]) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool None() => !Any();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void And(scoped in LuminBitArray other)
        {
            for (int i = 0; i < _length; i++)
                _bits[i] = _bits[i] && other._bits[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Or(scoped in LuminBitArray other)
        {
            for (int i = 0; i < _length; i++)
                _bits[i] = _bits[i] || other._bits[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Xor(scoped in LuminBitArray other)
        {
            for (int i = 0; i < _length; i++)
                _bits[i] = _bits[i] != other._bits[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Not() => FlipAll();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in bool[] dest, int destIndex)
        {
            for (int i = 0; i < _length; i++)
                dest[destIndex + i] = _bits[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool[] ToArray()
        {
            if (_length == 0) return [];
#if NET8_0_OR_GREATER
            var arr = GC.AllocateUninitializedArray<bool>(_length);
#else
            bool[] arr = new bool[_length];
#endif
            for (int i = 0; i < _length; i++) arr[i] = _bits[i];
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<bool> AsSpan() => new Span<bool>(_bits, _length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<bool> AsReadOnlySpan() => new ReadOnlySpan<bool>(_bits, _length);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<bool> AsSpan(int start) => new Span<bool>(_bits + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<bool> AsSpan(int start, int length) => new Span<bool>(_bits + start, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<bool> AsReadOnlySpan(int start) => new ReadOnlySpan<bool>(_bits + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<bool> AsReadOnlySpan(int start, int length) => new ReadOnlySpan<bool>(_bits + start, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminBitArray Slice(int start)
        {
            if (start < 0 || start > _length) throw new ArgumentOutOfRangeException(nameof(start));
            int len = _length - start;
            var slice = new LuminBitArray(len);
            for (int i = 0; i < len; i++) slice[i] = _bits[start + i];
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminBitArray Slice(int start, int length)
        {
            if (start < 0 || length < 0 || start + length > _length) throw new ArgumentOutOfRangeException();
            var slice = new LuminBitArray(length);
            for (int i = 0; i < length; i++) slice[i] = _bits[start + i];
            return slice;
        }

        public static LuminBitArray Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            ref byte b = ref MemoryMarshal.GetReference(buffer);
            nint ptr = (nint)Unsafe.AsPointer(ref b);
            nint aligned = (nint)LuminMemoryHelper.AlignUp((nuint)ptr, alignment);
            byteOffset = aligned - ptr;
            
            int boolCount = (buffer.Length - (int)byteOffset) / sizeof(bool);
            var ba = new LuminBitArray(boolCount);
            
            fixed (byte* source = buffer.Slice((int)byteOffset))
            {
                Unsafe.CopyBlock(ba._bits, source, (uint)(boolCount * sizeof(bool)));
            }
            
            return ba;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminBitArray Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<bool>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        public void EnsureCapacity(int capacity)
        {
            if (capacity > _capacity) SetCapacity(capacity);
        }

        public void TrimExcess()
        {
            if (_length < _capacity) SetCapacity(_length);
        }

        private void SetCapacity(int newCap)
        {
            if (newCap == _capacity) return;

            nuint align = LuminMemoryHelper.AlignOf<bool>();
            var size = newCap * sizeof(bool);
            bool* newBits = newCap > 0 ? 
                (bool*)LuminAllocator.AlignedAlloc(align, (nuint)size) : 
                null;
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
                
            int copy = Math.Min(_length, newCap);
            if (copy > 0) 
                Unsafe.CopyBlock(newBits, _bits, (uint)(copy * sizeof(bool)));
                
            if (_bits != null)
            {
                LuminAllocator.FreeAligned(_bits, align);
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(bool));
#endif
            }
            
            _bits = newBits; 
            _capacity = newCap;
            _length = Math.Min(_length, newCap);
        }

        public void Dispose()
        {
            if (_bits != null)
            {
                LuminAllocator.FreeAligned(_bits, LuminMemoryHelper.AlignOf<bool>());
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(bool));
#endif
                _bits = null; 
                _length = 0; 
                _capacity = 0;
            }
        }

        #region Enumerator
        public struct Enumerator
        {
            private LuminBitArray _ba;
            private int _idx;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)] 
            internal Enumerator(LuminBitArray ba) { _ba = ba; _idx = -1; }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)] 
            public bool MoveNext() => ++_idx < _ba._length;
            
            public ref bool Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _ba._bits[_idx];
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)] 
            public void Reset() => _idx = -1;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)] 
            public void Dispose() { }
        }
        #endregion
    }
}