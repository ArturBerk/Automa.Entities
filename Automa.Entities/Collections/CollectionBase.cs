using System;
using Automa.Entities.Internal;

namespace Automa.Entities.Collections
{
    public abstract class CollectionBase
    {
        public abstract int CalculatedCount { get; }
        internal abstract void AddArray(EntityTypeData data);
        internal abstract void RemoveArray(EntityTypeData data);
    }

    public abstract class CollectionBase<T> : CollectionBase
    {
        internal readonly ArrayList<ComponentArray<T>> arrays = new ArrayList<ComponentArray<T>>();

        public override int CalculatedCount
        {
            get
            {
                var result = 0;
                for (var i = 0; i < arrays.Count; i++)
                {
                    result += arrays[i].Count;
                }
                previousIndex = -2;
                return result;
            }
        }

        private int previousIndex = -2;
        private int previousIndexInArray = -1;
        private int previousArrayIndex = -1;

        public ref T this[int index]
        {
            get
            {
                if (index < 0) throw new ArgumentException("Index is out of range");
                if (previousIndex == index - 1)
                {
                    previousIndex = index;
                    // Use cached indices
                    var chunkArray = arrays[previousArrayIndex];
                    var indexInArray = previousIndexInArray + 1;
                    if (indexInArray < chunkArray.Count)
                    {
                        previousIndexInArray = indexInArray;
                        return ref chunkArray[indexInArray];
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
                            return ref chunkArray[previousIndexInArray];
                        }
                    }
                }
                previousIndex = index;
                for (var i = 0; i < arrays.Count; i++)
                {
                    var chunkArray = arrays[i];
                    var chunkArrayCount = chunkArray.Count;
                    if (index < chunkArrayCount)
                    {
                        previousIndexInArray = index;
                        previousArrayIndex = i;
                        return ref chunkArray[index];
                    }
                    index -= chunkArrayCount;
                }
                throw new ArgumentException("Index is out of range");
            }
        }

        internal override void AddArray(EntityTypeData data)
        {
            var componentArray = data.GetComponentArray<T>();
            if (arrays.Contains(componentArray))
            {
                return;
            }
            arrays.Add(componentArray);
        }

        internal override void RemoveArray(EntityTypeData data)
        {
            arrays.Remove(data.GetComponentArray<T>());
        }

        public T[] ToArray(T[] destination = null)
        {
            var count = CalculatedCount;
            if (destination == null)
            {
                destination = new T[count];
            }
            else if (destination.Length < count)
            {
                Array.Resize(ref destination, count);
            }
            var index = 0;
            for (var i = 0; i < arrays.Count; i++)
            {
                var componentArray = arrays[i];
                componentArray.CopyTo(destination, index);
                index += componentArray.Count;
            }
            return destination;
        }
    }
}