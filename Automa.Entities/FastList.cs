using System;
using System.Collections;
using System.Collections.Generic;

namespace Automa.Collections
{
    public class FastList<T> : IList<T>, IReadOnlyList<T>
    {
        private const int MIN_SIZE = 4;

        private T[] _buffer;

        public FastList()
        {
            Count = 0;

            _buffer = new T[MIN_SIZE];
        }

        public FastList(int initialSize)
        {
            Count = 0;

            _buffer = new T[initialSize];
        }

        public FastList(ICollection<T> collection)
        {
            _buffer = new T[collection.Count];

            collection.CopyTo(_buffer, 0);

            Count = _buffer.Length;
        }

        public FastList(IList<T> listCopy)
        {
            _buffer = new T[listCopy.Count];

            listCopy.CopyTo(_buffer, 0);

            Count = listCopy.Count;
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public T this[int i]
        {
            get
            {
                return _buffer[i];
            }
            set
            {
                _buffer[i] = value;
            }
        }

        public void Add(T item)
        {
            if (Count == _buffer.Length)
                AllocateMore();

            _buffer[Count++] = item;
        }

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);

            Count = 0;
        }

        public bool Contains(T item)
        {
            var index = IndexOf(item);

            return index != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_buffer, 0, array, arrayIndex, Count);
        }

        public int IndexOf(T item)
        {
            var comp = EqualityComparer<T>.Default;

            for (var index = Count - 1; index >= 0; --index)
                if (comp.Equals(_buffer[index], item))
                    return index;

            return -1;
        }

        public void Insert(int index, T item)
        {
            if (Count == _buffer.Length) AllocateMore();

            Array.Copy(_buffer, index, _buffer, index + 1, Count - index);

            _buffer[index] = item;
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

            Array.Copy(_buffer, index + 1, _buffer, index, Count - index);

            _buffer[Count] = default(T);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void AddRange(IEnumerable<T> items, int count)
        {
            AddRange(items.GetEnumerator(), count);
        }

        public void AddRange(IEnumerator<T> items, int count)
        {
            if (Count + count >= _buffer.Length)
                AllocateMore(Count + count);

            while (items.MoveNext())
                _buffer[Count++] = items.Current;
        }

        public void AddRange(ICollection<T> items)
        {
            AddRange(items.GetEnumerator(), items.Count);
        }

        public void AddRange(FastList<T> items)
        {
            AddRange(items.ToArrayFast(), items.Count);
        }

        public void AddRange(T[] items, int count)
        {
            if (count == 0) return;

            if (Count + count >= _buffer.Length)
                AllocateMore(Count + count);

            Array.Copy(items, 0, _buffer, Count, count);
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
            return new EcsListEnumerator<T>(_buffer, Count);
        }

        public void Release()
        {
            Count = 0;
            _buffer = null;
        }

        public void Resize(int newSize)
        {
            if (newSize < MIN_SIZE)
                newSize = MIN_SIZE;

            Array.Resize(ref _buffer, newSize);

            Count = newSize;
        }

        public void SetAt(int index, T value)
        {
            if (index >= _buffer.Length)
                AllocateMore(index + 1);

            if (Count <= index)
                Count = index + 1;

            this[index] = value;
        }

        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(_buffer, 0, Count, comparer);
        }

        public T[] ToArray()
        {
            var destinationArray = new T[Count];

            Array.Copy(_buffer, 0, destinationArray, 0, Count);

            return destinationArray;
        }

        /// <summary>
        ///     This function exists to allow fast iterations. The size of the array returned cannot be
        ///     used. The list count must be used instead.
        /// </summary>
        /// <returns></returns>
        public T[] ToArrayFast()
        {
            return _buffer;
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
                _buffer[Count] = default(T);
                return false;
            }

            _buffer[index] = _buffer[Count];
            _buffer[Count] = default(T);

            return true;
        }

        private void AllocateMore()
        {
            var newList = new T[Math.Max(_buffer.Length << 1, MIN_SIZE)];
            if (Count > 0) _buffer.CopyTo(newList, 0);
            _buffer = newList;
        }

        private void AllocateMore(int newSize)
        {
            var oldLength = Math.Max(_buffer.Length, MIN_SIZE);

            while (oldLength < newSize)
                oldLength <<= 1;

            var newList = new T[oldLength];
            if (Count > 0) Array.Copy(_buffer, newList, Count);
            _buffer = newList;
        }

        public void Trim()
        {
            if (Count < _buffer.Length)
                Resize(Count);
        }

        public bool Reuse<U>(int index, out U result)
            where U : class, T
        {
            result = default(U);

            if (index >= _buffer.Length)
                return false;

            result = (U) _buffer[index];

            return result != null;
        }
    }

    public struct EcsListEnumerator<T> : IEnumerator<T>
    {
        public T Current { get; private set; }

        public EcsListEnumerator(T[] buffer, int size)
        {
            _size = size;
            _counter = 0;
            _buffer = buffer;
            Current = default(T);
        }

        object IEnumerator.Current => Current;

        T IEnumerator<T>.Current => Current;

        public void Dispose()
        {
            _buffer = null;
        }

        public bool MoveNext()
        {
            if (_counter < _size)
            {
                Current = _buffer[_counter++];

                return true;
            }

            Current = default(T);

            return false;
        }

        public void Reset()
        {
            _counter = 0;
        }

        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void IEnumerator.Reset()
        {
            Reset();
        }

        private T[] _buffer;
        private int _counter;
        private readonly int _size;
    }
}