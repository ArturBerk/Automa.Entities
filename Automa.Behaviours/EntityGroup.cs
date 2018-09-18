using System;
using System.Globalization;
using System.Reflection;
using Automa.Common;

namespace Automa.Behaviours
{
    public class EntityGroup : IDisposable
    {
        private ArrayList<IEntityCollection> entityLists = new ArrayList<IEntityCollection>(4);

        public ref T GetEntityByRef<T>(ref EntityReference reference)
        {
            return ref GetEntities<T>().ByRef(ref reference);
        }

        public ref T GetEntity<T>(EntityReference reference)
        {
            return ref GetEntities<T>()[reference];
        }

        public EntityReference Add<T>(T entity)
        {
            return GetEntities<T>().Add(entity);
        }

        public EntityReference Add(Type type, object entity)
        {
            return GetEntities(type).Add(entity);
        }

        public EntityReference AddConnected(Type type, object entity, EntityReference reference)
        {
            var targetCollection = GetEntities(type);
            var targetReference = targetCollection.Add(entity);

            var sourceCollection = (EntityCollection)entityLists[(int)reference.TypeIndex];
            ref var currentIndex = ref sourceCollection.EntityIndices[reference.Index];
            currentIndex.parentTypeCollection = (EntityCollection)targetCollection;
            currentIndex.parentTypeEntityReference = targetReference;

            return targetReference;
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
