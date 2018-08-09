namespace Automa.Behaviours
{
    public struct EntityCollection<T>
    {
        private readonly EntityList<T> entityList;

        public readonly int Count;

        public ref T this[int index] => ref entityList.Entities.Buffer[index].Instance;

        internal EntityCollection(EntityList<T> entityList) : this()
        {
            this.entityList = entityList;
            Count = entityList.Entities.Count;
        }
    }
}