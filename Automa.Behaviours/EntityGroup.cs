using System;
using System.Collections.Generic;
using Automa.Common;

namespace Automa.Behaviours
{
    public class EntityGroup : IDisposable
    {
        private readonly Dictionary<Type, IEntityList> entityLists = new Dictionary<Type, IEntityList>();

        public IEntityLink<T> Add<T>(T entity)
        {
            return GetEntityList<T>().Add(entity);
        }

        public IEntityLink Add(Type type, object entity)
        {
            return GetEntityList(type).Add(entity);
        }

        public IEntityList<T> GetEntityList<T>()
        {
            if (!entityLists.TryGetValue(TypeOf<T>.Type, out var entityList))
            {
                entityList = new EntityList<T>();
                entityLists.Add(TypeOf<T>.Type, entityList);
            }
            return (IEntityList<T>)entityList;
        }

        public IEntityList GetEntityList(Type type)
        {
            if (!entityLists.TryGetValue(type, out var entityList))
            {
                entityList = (IEntityList)Activator.CreateInstance(typeof(EntityList<>).MakeGenericType(type));
                entityLists.Add(type, entityList);
            }
            return entityList;
        }

        public void Dispose()
        {
            foreach (var updatablesValue in entityLists.Values)
            {
                updatablesValue.Dispose();
            }
        }
    }
}
