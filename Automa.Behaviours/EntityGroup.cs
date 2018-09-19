using System;
using System.Globalization;
using System.Reflection;
using Automa.Common;

namespace Automa.Behaviours
{
    public class EntityGroup : IDisposable
    {
        private ArrayList<IEntityCollectionInternal> entityLists = new ArrayList<IEntityCollectionInternal>(4);

        public ref T GetEntityByRef<T>(ref EntityReference reference) where T : IEntity
        {
            return ref GetEntitiesInternal<T>().ByRef(ref reference);
        }

        public ref T GetEntity<T>(EntityReference reference) where T : IEntity
        {
            return ref GetEntitiesInternal<T>()[reference];
        }

        public T Add<T>(T entity, bool withSubTypes = false) where T : IEntity
        {
            if (withSubTypes)
            {
                var parentCollection = (EntityCollection)GetEntitiesInternal<T>();
                var baseReference = parentCollection.Add(entity);

                var parentReference = entity.Reference;
                var currentType = TypeOf<T>.Type.BaseType;
                while (currentType != null && !currentType.IsAbstract && currentType != typeof(object))
                {
                    var currentCollection = (EntityCollection)GetEntitiesInternal(currentType);
                    parentReference = currentCollection.AddConnected(entity, parentCollection, ref parentReference);
                    parentCollection = currentCollection;
                    currentType = currentType.BaseType;
                }
                entity.Reference = baseReference;
                return entity;
            }
            else
            {
                entity.Reference = GetEntitiesInternal<T>().Add(entity);
                return entity;
            }
        }

        public IEntity Add(Type type, IEntity entity, bool withSubTypes = false)
        {
            if (withSubTypes)
            {
                var parentCollection = (EntityCollection) GetEntitiesInternal(type);
                var baseReference = parentCollection.Add(entity);

                var parentReference = entity.Reference;
                var currentType = type.BaseType;
                while (currentType != null && currentType != typeof(object))
                {
                    var currentCollection = (EntityCollection) GetEntitiesInternal(currentType);
                    parentReference = currentCollection.AddConnected(entity, parentCollection, ref parentReference);
                    parentCollection = currentCollection;
                    currentType = currentType.BaseType;
                }
                var interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    currentType = interfaces[i];
                    if (currentType == TypeOf<IEntity>.Type || !TypeOf<IEntity>.Type.IsAssignableFrom(currentType)) continue;
                    var currentCollection = (EntityCollection)GetEntitiesInternal(currentType);
                    parentReference = currentCollection.AddConnected(entity, parentCollection, ref parentReference);
                    parentCollection = currentCollection;
                }
                entity.Reference = baseReference;
                return entity;
            }
            else
            {
                entity.Reference = GetEntitiesInternal(type).Add(entity);
                return entity;
            }
        }

        public void Remove(IEntity entity)
        {
            var entityReference = entity.Reference;
            if (entityReference.IsNull) throw new ApplicationException("Entity is already added to group");
            entityLists[(int)entityReference.TypeIndex].Remove(entityReference);
            entity.Reference = EntityReference.Null;
        }

        private IEntityCollectionInternal<T> GetEntitiesInternal<T>() where T : IEntity
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

        private IEntityCollectionInternal GetEntitiesInternal(Type type)
        {
            IEntityCollectionInternal entityCollection;
            var entityType = EntityTypeManager.GetTypeIndex(type);
            if (entityLists.Count <= entityType)
            {
                entityCollection = (IEntityCollectionInternal)Activator.CreateInstance(typeof(EntityCollection<>).MakeGenericType(type),
                    BindingFlags.Public | BindingFlags.Instance, null, new object[] { entityType }, CultureInfo.CurrentCulture, null);
                entityLists.SetAt(entityType, entityCollection);
            }
            else
            {
                entityCollection = entityLists[entityType];
            }
            return entityCollection;
        }

        public IEntityCollection<T> GetEntities<T>() where T : IEntity
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
            IEntityCollectionInternal entityCollection;
            var entityType = EntityTypeManager.GetTypeIndex(type);
            if (entityLists.Count <= entityType)
            {
                entityCollection = (IEntityCollectionInternal)Activator.CreateInstance(typeof(EntityCollection<>).MakeGenericType(type),
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
