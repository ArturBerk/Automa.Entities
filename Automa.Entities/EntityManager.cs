using System;
using System.Collections.Generic;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public sealed class EntityManager
    {
        private readonly Dictionary<uint, ArchetypeData> archetypeDatas = new Dictionary<uint, ArchetypeData>();
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

        internal IEnumerable<ArchetypeData> Datas => archetypeDatas.Values;

        public int EntityCount => entityLinks.Count - availableIndices.Count;

        public Entity CreateEntity(params ComponentType[] types)
        {
            var archetype = new Archetype(types);
            var chunk = GetOrCreateData(archetype.Index, types, types.Length);
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

        public void SetComponent<T>(Entity entity, T component) where T : IComponent
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            entityLink.Data.SetComponent(entityLink.IndexInData, component);
        }

        public bool HasComponent<T>(Entity entity) where T : IComponent
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

        public void AddComponent<T>(Entity entity, T component) where T : IComponent
        {
            ref var entityLink = ref entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var archetype = entityLink.Data.Archetype;


            if (componentTypeCache.Length < archetype.Types.Length + 1)
            {
                Array.Resize(ref componentTypeCache, archetype.Types.Length + 1);
            }
            ComponentType newComponentType = typeof(T);
            var index = 0;
            for (var i = 0; i < archetype.Types.Length; i++)
            {
                var archetypeType = archetype.Types[i];
                if (archetypeType.TypeId < newComponentType.TypeId)
                {
                    componentTypeCache[index++] = archetypeType;
                }
                else if (archetypeType.TypeId == newComponentType.TypeId)
                {
                    throw new ArgumentException("Entity already contains component of type " + typeof(T));
                }
                else
                {
                    break;
                }
            }
            componentTypeCache[index++] = newComponentType;
            for (var i = index - 1; i < archetype.Types.Length; i++)
            {
                componentTypeCache[index++] = archetype.Types[i];
            }
            var archetypeIndex = Archetype.CalculateIndex(componentTypeCache, index);
            var data = GetOrCreateData(archetypeIndex, componentTypeCache, index);

            MoveEntity(ref entityLink, data, ref archetype);
            data.SetComponent(entityLink.IndexInData, component);
        }

        private ArchetypeData GetOrCreateData(uint index, ComponentType[] components, int count)
        {
            if (!archetypeDatas.TryGetValue(index, out var data))
            {
                data = new ArchetypeData(new Archetype(index, components, count));
                archetypeDatas.Add(index, data);
                OnArchetypeAdd(ref data);
            }
            return data;
        }

        public void RemoveComponent<T>(Entity entity)
        {
            ref var entityLink = ref entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var archetype = entityLink.Data.Archetype;

            if (componentTypeCache.Length < archetype.Types.Length - 1)
            {
                Array.Resize(ref componentTypeCache, archetype.Types.Length - 1);
            }
            ComponentType newComponentType = typeof(T);
            var index = 0;
            for (var i = 0; i < archetype.Types.Length; i++)
            {
                var archetypeType = archetype.Types[i];
                if (archetypeType.TypeId != newComponentType.TypeId)
                {
                    componentTypeCache[index++] = archetypeType;
                }
            }
            var archetypeIndex = Archetype.CalculateIndex(componentTypeCache, index);
            var data = GetOrCreateData(archetypeIndex, componentTypeCache, index);

            var newArchetype = data.Archetype;
            MoveEntity(ref entityLink, data, ref newArchetype);
        }

        private void MoveEntity(ref EntityLink entityLink, ArchetypeData data, ref Archetype copyTypesBasedOn)
        {
            // Add new entity to new archetypeData
            var newEntityIndexInData = data.AddEntity(entityLink.Entity);
            // Copy component data from old to new archetypeData
            for (var i = 0; i < copyTypesBasedOn.Types.Length; i++)
            {
                var type = copyTypesBasedOn.Types[i];
                data.GetComponentArrayUnchecked(type)
                    .CopyFrom(entityLink.Data.GetComponentArrayUnchecked(type),
                        entityLink.IndexInData,
                        newEntityIndexInData);
            }
            // Remove entity from old archetypeData
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
            return group;
        }

        public void UnregisterGroup<T>(T group) where T : Group
        {
            if (groups.Remove(group))
            {
                group.Unregister(this);
            }
        }

        private void OnArchetypeAdd(ref ArchetypeData data)
        {
            for (var index = 0; index < groups.Count; index++)
            {
                var group = groups[index];
                group.OnArchetypeAdd(data);
            }
        }

        private void OnArchetypeRemove(ref ArchetypeData data)
        {
            for (var index = 0; index < groups.Count; index++)
            {
                var group = groups[index];
                group.OnArchetypeRemoved(data);
            }
        }

        private struct EntityLink
        {
            public Entity Entity;
            public ArchetypeData Data;
            public int IndexInData;
        }

        public class EntitiesGroup : Group
        {
            public Collections.Entities Entities;
        }
    }
}