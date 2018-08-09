using System;
using Automa.Common;
using Automa.Entities.Internal;

namespace Automa.Entities.Collections
{
    public abstract class CollectionBase
    {
        public abstract int CalculatedCount { get; }
        internal abstract void AddArray(EntityTypeData data);
        internal abstract void GetArrayLengths(ref ArrayList<int> componentArrayLengths, out int count);
    }

    public abstract class CollectionBase<T> : CollectionBase
    {
        internal ArrayList<ComponentArray<T>> arrays = new ArrayList<ComponentArray<T>>(4);

        public override int CalculatedCount
        {
            get
            {
                var result = 0;
                for (var i = 0; i < arrays.Count; i++)
                {
                    ref var componentArray = ref arrays.Buffer[i];
                    result += componentArray.Count;
                }
                return result;
            }
        }

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

        public ref T this[Group.EntityIndex index] => ref arrays.Buffer[index.ArrayIndex][index.Index];

        internal override void GetArrayLengths(ref ArrayList<int> componentArrayLengths, out int count)
        {
            count = 0;
            for (int i = 0; i < arrays.Count; i++)
            {
                ref var componentArray = ref arrays.Buffer[i];
                componentArrayLengths.Add(componentArray.Count);
                count += componentArray.Count;
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