using System.Collections;
using System.Collections.Generic;

namespace Automa.Entities.Internal
{
    public struct ReadOnlyArray<T> : IEnumerable<T>
    {
        private readonly T[] buffer;
        public readonly int Count;

        public ReadOnlyArray(T[] buffer, int count)
        {
            this.buffer = buffer;
            Count = count;
        }

        public ref T this[int index] => ref buffer[index];

        public IEnumerator<T> GetEnumerator()
        {
            return new ArrayEnumerator<T>(buffer, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArrayEnumerator<T>(buffer, Count);
        }
    }
}