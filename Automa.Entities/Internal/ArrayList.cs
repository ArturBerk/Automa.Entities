using System;
using System.Collections;
using System.Collections.Generic;

namespace Automa.Entities.Internal
{
    internal class ArrayList<T> : IEnumerable<T>
    {
        private const int MinSize = 4;

        private T[] buffer;
        public int Count;

        public ArrayList(int initialSize = MinSize)
        {
            Count = 0;
            buffer = new T[initialSize];
        }

        public ArrayList(ICollection<T> collection)
        {
            buffer = new T[collection.Count];

            collection.CopyTo(buffer, 0);

            Count = buffer.Length;
        }

        public ArrayList(IList<T> listCopy)
        {
            buffer = new T[listCopy.Count];

            listCopy.CopyTo(buffer, 0);

            Count = listCopy.Count;
        }

        public ref T this[int i] => ref buffer[i];

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ref T Get(int index)
        {
            return ref buffer[index];
        }

        public void SetAt(int index, T value)
        {
            if (buffer.Length <= index) AllocateMore(index + 1);
            if (Count <= index) Count = index + 1;
            buffer[index] = value;
        }

        public void Add(T item)
        {
            if (Count == buffer.Length)
                AllocateMore();

            buffer[Count++] = item;
        }

        public void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);

            Count = 0;
        }

        public bool Contains(T item)
        {
            var index = IndexOf(item);

            return index != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(buffer, 0, array, arrayIndex, Count);
        }

        public int IndexOf(T item)
        {
            var comp = EqualityComparer<T>.Default;

            for (var index = Count - 1; index >= 0; --index)
                if (comp.Equals(buffer[index], item))
                    return index;

            return -1;
        }

        public void Insert(int index, T item)
        {
            if (Count == buffer.Length) AllocateMore();

            Array.Copy(buffer, index, buffer, index + 1, Count - index);

            buffer[index] = item;
            ++Count;
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);

            if (index == -1)
                return false;

            RemoveAt(index);

            return true;
        }

        public void RemoveAt(int index)
        {
            if (index == --Count)
                return;

            Array.Copy(buffer, index + 1, buffer, index, Count - index);

            buffer[Count] = default(T);
        }

        public void AddRange(IEnumerable<T> items, int count)
        {
            AddRange(items.GetEnumerator(), count);
        }

        public void AddRange(IEnumerator<T> items, int count)
        {
            if (Count + count >= buffer.Length)
                AllocateMore(Count + count);

            while (items.MoveNext())
                buffer[Count++] = items.Current;
        }

        public void AddRange(ICollection<T> items)
        {
            AddRange(items.GetEnumerator(), items.Count);
        }

        public void AddRange(ArrayList<T> items)
        {
            AddRange(items.buffer, items.Count);
        }

        public void AddRange(T[] items, int count)
        {
            if (count == 0) return;

            if (Count + count >= buffer.Length)
                AllocateMore(Count + count);

            Array.Copy(items, 0, buffer, Count, count);
            Count += count;
        }

        public void AddRange(T[] items)
        {
            AddRange(items, items.Length);
        }

        /// <summary>
        ///     Careful, you could keep on holding references you don't want to hold to anymore
        ///     Use DeepClear in case.
        /// </summary>
        public void FastClear()
        {
            Count = 0;
        }

        public EcsListEnumerator<T> GetEnumerator()
        {
            return new EcsListEnumerator<T>(buffer, Count);
        }

        public void Release()
        {
            Count = 0;
            buffer = null;
        }

        public void Resize(int newSize)
        {
            if (newSize < MinSize)
                newSize = MinSize;

            Array.Resize(ref buffer, newSize);

            Count = newSize;
        }

        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(buffer, 0, Count, comparer);
        }

        public bool UnorderedRemove(T item)
        {
            var index = IndexOf(item);

            if (index == -1)
                return false;

            UnorderedRemoveAt(index);

            return true;
        }

        public bool UnorderedRemoveAt(int index)
        {
            if (index == --Count)
            {
                buffer[Count] = default(T);
                return false;
            }

            buffer[index] = buffer[Count];
            buffer[Count] = default(T);

            return true;
        }

        private void AllocateMore()
        {
            var newList = new T[Math.Max(buffer.Length << 1, MinSize)];
            if (Count > 0) buffer.CopyTo(newList, 0);
            buffer = newList;
        }

        private void AllocateMore(int newSize)
        {
            var oldLength = Math.Max(buffer.Length, MinSize);

            while (oldLength < newSize)
                oldLength <<= 1;

            var newList = new T[oldLength];
            if (Count > 0) Array.Copy(buffer, newList, Count);
            buffer = newList;
        }

        public void Trim()
        {
            if (Count < buffer.Length)
                Resize(Count);
        }
    }

    public struct EcsListEnumerator<T> : IEnumerator<T>
    {
        public T Current { get; private set; }

        public EcsListEnumerator(T[] buffer, int size)
        {
            this.size = size;
            counter = 0;
            this.buffer = buffer;
            Current = default(T);
        }

        object IEnumerator.Current => Current;

        T IEnumerator<T>.Current => Current;

        public void Dispose()
        {
            buffer = null;
        }

        public bool MoveNext()
        {
            if (counter < size)
            {
                Current = buffer[counter++];

                return true;
            }

            Current = default(T);

            return false;
        }

        public void Reset()
        {
            counter = 0;
        }

        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void IEnumerator.Reset()
        {
            Reset();
        }

        private T[] buffer;
        private int counter;
        private readonly int size;
    }
}