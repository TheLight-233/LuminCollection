using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminBitArray : IDisposable
    {
        private UnsafeCollection.LuminBitArray _bitArray;

        public int Length => _bitArray.Length;
        public int Capacity => _bitArray.Capacity;
        public bool IsCreated => _bitArray.IsCreated;

        public ref bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range [0, {Length - 1}]");
                
                return ref _bitArray[index];
            }
        }
        
        public ref bool this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
                if (!IsCreated) 
                    throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
                if (index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range [0, {Length - 1}]");
                
                return ref _bitArray[index];
            }
        }

        #region Ctor
        public LuminBitArray(int length, bool defaultValue = false)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero");
            
            _bitArray = new UnsafeCollection.LuminBitArray(length, defaultValue);
        }

        public LuminBitArray(scoped in bool[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            
            _bitArray = new UnsafeCollection.LuminBitArray(values);
        }

        public LuminBitArray(scoped in LuminBitArray source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (!source.IsCreated)
                throw new ObjectDisposedException(nameof(LuminBitArray));
            
            _bitArray = new UnsafeCollection.LuminBitArray(in source._bitArray);
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(bool value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            _bitArray.SetAll(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range [0, {Length - 1}]");
            
            _bitArray[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range [0, {Length - 1}]");
            
            return _bitArray[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRange(int index, int count, bool value)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (index + count > Length)
                throw new ArgumentException("Invalid range");
            
            _bitArray.SetRange(index, count, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flip(int index)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range [0, {Length - 1}]");
            
            _bitArray.Flip(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FlipAll()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            _bitArray.FlipAll();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountTrue()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return _bitArray.CountTrue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountFalse()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return _bitArray.CountFalse();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return _bitArray.Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool All()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return _bitArray.All();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool None()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return _bitArray.None();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void And(LuminBitArray other)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (other is null)
                throw new ArgumentNullException(nameof(other));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!other.IsCreated, other);
#else
            if (!other.IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (other.Length != Length)
                throw new ArgumentException("Bit arrays must be of the same length", nameof(other));
            
            _bitArray.And(other._bitArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Or(LuminBitArray other)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (other is null)
                throw new ArgumentNullException(nameof(other));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!other.IsCreated, other);
#else
            if (!other.IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (other.Length != Length)
                throw new ArgumentException("Bit arrays must be of the same length", nameof(other));
            
            _bitArray.Or(other._bitArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Xor(LuminBitArray other)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (other is null)
                throw new ArgumentNullException(nameof(other));
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!other.IsCreated, other);
#else
            if (!other.IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (other.Length != Length)
                throw new ArgumentException("Bit arrays must be of the same length", nameof(other));
            
            _bitArray.Xor(other._bitArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Not()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            _bitArray.Not();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(scoped in bool[] array, int arrayIndex)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + Length > array.Length)
                throw new ArgumentException("Destination array is not long enough");
            
            _bitArray.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool[] ToArray()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return _bitArray.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            _bitArray.EnsureCapacity(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            _bitArray.TrimExcess();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<bool> AsSpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return _bitArray.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<bool> AsReadOnlySpan()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return _bitArray.AsReadOnlySpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<bool> AsSpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            return _bitArray.AsSpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<bool> AsSpan(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (start < 0 || length < 0 || start + length > Length)
                throw new ArgumentOutOfRangeException();
            
            return _bitArray.AsSpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<bool> AsReadOnlySpan(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            return _bitArray.AsReadOnlySpan(start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<bool> AsReadOnlySpan(int start, int length)
        {
#if NET8_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (start < 0 || length < 0 || start + length > Length)
                throw new ArgumentOutOfRangeException();
            
            return _bitArray.AsReadOnlySpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminBitArray Slice(int start)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (start < 0 || start > Length)
                throw new ArgumentOutOfRangeException(nameof(start));

            var slice = Activator.CreateInstance<LuminBitArray>();
            slice._bitArray = _bitArray.Slice(start);
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuminBitArray Slice(int start, int length)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            if (start < 0 || length < 0 || start + length > Length)
                throw new ArgumentOutOfRangeException();
            
            var slice = Activator.CreateInstance<LuminBitArray>();
            slice._bitArray = _bitArray.Slice(start, length);
            return slice;
        }

        public static LuminBitArray Create(scoped in Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            var ba = Activator.CreateInstance<LuminBitArray>();
            ba._bitArray = UnsafeCollection.LuminBitArray.Create(buffer, alignment, out byteOffset);
            return ba;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuminBitArray Create(scoped in Span<byte> buffer)
            => Create(buffer, (uint)LuminMemoryHelper.AlignOf<bool>(), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated) 
                throw new ObjectDisposedException(nameof(LuminBitArray));
#endif
            
            return new Enumerator(_bitArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
            {
                _bitArray.Dispose();
            }
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private UnsafeCollection.LuminBitArray.Enumerator _e;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)] 
            internal Enumerator(UnsafeCollection.LuminBitArray ba) => _e = ba.GetEnumerator();
            
            public ref bool Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _e.Current;
            }

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