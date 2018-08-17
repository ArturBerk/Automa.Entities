using System;
using System.Linq;
using Automa.Common;

namespace Automa.Behaviours
{
    public interface IEntityList : IDisposable
    {
        int Count { get; }
        Type Type { get; }
        IEntityLink Add(object entity);
    }

    public interface IEntityList<T> : IEntityList
    {
        ref T this[int index] { get; }
        ref T Single { get; }
        T[] ToArray();
        IEntityLink<T> Add(T entity);
        void AddHandler(IEntityAddedHandler<T> handler);
        void AddHandler(IEntityRemovedHandler<T> handler);
        void RemoveHandler(IEntityAddedHandler<T> handler);
        void RemoveHandler(IEntityRemovedHandler<T> handler);
    }

    internal class EntityList<T> : IEntityList<T>
    {
        private ArrayList<IEntityAddedHandler<T>> addedHandlers = new ArrayList<IEntityAddedHandler<T>>(4);
        internal ArrayList<EntityLink<T>> Entities = new ArrayList<EntityLink<T>>(4);
        private ArrayList<IEntityRemovedHandler<T>> removedHandlers = new ArrayList<IEntityRemovedHandler<T>>(4);

        public ref T this[int index] => ref Entities.Buffer[index].Instance;

        public ref T Single => ref Entities.Buffer[0].Instance;

        public T[] ToArray()
        {
            return Entities.Select(link => link.Entity).ToArray();
        }

        public IEntityLink<T> Add(T entity)
        {
            var index = Entities.Count;
            var link = EntityLink<T>.Take(entity, this, index);
            if (entity is IEntity entityLinked)
            {
                entityLinked.Link = link;
            }
            Entities.Add(link);
            if (addedHandlers.Count <= 0) return link;
            for (var i = 0; i < addedHandlers.Count; i++)
            {
                addedHandlers[i].OnEntityAdded(entity);
            }
            return link;
        }

        public EntityCollection<T> GetEntitites()
        {
            return new EntityCollection<T>(this);
        }

        public void AddHandler(IEntityAddedHandler<T> handler)
        {
            addedHandlers.Add(handler);
            for (var i = 0; i < Entities.Count; i++)
            {
                handler.OnEntityAdded(Entities.Buffer[i].Entity);
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
                handler.OnEntityRemoved(Entities.Buffer[i].Entity);
            }
        }

        public void Dispose()
        {
            addedHandlers.Clear();
            removedHandlers.Clear();
            foreach (var entityLink in Entities)
            {
                entityLink.isDisposed = true;
            }
            Entities.Clear();
        }

        public IEntityLink Add(object entity)
        {
            return Add((T) entity);
        }

        public int Count => Entities.Count;

        public Type Type => TypeOf<T>.Type;

        internal void Remove(EntityLink<T> entityLink)
        {
            var indexInList = entityLink.Index;
            if (Entities.UnorderedRemoveAt(indexInList))
            {
                Entities[indexInList].Index = indexInList;
            }
            if (removedHandlers.Count <= 0) return;
            for (var i = 0; i < removedHandlers.Count; i++)
            {
                removedHandlers[i].OnEntityRemoved(entityLink.Entity);
            }
        }
    }

    public interface IEntityAddedHandler<in T>
    {
        void OnEntityAdded(T entity);
    }

    public interface IEntityRemovedHandler<in T>
    {
        void OnEntityRemoved(T entity);
    }
}