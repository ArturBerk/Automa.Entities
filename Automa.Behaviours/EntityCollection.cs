using System;
using System.Linq;
using Automa.Common;

namespace Automa.Behaviours
{
    public interface IEntityCollection : IDisposable
    {
        int Count { get; }
        Type Type { get; }
        EntityReference Add(object entity);
        void Remove(EntityReference entityReference);
    }

    public interface IEntityCollection<T> : IEntityCollection
    {
        ref T this[int index] { get; }
        ref T Single { get; }
        T[] ToArray();
        EntityReference Add(T entity);
        void AddHandler(IEntityAddedHandler<T> handler);
        void AddHandler(IEntityRemovedHandler<T> handler);
        void RemoveHandler(IEntityAddedHandler<T> handler);
        void RemoveHandler(IEntityRemovedHandler<T> handler);
    }

    internal class EntityCollection<T> : IEntityCollection<T>
    {
        internal struct EntityIndex
        {
            public int Index;
            public byte Version;

            public EntityIndex(int index, byte version)
            {
                Index = index;
                Version = version;
            }
        }

        internal struct EntitySlot
        {
            public T Value;
            public int IndexPosition;

            public EntitySlot(ref T value, int indexPosition)
            {
                Value = value;
                IndexPosition = indexPosition;
            }
        }


        public EntityCollection(uint entityType)
        {
            this.entityType = entityType;
        }

        private uint entityType;
        private ArrayList<IEntityAddedHandler<T>> addedHandlers = new ArrayList<IEntityAddedHandler<T>>(4);
        private ArrayList<IEntityRemovedHandler<T>> removedHandlers = new ArrayList<IEntityRemovedHandler<T>>(4);

        internal ArrayList<EntityIndex> EntityIndices = new ArrayList<EntityIndex>(4);
        internal ArrayList<int> AvailableEntityIndices = new ArrayList<int>(4);
        internal ArrayList<EntitySlot> Entities = new ArrayList<EntitySlot>(4);

        public ref T this[int index] => ref Entities.Buffer[index].Value;

        public ref T ByLink(ref EntityReference reference)
        {
            ref var index = ref EntityIndices[reference.Index];
            if (index.Version != reference.Version)
            {
                throw new ApplicationException("Entity removed");
            }
            return ref Entities.Buffer[index.Index].Value;
        }

        public ref T Single => ref Entities.Buffer[0].Value;

        public T[] ToArray()
        {
            return Entities.Select(slot => slot.Value).ToArray();
        }

        public EntityReference Add(T entity)
        {
            var indexPosition = AvailableEntityIndices.Count > 0
                ? AvailableEntityIndices[--AvailableEntityIndices.Count] : EntityIndices.Count;

            var entityPosition = Entities.Count;

            EntityIndices.ExpandCount(indexPosition + 1);
            ref var currentIndex = ref EntityIndices.Buffer[indexPosition];
            currentIndex.Index = entityPosition;
            // unchecked { currentIndex.Version = (byte)(currentIndex.Version + 1); }
            Entities.Add(new EntitySlot(ref entity, indexPosition));

            var link = new EntityReference(entityType, indexPosition, currentIndex.Version);

            // Added handlers
            if (addedHandlers.Count <= 0) return link;
            ref var entityValue = ref Entities[entityPosition].Value;
            for (var i = 0; i < addedHandlers.Count; i++)
            {
                addedHandlers[i].OnEntityAdded(ref entityValue);
            }

            return link;
        }

        public void AddHandler(IEntityAddedHandler<T> handler)
        {
            addedHandlers.Add(handler);
            for (var i = 0; i < Entities.Count; i++)
            {
                handler.OnEntityAdded(ref Entities.Buffer[i].Value);
            }
        }

        public void AddHandler(IEntityRemovedHandler<T> handler)
        {
            removedHandlers.Add(handler);
        }

        public void RemoveHandler(IEntityAddedHandler<T> handler)
        {
            addedHandlers.Remove(handler);
        }

        public void RemoveHandler(IEntityRemovedHandler<T> handler)
        {
            removedHandlers.Remove(handler);
            for (var i = 0; i < Entities.Count; i++)
            {
                handler.OnEntityRemoved(ref Entities.Buffer[i].Value);
            }
        }

        public void Dispose()
        {
            addedHandlers.Clear();
            removedHandlers.Clear();
            for (int i = 0; i < EntityIndices.Count; i++)
            {
                ++EntityIndices.Buffer[i].Version;
            }
            Entities.Clear();
        }

        public EntityReference Add(object entity)
        {
            return Add((T)entity);
        }

        public int Count => Entities.Count;

        public Type Type => TypeOf<T>.Type;

        public void Remove(EntityReference reference)
        {
            ref var index = ref EntityIndices[reference.Index];
            if (index.Version != reference.Version)
            {
                throw new ApplicationException("Entity already removed");
            }
            unchecked
            {
                ++index.Version;
            }
            AvailableEntityIndices.Add(reference.Index);

            var entityPosition = index.Index;

            if (removedHandlers.Count > 0)
            {
                ref var entityValue = ref Entities[entityPosition].Value;
                for (var i = 0; i < removedHandlers.Count; i++)
                {
                    removedHandlers[i].OnEntityRemoved(ref entityValue);
                }
            }
            if (Entities.UnorderedRemoveAt(entityPosition))
            {
                EntityIndices[Entities[entityPosition].IndexPosition].Index = entityPosition;
            }
        }
    }

    public interface IEntityAddedHandler<T>
    {
        void OnEntityAdded(ref T entity);
    }

    public interface IEntityRemovedHandler<T>
    {
        void OnEntityRemoved(ref T entity);
    }
}