using Automa.Entities.Internal;

namespace Automa.Entities
{
    internal class ComponentArray<T> : ArrayList<T>, IComponentArray
    {
        public void CopyFrom(IComponentArray source, int sourceIndex, int destIndex)
        {
            var sourceArray = (ComponentArray<T>) source;
            this[destIndex] = sourceArray[sourceIndex];
        }

        public void SetDefault(int index)
        {
            SetWithExpand(index, default(T));
        }
    }

    internal interface IComponentArray
    {
        void CopyFrom(IComponentArray source, int index, int destIndex);
        void SetDefault(int index);
        bool UnorderedRemoveAt(int index);
    }
}