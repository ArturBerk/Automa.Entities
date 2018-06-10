using System;

namespace Automa.Entities.Collections
{
    public class Collection<T> : CollectionBase<T>
    {
        public override int CalculatedCount
        {
            get
            {
                previousIndex = -2;
                return base.CalculatedCount;
            }
        }

        private int previousIndex = -2;
        private int previousIndexInArray = -1;
        private int previousArrayIndex = -1;

        public ref T this[int index]
        {
            get
            {
                if (previousIndex == index - 1)
                {
                    previousIndex = index;
                    // Use cached indices
                    var chunkArray = arrays[previousArrayIndex];
                    var indexInArray = previousIndexInArray + 1;
                    if (indexInArray < chunkArray.Count)
                    {
                        previousIndexInArray = indexInArray;
                        return ref chunkArray.Buffer[indexInArray];
                    }
                    previousIndexInArray = 0;
                    while (true)
                    {
                        ++previousArrayIndex;
                        if (previousArrayIndex >= arrays.Count)
                        {
                            throw new ArgumentException("Index is out of range");
                        }
                        chunkArray = arrays[previousArrayIndex];
                        if (chunkArray.Count > 0)
                        {
                            return ref chunkArray.Buffer[previousIndexInArray];
                        }
                    }
                }
                previousIndex = index;
                for (var i = 0; i < arrays.Count; i++)
                {
                    ref var chunkArray = ref arrays.Buffer[i];
                    var chunkArrayCount = chunkArray.Count;
                    if (index < chunkArrayCount)
                    {
                        previousIndexInArray = index;
                        previousArrayIndex = i;
                        return ref chunkArray.Buffer[index];
                    }
                    index -= chunkArrayCount;
                }
                throw new ArgumentException("Index is out of range");
            }
        }
    }

    // This is for multithreaded access
    public class AsyncCollection<T> : CollectionBase<T>
    {
        public ref T this[int index]
        {
            get
            {
                for (var i = 0; i < arrays.Count; i++)
                {
                    ref var chunkArray = ref arrays.Buffer[i];
                    var chunkArrayCount = chunkArray.Count;
                    if (index < chunkArrayCount)
                    {
                        return ref chunkArray.Buffer[index];
                    }
                    index -= chunkArrayCount;
                }
                throw new ArgumentException("Index is out of range");
            }
        }
    }
}