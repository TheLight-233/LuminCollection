using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LuminMemoryAllocator;

namespace LuminCollection.UnsafeCollection;

[Intrinsic]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LuminLinkedList<T> : IDisposable where T : unmanaged
{
    private struct Node
    {
        public Node* Prev;
        public Node* Next;
        public T Data;
    }

    private Node* _head;
    private Node* _tail;
    private int _count;

    public int Count => _count;
    public bool IsEmpty => _count == 0;
    
    public bool IsCreated => _head != null;

    public LuminLinkedList()
    {
        _head = null;
        _tail = null;
        _count = 0;
    }

    public void AddFirst(T value)
    {
        Node* newNode = AllocateNode(value);
            
        if (_head == null)
        {
            _head = _tail = newNode;
        }
        else
        {
            newNode->Next = _head;
            _head->Prev = newNode;
            _head = newNode;
        }
        
        _count++;
    }

    public void AddLast(T value)
    {
        Node* newNode = AllocateNode(value);
            
        if (_tail == null)
        {
            _head = _tail = newNode;
        }
        else
        {
            newNode->Prev = _tail;
            _tail->Next = newNode;
            _tail = newNode;
        }
            
        _count++;
    }

    public void RemoveFirst()
    {
        if (_head == null)
            throw new InvalidOperationException("The list is empty");
            
        Node* toRemove = _head;
        _head = _head->Next;
            
        if (_head != null)
            _head->Prev = null;
        else
            _tail = null;
            
        FreeNode(toRemove);
        _count--;
    }

    public void RemoveLast()
    {
        if (_tail == null)
            throw new InvalidOperationException("The list is empty");
            
        Node* toRemove = _tail;
        _tail = _tail->Prev;
            
        if (_tail != null)
            _tail->Next = null;
        else
            _head = null;
            
        FreeNode(toRemove);
        _count--;
    }

    public void Clear()
    {
        Node* current = _head;
        while (current != null)
        {
            Node* next = current->Next;
            FreeNode(current);
            current = next;
        }
            
        _head = _tail = null;
        _count = 0;
    }

    public ref T First
    {
        get
        {
            if (_head is null)
                throw new InvalidOperationException("The list is empty");
            
            return ref _head->Data;
        }
    }
    
    public ref T Last
    {
        get
        {
            if (_tail is null)
                throw new InvalidOperationException("The list is empty");
            
            return ref _tail->Data;
        }
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new Enumerator(this);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Node* AllocateNode(T value)
    {
        nuint alignment = LuminMemoryHelper.AlignOf<Node>();
        var byteCount = sizeof(Node);
        Node* node = (Node*)LuminAllocator.AlignedAlloc(alignment, (nuint)byteCount);
#if DEBUG
        LuminMemoryHelper.AddMemoryPressure(byteCount);
#endif
        node->Prev = null;
        node->Next = null;
        node->Data = value;
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FreeNode(Node* node)
    {
        LuminAllocator.FreeAligned(node, (nuint)sizeof(Node));
#if DEBUG
        LuminMemoryHelper.RemoveMemoryPressure(sizeof(Node));
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => Clear();

    public struct Enumerator
    {
        private readonly LuminLinkedList<T> _list;
        private Node* _current;
        private bool _started;

        internal Enumerator(LuminLinkedList<T> list)
        {
            _list = list;
            _current = null;
            _started = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (!_started)
            {
                _current = _list._head;
                _started = true;
            }
            else
            {
                _current = _current->Next;
            }

            return _current != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _current = null;
            _started = false;
        }

        public ref T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _current->Data;
        }

        public void Dispose() { }
    }
}