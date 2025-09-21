using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection
{
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LuminConcurrentQueue<T> : IDisposable where T : unmanaged
    {
        private const int SEGMENT_SIZE = 32;

        [StructLayout(LayoutKind.Sequential)]
        private struct Segment
        {
            public T* Items;
            public IntPtr Next;
            public int Low;
            public int High;
            public int Sequence;
        }

        private IntPtr _head;
        private IntPtr _tail;
        private int _count;
        private int _padding;
        private nuint _alignment;
        private int _elementSize;

        public int Count => Volatile.Read(ref _count);
        public bool IsCreated => _head != IntPtr.Zero;

        #region Constructor & Dispose
        public LuminConcurrentQueue(int capacity = 0)
        {
            _head = IntPtr.Zero;
            _tail = IntPtr.Zero;
            _count = 0;
            _padding = 0;
            _alignment = LuminMemoryHelper.AlignOf<T>();
            _elementSize = sizeof(T);

            if (capacity > 0)
            {
                Segment* segment = AllocateSegment();
                _head = (IntPtr)segment;
                _tail = (IntPtr)segment;
            }
        }

        public void Dispose()
        {
            Segment* current = (Segment*)_head;
            while (current != null)
            {
                Segment* next = (Segment*)current->Next;
                FreeSegment(current);
                current = next;
            }

            _head = IntPtr.Zero;
            _tail = IntPtr.Zero;
            _count = 0;
        }
        #endregion

        #region Segment Management
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Segment* AllocateSegment()
        {
            nuint segmentSize = (nuint)sizeof(Segment);
            nuint itemsSize = (nuint)(SEGMENT_SIZE * _elementSize);
            
            Segment* segment = (Segment*)LuminAllocator.Alloc(segmentSize);
            segment->Items = (T*)LuminAllocator.AlignedAlloc(_alignment, itemsSize);
            segment->Next = IntPtr.Zero;
            segment->Low = 0;
            segment->High = 0;
            segment->Sequence = 0;
            
            return segment;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FreeSegment(Segment* segment)
        {
            if (segment->Items != null)
            {
                LuminAllocator.FreeAligned(segment->Items, _alignment);
            }
            LuminAllocator.Free(segment);
        }
        #endregion

        #region Memory Operations for Unmanaged Types
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteValue(T* destination, T value)
        {
            // 使用内存复制来写入值，确保正确的内存语义
            Unsafe.WriteUnaligned(destination, value);
            // 插入内存屏障确保写入对其他线程可见
            Interlocked.MemoryBarrier();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T ReadValue(T* source)
        {
            // 插入内存屏障确保读取最新值
            Interlocked.MemoryBarrier();
            // 使用内存复制来读取值
            return Unsafe.ReadUnaligned<T>(source);
        }
        #endregion

        #region Enqueue Operations
        public void Enqueue(T item)
        {
            while (true)
            {
                Segment* tail = (Segment*)Volatile.Read(ref _tail);
                int high = Volatile.Read(ref tail->High);
                
                if (high >= SEGMENT_SIZE)
                {
                    if (Volatile.Read(ref tail->Next) == IntPtr.Zero)
                    {
                        Segment* newSegment = AllocateSegment();
                        newSegment->High = 0;
                        
                        IntPtr expected = IntPtr.Zero;
                        IntPtr desired = (IntPtr)newSegment;
                        
                        if (Interlocked.CompareExchange(ref tail->Next, desired, expected) == expected)
                        {
                            Interlocked.CompareExchange(ref _tail, desired, (IntPtr)tail);
                            continue;
                        }
                        else
                        {
                            FreeSegment(newSegment);
                        }
                    }
                    
                    Interlocked.CompareExchange(ref _tail, tail->Next, (IntPtr)tail);
                    continue;
                }
                
                if (Interlocked.CompareExchange(ref tail->High, high + 1, high) == high)
                {
                    WriteValue(&tail->Items[high], item);
                    Interlocked.Increment(ref _count);
                    return;
                }
            }
        }

        public bool TryEnqueue(T item)
        {
            Segment* tail = (Segment*)Volatile.Read(ref _tail);
            int high = Volatile.Read(ref tail->High);
            
            if (high >= SEGMENT_SIZE)
            {
                if (Volatile.Read(ref tail->Next) == IntPtr.Zero)
                {
                    Segment* newSegment = AllocateSegment();
                    newSegment->High = 0;
                    
                    IntPtr expected = IntPtr.Zero;
                    IntPtr desired = (IntPtr)newSegment;
                    
                    if (Interlocked.CompareExchange(ref tail->Next, desired, expected) == expected)
                    {
                        Interlocked.CompareExchange(ref _tail, desired, (IntPtr)tail);
                        tail = newSegment;
                        high = 0;
                    }
                    else
                    {
                        FreeSegment(newSegment);
                        return false;
                    }
                }
                else
                {
                    Interlocked.CompareExchange(ref _tail, tail->Next, (IntPtr)tail);
                    return false;
                }
            }
            
            if (Interlocked.CompareExchange(ref tail->High, high + 1, high) == high)
            {
                WriteValue(&tail->Items[high], item);
                Interlocked.Increment(ref _count);
                return true;
            }
            
            return false;
        }
        #endregion

        #region Dequeue Operations
        public bool TryDequeue(out T result)
        {
            result = default;
            
            while (true)
            {
                Segment* head = (Segment*)Volatile.Read(ref _head);
                int low = Volatile.Read(ref head->Low);
                int high = Volatile.Read(ref head->High);
                
                if (low >= high)
                {
                    if (Volatile.Read(ref head->Next) == IntPtr.Zero)
                    {
                        return false;
                    }
                    
                    if (Interlocked.CompareExchange(ref _head, head->Next, (IntPtr)head) == (IntPtr)head)
                    {
                        FreeSegment(head);
                    }
                    
                    continue;
                }
                
                if (Interlocked.CompareExchange(ref head->Low, low + 1, low) == low)
                {
                    result = ReadValue(&head->Items[low]);
                    Interlocked.Decrement(ref _count);
                    return true;
                }
            }
        }

        public bool TryPeek(out T result)
        {
            result = default;
            
            while (true)
            {
                Segment* head = (Segment*)Volatile.Read(ref _head);
                int low = Volatile.Read(ref head->Low);
                int high = Volatile.Read(ref head->High);
                
                if (low >= high)
                {
                    if (Volatile.Read(ref head->Next) == IntPtr.Zero)
                    {
                        return false;
                    }
                    
                    head = (Segment*)head->Next;
                    low = Volatile.Read(ref head->Low);
                    high = Volatile.Read(ref head->High);
                    
                    if (low >= high)
                    {
                        return false;
                    }
                }
                
                result = ReadValue(&head->Items[low]);
                
                if (head == (Segment*)Volatile.Read(ref _head) && 
                    low == Volatile.Read(ref head->Low) && 
                    high == Volatile.Read(ref head->High))
                {
                    return true;
                }
            }
        }
        #endregion

        #region Additional Operations
        public void Clear()
        {
            Segment* current = (Segment*)_head;
            while (current != null)
            {
                Segment* next = (Segment*)current->Next;
                FreeSegment(current);
                current = next;
            }
            
            _head = IntPtr.Zero;
            _tail = IntPtr.Zero;
            _count = 0;
            
            Segment* segment = AllocateSegment();
            _head = (IntPtr)segment;
            _tail = (IntPtr)segment;
        }

        public bool IsEmpty
        {
            get
            {
                Segment* head = (Segment*)Volatile.Read(ref _head);
                return Volatile.Read(ref head->Low) >= Volatile.Read(ref head->High) && 
                       Volatile.Read(ref head->Next) == IntPtr.Zero;
            }
        }

        public T[] ToArray()
        {
            int count = Volatile.Read(ref _count);
            T[] result = new T[count];
            int index = 0;
            
            Segment* current = (Segment*)Volatile.Read(ref _head);
            while (current != null)
            {
                int low = Volatile.Read(ref current->Low);
                int high = Volatile.Read(ref current->High);
                
                for (int i = low; i < high; i++)
                {
                    if (index < result.Length)
                    {
                        result[index++] = ReadValue(&current->Items[i]);
                    }
                }
                
                current = (Segment*)Volatile.Read(ref current->Next);
            }
            
            return result;
        }
        #endregion
    }
}