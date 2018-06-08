using System;
using System.Collections.Generic;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public sealed class EntityManager : IManager
    {
        private readonly Dictionary<uint, EntityTypeData> entityTypeDatas = new Dictionary<uint, EntityTypeData>();
        private readonly Queue<int> availableIndices = new Queue<int>();

        private readonly EntitiesGroup entitiesGroup;
        private readonly ArrayList<EntityLink> entityLinks = new ArrayList<EntityLink>();

        private readonly ArrayList<Group> groups = new ArrayList<Group>();

        private ComponentType[] componentTypeCache = new ComponentType[10];

        public EntityManager()
        {
            entitiesGroup = RegisterGroup(new EntitiesGroup());
        }

        public Collections.Entities Entities => entitiesGroup.Entities;

        internal IEnumerable<EntityTypeData> Datas => entityTypeDatas.Values;

        public int EntityCount => entityLinks.Count - availableIndices.Count;

        public void OnAttachToContext(IContext context)
        {
            //
        }

        public void OnDetachFromContext(IContext context)
        {
            //
        }

        public void OnUpdate()
        {
            foreach (var @group in groups)
            {
                group.UpdateLength();
            }
        }

        public Entity CreateEntity(params ComponentType[] types)
        {
            var entityType = new EntityType(types);
            var chunk = GetOrCreateData(entityType.Hash, types, types.Length);
            var entityId = availableIndices.Count > 0 ? availableIndices.Dequeue() : entityLinks.Count;
            var version = 0;
            if (entityLinks.Count > entityId)
            {
                version = entityLinks[entityId].Entity.Version + 1;
            }
            else
            {
                entityLinks.SetWithExpand(entityId, new EntityLink());
            }
            var entity = new Entity(entityId, version);
            var chunkIndex = chunk.AddEntity(entity);
            entityLinks[entityId] = new EntityLink
            {
                Data = chunk,
                IndexInData = chunkIndex,
                Entity = entity
            };
            return entity;
        }

        public void SetComponent<T>(Entity entity, T component)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            entityLink.Data.SetComponent(entityLink.IndexInData, component);
        }

        public bool HasComponent<T>(Entity entity)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            return entityLink.Data.HasComponent<T>();
        }

        public void RemoveEntity(Entity entity)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            HandleEntityRemoving(entityLink.Data.RemoveEntity(entityLink.IndexInData));
            entityLinks[entity.Id] = new EntityLink
            {
                Entity = Entity.Null
            };
            availableIndices.Enqueue(entity.Id);
        }

        private void HandleEntityRemoving((int entityId, int newIndexInChunk) removeData)
        {
            entityLinks[removeData.entityId].IndexInData = removeData.newIndexInChunk;
        }

        public void AddComponent<T>(Entity entity, T component)
        {
            ref var entityLink = ref entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var entityType = entityLink.Data.EntityType;


            if (componentTypeCache.Length < entityType.Types.Length + 1)
            {
                Array.Resize(ref componentTypeCache, entityType.Types.Length + 1);
            }
            ComponentType newComponentType = typeof(T);
            var index = 0;
            for (var i = 0; i < entityType.Types.Length; i++)
            {
                var entityComponentType = entityType.Types[i];
                if (entityComponentType.TypeId < newComponentType.TypeId)
                {
                    componentTypeCache[index++] = entityComponentType;
                }
                else if (entityComponentType.TypeId == newComponentType.TypeId)
                {
                    throw new ArgumentException("Entity already contains component of type " + typeof(T));
                }
                else
                {
                    break;
                }
            }
            componentTypeCache[index++] = newComponentType;
            for (var i = index - 1; i < entityType.Types.Length; i++)
            {
                componentTypeCache[index++] = entityType.Types[i];
            }
            var entityTypeHash = EntityType.CalculateHash(componentTypeCache, index);
            var data = GetOrCreateData(entityTypeHash, componentTypeCache, index);

            MoveEntity(ref entityLink, data, ref entityType);
            data.SetComponent(entityLink.IndexInData, component);
        }

        private EntityTypeData GetOrCreateData(uint index, ComponentType[] components, int count)
        {
            if (!entityTypeDatas.TryGetValue(index, out var data))
            {
                data = new EntityTypeData(new EntityType(index, components, count));
                entityTypeDatas.Add(index, data);
                OnEntityTypeAdd(ref data);
            }
            return data;
        }

        public void RemoveComponent<T>(Entity entity)
        {
            ref var entityLink = ref entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var entityType = entityLink.Data.EntityType;

            if (componentTypeCache.Length < entityType.Types.Length - 1)
            {
                Array.Resize(ref componentTypeCache, entityType.Types.Length - 1);
            }
            ComponentType newComponentType = typeof(T);
            var index = 0;
            for (var i = 0; i < entityType.Types.Length; i++)
            {
                var entityComponentType = entityType.Types[i];
                if (entityComponentType.TypeId != newComponentType.TypeId)
                {
                    componentTypeCache[index++] = entityComponentType;
                }
            }
            var entityTypeHash = EntityType.CalculateHash(componentTypeCache, index);
            var data = GetOrCreateData(entityTypeHash, componentTypeCache, index);

            var newEntityType = data.EntityType;
            MoveEntity(ref entityLink, data, ref newEntityType);
        }

        private void MoveEntity(ref EntityLink entityLink, EntityTypeData data, ref EntityType copyTypesBasedOn)
        {
            // Add new entity to new entity type data
            var newEntityIndexInData = data.AddEntity(entityLink.Entity);
            // Copy component data from old to new entity type data
            for (var i = 0; i < copyTypesBasedOn.Types.Length; i++)
            {
                var type = copyTypesBasedOn.Types[i];
                data.GetComponentArrayUnchecked(type)
                    .CopyFrom(entityLink.Data.GetComponentArrayUnchecked(type),
                        entityLink.IndexInData,
                        newEntityIndexInData);
            }
            // Remove entity from old entity type data
            HandleEntityRemoving(entityLink.Data.RemoveEntity(entityLink.IndexInData));
            // Update new entity link
            entityLink.Data = data;
            entityLink.IndexInData = newEntityIndexInData;
        }

        public ref T GetComponent<T>(Entity entity)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            return ref entityLink.Data.GetComponentArray<T>()[entityLink.IndexInData];
        }

        public T RegisterGroup<T>(T group) where T : Group
        {
            groups.Add(group);
            group.Register(this);
            group.UpdateLength();
            return group;
        }

        public void UnregisterGroup<T>(T group) where T : Group
        {
            if (groups.Remove(group))
            {
                group.Unregister(this);
            }
        }

        private void OnEntityTypeAdd(ref EntityTypeData data)
        {
            for (var index = 0; index < groups.Count; index++)
            {
                var group = groups[index];
                group.OnEntityTypeAdd(data);
            }
        }

        private void OnEntityTypeRemove(ref EntityTypeData data)
        {
            for (var index = 0; index < groups.Count; index++)
            {
                var group = groups[index];
                group.OnEntityTypeRemoved(data);
            }
        }

        private struct EntityLink
        {
            public Entity Entity;
            public EntityTypeData Data;
            public int IndexInData;
        }

        public class EntitiesGroup : Group
        {
            public Collections.Entities Entities;
        }
    }
}