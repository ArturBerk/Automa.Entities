using System;
using System.Diagnostics;
using System.Linq;
using Automa.Common;

namespace Automa.Behaviours
{
    public interface IEntityCollection : IDisposable
    {
        int Count { get; }
        Type Type { get; }
        IEntity[] ToArray();
        IEntity this[int index] { get; }
        IEntity this[EntityReference index] { get; }
    }

    public interface IEntityCollection<T> : IEntityCollection where T : IEntity
    {
        new ref T this[int index] { get; }
        new ref T this[EntityReference index] { get; }
        new T[] ToArray();
        void AddHandler(IEntityAddedHandler<T> handler);
        void AddHandler(IEntityRemovedHandler<T> handler);
        void RemoveHandler(IEntityAddedHandler<T> handler);
        void RemoveHandler(IEntityRemovedHandler<T> handler);
    }

    internal interface IEntityCollectionInternal : IEntityCollection
    {
        EntityReference Add(IEntity entity);
        void Remove(EntityReference entityReference);
    }

    internal interface IEntityCollectionInternal<T> : IEntityCollection<T> where T : IEntity
    {
        EntityReference Add(T entity);
        ref T ByRef(ref EntityReference reference);
    }

    [DebuggerDisplay("EntityCollection {" + nameof(Type) + ".Name} ({"+nameof(Count)+"})")]
    internal abstract class EntityCollection : IEntityCollectionInternal
    {
        internal ArrayList<EntityIndex> EntityIndices = new ArrayList<EntityIndex>(4);
        internal ArrayList<int> AvailableEntityIndices = new ArrayList<int>(4);

        public abstract void Dispose();
        public abstract int Count { get; }
        public abstract Type Type { get; }

        public abstract IEntity[] ToArray();

        public abstract IEntity this[int index] { get; }
        public abstract IEntity this[EntityReference index] { get; }

        public abstract EntityReference Add(IEntity entity);
        public abstract void Remove(EntityReference entityReference);

        public abstract EntityReference AddConnected(IEntity entity, EntityCollection sourceCollection,
            ref EntityReference parentReference);
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

    internal class EntityCollection<T> : EntityCollection, IEntityCollectionInternal<T> where T: IEntity
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

        public override IEntity this[int index] => Entities.Buffer[index].Value;

        public override IEntity this[EntityReference reference]
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
                return Entities.Buffer[index.Index].Value;
            }
        }

        ref T IEntityCollection<T>.this[int index] => ref Entities.Buffer[index].Value;

        ref T IEntityCollection<T>.this[EntityReference reference]
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

        public override IEntity[] ToArray()
        {
            var t = new IEntity[Entities.Count];
            for (int i = 0; i < Entities.Count; i++)
            {
                t[i] = Entities[i].Value;
            }
            return t;
        }

        T[] IEntityCollection<T>.ToArray()
        {
            var t = new T[Entities.Count];
            for (int i = 0; i < Entities.Count; i++)
            {
                t[i] = Entities[i].Value;
            }
            return t;
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
            var reference = new EntityReference(entityType, indexPosition, currentIndex.Version);
            if (addedHandlers.Count <= 0) return reference;
            ref var entityValue = ref Entities[entityPosition].Value;
            for (var i = 0; i < addedHandlers.Count; i++)
            {
                addedHandlers[i].OnEntityAdded(ref entityValue);
            }
            return reference;
        }

        public override EntityReference Add(IEntity entity)
        {
            return Add((T) entity);
        }

        public override EntityReference AddConnected(IEntity entity, 
            EntityCollection sourceCollection, ref EntityReference parentReference)
        {
            var reference1 = Add(entity);

            ref var currentIndex = ref sourceCollection.EntityIndices[parentReference.Index];
            while (currentIndex.parentTypeCollection != null)
            {
                currentIndex =
                    currentIndex.parentTypeCollection.EntityIndices[currentIndex.parentTypeEntityReference.Index];
            }
            currentIndex.parentTypeCollection = this;
            currentIndex.parentTypeEntityReference = reference1;

            return reference1;
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