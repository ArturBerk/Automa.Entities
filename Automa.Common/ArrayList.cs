using System;
using System.Collections;
using System.Collections.Generic;
using Automa.Entities.Internal;

namespace Automa.Common
{
    internal struct ArrayList<T> : IEnumerable<T>
    {
        private const int MinSize = 4;

        public T[] Buffer;
        public int Count;

        public ArrayList(int initialSize)
        {
            Count = 0;
            Buffer = new T[initialSize];
        }

        public ArrayList(ICollection<T> collection)
        {
            Buffer = new T[collection.Count];

            collection.CopyTo(Buffer, 0);

            Count = Buffer.Length;
        }

        public ArrayList(IList<T> listCopy)
        {
            Buffer = new T[listCopy.Count];

            listCopy.CopyTo(Buffer, 0);

            Count = listCopy.Count;
        }

        public ref T this[int i] => ref Buffer[i];

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SetAt(int index, T value)
        {
            if (Buffer.Length <= index) AllocateMore(index + 1);
            if (Count <= index) Count = index + 1;
            Buffer[index] = value;
        }

        public void Add(T item)
        {
            if (Count == Buffer.Length)
                AllocateMore();

            Buffer[Count++] = item;
        }

        public void Add(ref T item)
        {
            if (Count == Buffer.Length)
                AllocateMore();

            Buffer[Count++] = item;
        }

        public void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);

            Count = 0;
        }

        public bool Contains(T item)
        {
            var index = IndexOf(item);

            return index != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(Buffer, 0, array, arrayIndex, Count);
        }

        public int IndexOf(T item)
        {
            var comp = EqualityComparer<T>.Default;

            for (var index = Count - 1; index >= 0; --index)
                if (comp.Equals(Buffer[index], item))
                    return index;

            return -1;
        }

        public void UnorderedRemoveAll(Predicate<T> condition)
        {
            for (int i = 0; i < Count; i++)
            {
                if (condition(Buffer[i]))
                {
                    UnorderedRemoveAt(i);
                }
            }
        }

        public void Insert(int index, T item)
        {
            if (Count == Buffer.Length) AllocateMore();

            Array.Copy(Buffer, index, Buffer, index + 1, Count - index);

            Buffer[index] = item;
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

            Array.Copy(Buffer, index + 1, Buffer, index, Count - index);

            Buffer[Count] = default(T);
        }

        public void AddRange(IEnumerable<T> items, int count)
        {
            AddRange(items.GetEnumerator(), count);
        }

        public void AddRange(IEnumerator<T> items, int count)
        {
            if (Count + count >= Buffer.Length)
                AllocateMore(Count + count);

            while (items.MoveNext())
                Buffer[Count++] = items.Current;
        }

        public void AddRange(ICollection<T> items)
        {
            AddRange(items.GetEnumerator(), items.Count);
        }

        public void AddRange(ref ArrayList<T> items)
        {
            AddRange(items.Buffer, items.Count);
        }

        public void AddRange(T[] items, int count)
        {
            if (count == 0) return;

            if (Count + count >= Buffer.Length)
                AllocateMore(Count + count);

            Array.Copy(items, 0, Buffer, Count, count);
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

        public ArrayEnumerator<T> GetEnumerator()
        {
            return new ArrayEnumerator<T>(Buffer, Count);
        }

        public void Release()
        {
            Count = 0;
            Buffer = null;
        }

        public void Resize(int newSize)
        {
            if (newSize < MinSize)
                newSize = MinSize;

            Array.Resize(ref Buffer, newSize);

            Count = newSize;
        }

        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(Buffer, 0, Count, comparer);
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
                Buffer[Count] = default(T);
                return false;
            }

            Buffer[index] = Buffer[Count];
            Buffer[Count] = default(T);

            return true;
        }

        public void ExpandCount(int newCount)
        {
            if (newCount <= Count) return;
            if (newCount > Buffer.Length)
            {
                AllocateMore(newCount);
            }
            Count = newCount;
        }

        private void AllocateMore()
        {
            var newList = new T[Math.Max(Buffer.Length << 1, MinSize)];
            if (Count > 0) Buffer.CopyTo(newList, 0);
            Buffer = newList;
        }

        private void AllocateMore(int newSize)
        {
            var oldLength = Math.Max(Buffer.Length, MinSize);

            while (oldLength < newSize)
                oldLength <<= 1;

            var newList = new T[oldLength];
            if (Count > 0) Array.Copy(Buffer, newList, Count);
            Buffer = newList;
        }

        public void Trim()
        {
            if (Count < Buffer.Length)
                Resize(Count);
        }
    }
}

namespace Automa.Entities.Internal
{
    public struct ArrayEnumerator<T> : IEnumerator<T>
    {
        public T Current { get; private set; }

        public ArrayEnumerator(T[] buffer, int size)
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