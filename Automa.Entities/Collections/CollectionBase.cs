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
                    ref var componentArray = ref arrays.Buffer[i];
                    result += componentArray.Count;
                }
                return result;
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