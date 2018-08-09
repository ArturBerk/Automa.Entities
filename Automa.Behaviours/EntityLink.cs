using Automa.Common;

namespace Automa.Behaviours
{
    internal class EntityLink<T> : IEntityLink<T>
    {
        private static ArrayList<EntityLink<T>> pool =
            new ArrayList<EntityLink<T>>(4);
        internal int Index;
        internal T Instance;
        private EntityList<T> list;

        public void Dispose()
        {
            list.Remove(this);
            Release(this);
        }

        object IEntityLink.Entity => Instance;

        public T Entity => Instance;

        internal static EntityLink<T> Take(T behaviour, EntityList<T> list, int index)
        {
            EntityLink<T> instance = null;
            if (pool.Count == 0)
            {
                instance = new EntityLink<T>();
            }
            else
            {
                instance = pool.Buffer[--pool.Count];
                pool.Buffer[pool.Count] = null;
            }
            instance.Index = index;
            instance.list = list;
            instance.Instance = behaviour;
            return instance;
        }

        internal static void Release(EntityLink<T> entityLink)
        {
            pool.Add(entityLink);
        }
    }
}