using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection
{
    using static HashHelpers;
    
    [Intrinsic]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LuminPriorityQueue<TKey, TValue> : IDisposable
        where TKey : unmanaged, IComparable<TKey>
        where TValue : unmanaged
    {
        private struct Item
        {
            public TKey Priority;
            public TValue Value;
        }

        private Item* _items;
        private int _count;
        private int _capacity;

        public int Count => _count;
        public int Capacity => _capacity;
        public bool IsCreated => _items != null;

        #region Ctor
        
        public LuminPriorityQueue()
        {
            _capacity = GetPrime(0);;
            _count = 0;

            nuint itemAlign = LuminMemoryHelper.AlignOf<Item>();
            var size = _capacity * sizeof(Item);
            _items = (Item*)LuminAllocator.AlignedAlloc(itemAlign, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
            
        }
        
        public LuminPriorityQueue(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            
            _capacity = GetPrime(capacity);
            _count = 0;

            nuint itemAlign = LuminMemoryHelper.AlignOf<Item>();
            var size = _capacity * sizeof(Item);
            _items = (Item*)LuminAllocator.AlignedAlloc(itemAlign, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif
        }

        public LuminPriorityQueue(scoped in LuminPriorityQueue<TKey, TValue> src)
        {
            if (!src.IsCreated || src._capacity == 0)
            {
                _items = null;
                _count = 0;
                _capacity = 0;
                return;
            }

            _capacity = src._capacity;
            _count = src._count;

            nuint itemAlign = LuminMemoryHelper.AlignOf<Item>();
            var size = _capacity * sizeof(Item);
            _items = (Item*)LuminAllocator.AlignedAlloc(itemAlign, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif

            Unsafe.CopyBlock(_items, src._items, (uint)(_count * sizeof(Item)));
        }
        #endregion

        #region Basic API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(scoped in TValue value, scoped in TKey priority)
        {
            if (_count == _capacity)
            {
                Resize();
            }

            _items[_count].Priority = priority;
            _items[_count].Value = value;
            _count++;

            HeapifyUp(_count - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Dequeue()
        {
            return TryDequeue(out var value) ? value : default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TValue value)
        {
            if (_count == 0)
            {
                value = default;
                return false;
            }
            
            value = _items[0].Value;

            _count--;
            if (_count > 0)
            {
                _items[0] = _items[_count];
                HeapifyDown(0);
            }
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out TValue value)
        {
            if (_count == 0)
            {
                value = default;
                return false;
            } 
            
            value = _items[0].Value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
            if (capacity > _capacity)
            {
                Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            int newCapacity = GetPrime(_count);
            if (newCapacity < _capacity)
            {
                Resize(newCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(this);
        #endregion

        #region Helpers
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize() => Resize(GetPrime(_capacity * 2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(int newCapacity)
        {
            if (newCapacity < _capacity) throw new ArgumentOutOfRangeException(nameof(newCapacity));

            nuint itemAlign = LuminMemoryHelper.AlignOf<Item>();
            var size = newCapacity * sizeof(Item);
            var newItems = (Item*)LuminAllocator.AlignedAlloc(itemAlign, (nuint)size);
#if DEBUG
            LuminMemoryHelper.AddMemoryPressure(size);
#endif

            if (_count > 0)
            {
                Unsafe.CopyBlock(newItems, _items, (uint)(_count * sizeof(Item)));
            }

            if (_items != null)
            {
                LuminAllocator.FreeAligned(_items, itemAlign);
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(Item));
#endif
            }

            _items = newItems;
            _capacity = newCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) >> 2;
                if (_items[index].Priority.CompareTo(_items[parent].Priority) >= 0)
                    break;

                Swap(index, parent);
                index = parent;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HeapifyDown(int index)
        {
            while (true)
            {
                int firstChild = (index << 2) + 1;
                if (firstChild >= _count) break;

                int minChild = firstChild;
                int lastChild = Math.Min(firstChild + 4, _count);
                
                for (int i = firstChild + 1; i < lastChild; i++)
                {
                    if (_items[i].Priority.CompareTo(_items[minChild].Priority) < 0)
                        minChild = i;
                }

                if (_items[minChild].Priority.CompareTo(_items[index].Priority) >= 0)
                    break;

                Swap(index, minChild);
                index = minChild;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int j) => (_items[i], _items[j]) = (_items[j], _items[i]);

        public void Dispose()
        {
            if (_items != null)
            {
                LuminAllocator.FreeAligned(_items, LuminMemoryHelper.AlignOf<Item>());
#if DEBUG
                LuminMemoryHelper.RemoveMemoryPressure(_capacity * sizeof(Item));
#endif
                _items = null;
            }
            
            _count = 0;
            _capacity = 0;
        }
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private readonly LuminPriorityQueue<TKey, TValue> _priorityQueue;
            private int _index;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(LuminPriorityQueue<TKey, TValue> priorityQueue)
            {
                _priorityQueue = priorityQueue;
                _index = -1;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                _index++;
                return _index < _priorityQueue._count;
            }
            
            public KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new KeyValuePair<TKey, TValue>(
                    _priorityQueue._items[_index].Priority,
                    _priorityQueue._items[_index].Value);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _index = -1;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }
        #endregion
    }
}
