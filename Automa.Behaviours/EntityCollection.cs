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
        ref T this[EntityReference index] { get; }
        ref T ByRef(ref EntityReference reference);
        T[] ToArray();
        EntityReference Add(T entity);
        void AddHandler(IEntityAddedHandler<T> handler);
        void AddHandler(IEntityRemovedHandler<T> handler);
        void RemoveHandler(IEntityAddedHandler<T> handler);
        void RemoveHandler(IEntityRemovedHandler<T> handler);
    }

    internal abstract class EntityCollection : IEntityCollection
    {
        internal ArrayList<EntityIndex> EntityIndices = new ArrayList<EntityIndex>(4);
        internal ArrayList<int> AvailableEntityIndices = new ArrayList<int>(4);

        public void Connect(IEntityCollection collection, ref EntityReference reference)
        {
            
        }

        public abstract void Dispose();
        public abstract int Count { get; }
        public abstract Type Type { get; }
        public abstract EntityReference Add(object entity);
        public abstract void Remove(EntityReference entityReference);
    }

    internal struct EntityIndex
    {
        public int Index;
        public byte Version;

        public EntityCollection parentTypeCollection;
        public EntityReference parentTypeEntityReference;

        public EntityIndex(int index, byte version)
        {
            Index = index;
            Version = version;
            parentTypeCollection = null;
            parentTypeEntityReference = new EntityReference(0, 0, 0);
        }
    }

    internal class EntityCollection<T> : EntityCollection, IEntityCollection<T>
    {
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

        public EntityCollection(ushort entityType)
        {
            this.entityType = entityType;
        }

        private readonly ushort entityType;
        private ArrayList<IEntityAddedHandler<T>> addedHandlers = new ArrayList<IEntityAddedHandler<T>>(4);
        private ArrayList<IEntityRemovedHandler<T>> removedHandlers = new ArrayList<IEntityRemovedHandler<T>>(4);

        internal ArrayList<EntitySlot> Entities = new ArrayList<EntitySlot>(4);

        public ref T this[int index] => ref Entities.Buffer[index].Value;
        public ref T this[EntityReference reference]
        {
            get
            {
                if (reference.TypeIndex != entityType)
                {
                    throw new ApplicationException("Invalid entity type");
                }
                ref var index = ref EntityIndices[reference.Index];
                if (index.Version != reference.Version)
                {
                    throw new ApplicationException("Entity removed");
                }
                return ref Entities.Buffer[index.Index].Value;
            }
        }

        public ref T ByRef(ref EntityReference reference)
        {
            if (reference.TypeIndex != entityType)
            {
                throw new ApplicationException("Invalid entity type");
            }
            ref var index = ref EntityIndices[reference.Index];
            if (index.Version != reference.Version)
            {
                throw new ApplicationException("Entity removed");
            }
            return ref Entities.Buffer[index.Index].Value;
        }

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

            // Added handlers
            if (addedHandlers.Count <= 0) return new EntityReference(entityType, indexPosition, currentIndex.Version);
            ref var entityValue = ref Entities[entityPosition].Value;
            for (var i = 0; i < addedHandlers.Count; i++)
            {
                addedHandlers[i].OnEntityAdded(ref entityValue);
            }
            return new EntityReference(entityType, indexPosition, currentIndex.Version);
        }

        public override EntityReference Add(object entity)
        {
            return Add((T) entity);
        }

        internal EntityReference AddConnected(T entity, EntityCollection sourceCollection, ref EntityReference reference)
        {
            var targetReference = Add(entity);

            ref var currentIndex = ref sourceCollection.EntityIndices[reference.Index];
            while (currentIndex.parentTypeCollection != null)
            {
                currentIndex =
                    currentIndex.parentTypeCollection.EntityIndices[currentIndex.parentTypeEntityReference.Index];
            }
            currentIndex.parentTypeCollection = this;
            currentIndex.parentTypeEntityReference = targetReference;

            return targetReference;
        }

        internal EntityReference AddConnected(object entity, EntityCollection sourceCollection, ref EntityReference reference)
        {
            return AddConnected((T) entity, sourceCollection, ref reference);
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

        public override void Dispose()
        {
            addedHandlers.Clear();
            removedHandlers.Clear();
            for (int i = 0; i < EntityIndices.Count; i++)
            {
                ++EntityIndices.Buffer[i].Version;
            }
            Entities.Clear();
        }

        public override int Count => Entities.Count;

        public override Type Type => TypeOf<T>.Type;

        public override void Remove(EntityReference reference)
        {
            ref var index = ref EntityIndices[reference.Index];
            if (index.Version != reference.Version)
            {
                return;
            }
            unchecked
            {
                ++index.Version;
            }
            //////// remove linked
            while (index.parentTypeCollection != null)
            {
                index.parentTypeCollection.Remove(index.parentTypeEntityReference);
                index.parentTypeCollection = null;
            }
            /////////
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