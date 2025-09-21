using System.Collections;
using System.Runtime.CompilerServices;

namespace LuminCollection
{
    public sealed class LuminLinkedList<T> : IDisposable where T : unmanaged
    {
        private UnsafeCollection.LuminLinkedList<T> _list;

        public int Count => _list.Count;
        
        public bool IsEmpty => _list.IsEmpty;
        
        public ref T First => ref _list.First;
        
        public ref T Last => ref _list.Last;
        
        public bool IsCreated => _list.IsCreated;

        public LuminLinkedList()
        {
            _list = new UnsafeCollection.LuminLinkedList<T>();
        }

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCreated()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(!IsCreated, this);
#else
            if (!IsCreated)
                throw new ObjectDisposedException(nameof(LuminLinkedList<T>));
#endif
        }
        #endregion

        #region API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFirst(T value) { EnsureCreated(); _list.AddFirst(value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLast(T value) { EnsureCreated(); _list.AddLast(value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveFirst() { EnsureCreated(); _list.RemoveFirst(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLast() { EnsureCreated(); _list.RemoveLast(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { EnsureCreated(); _list.Clear(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() { EnsureCreated(); return new Enumerator(ref _list); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Clear();
        #endregion

        #region Enumerator
        public struct Enumerator
        {
            private UnsafeCollection.LuminLinkedList<T>.Enumerator _enumerator;
            public Enumerator(ref UnsafeCollection.LuminLinkedList<T> list) => _enumerator = list.GetEnumerator();
            
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _enumerator.Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _enumerator.MoveNext();
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _enumerator.Reset();
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _enumerator.Dispose();
        }
        #endregion
    }
}