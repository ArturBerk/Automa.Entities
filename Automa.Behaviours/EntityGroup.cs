using System;
using System.Globalization;
using System.Reflection;
using Automa.Common;

namespace Automa.Behaviours
{
    public class EntityGroup : IDisposable
    {
        private ArrayList<IEntityCollection> entityLists = new ArrayList<IEntityCollection>(4);

        public EntityReference Add<T>(T entity)
        {
            return GetEntities<T>().Add(entity);
        }

        public EntityReference Add(Type type, object entity)
        {
            return GetEntities(type).Add(entity);
        }

        public void Remove(EntityReference entityReference)
        {
            entityLists[(int)entityReference.TypeIndex].Remove(entityReference);
        }

        public IEntityCollection<T> GetEntities<T>()
        {
            EntityCollection<T> entityCollection;
            var entityType = EntityTypeManager.GetTypeIndex<T>();
            if (entityLists.Count <= entityType)
            {
                entityCollection = new EntityCollection<T>(entityType);
                entityLists.SetAt(entityType, entityCollection);
            }
            else
            {
                entityCollection = (EntityCollection<T>)entityLists[entityType];
            }
            return entityCollection;
        }

        public IEntityCollection GetEntities(Type type)
        {
            IEntityCollection entityCollection;
            var entityType = EntityTypeManager.GetTypeIndex(type);
            if (entityLists.Count <= entityType)
            {
                entityCollection = (IEntityCollection)Activator.CreateInstance(typeof(EntityCollection<>).MakeGenericType(type),
                    BindingFlags.Public | BindingFlags.Instance, null, new object[] { entityType }, CultureInfo.CurrentCulture, null);
                entityLists.SetAt(entityType, entityCollection);
            }
            else
            {
                entityCollection = entityLists[entityType];
            }
            return entityCollection;
        }

        public void Dispose()
        {
            foreach (var updatablesValue in entityLists)
            {
                updatablesValue.Dispose();
            }
            entityLists.Clear();
        }
    }
}
