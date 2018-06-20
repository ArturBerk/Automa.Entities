using System;
using System.Collections.Generic;
using System.Linq;
using Automa.Entities.Debugging;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public sealed class EntityManager : IManager
    {
        private readonly Queue<int> availableIndices = new Queue<int>();

        private readonly AllEntities allEntities;
        private ArrayList<EntityLink> entityLinks = new ArrayList<EntityLink>(4);
        private readonly Dictionary<uint, EntityTypeData> entityTypeDatas = new Dictionary<uint, EntityTypeData>();

        private ArrayList<GroupSlot> groups = new ArrayList<GroupSlot>(4);

        private ComponentType[] componentTypeCache = new ComponentType[10];

        #region Debugging
        private bool debug;
        private EntityManagerDebugInfo debugInfo;
        public EntityManagerDebugInfo DebugInfo => debugInfo;
        #endregion

        public EntityManager() : this(false)
        {
        }

        public EntityManager(bool debug)
        {
            allEntities = RegisterGroup(new AllEntities());
            this.debug = debug;
            if (debug)
            {
                debugInfo = new EntityManagerDebugInfo(groups.Select(slot => slot.DebugInfo).ToArray());
            }
        }
        /// <summary>
        /// 27716
        /// </summary>

        public Collections.EntityCollection Entities => allEntities.Entities;

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
            // Need manual update groups in systems
//            foreach (var group in groups)
//            {
//                group.Group.Update();
//            }
        }

        public EntityReference CreateEntityReferenced(params ComponentType[] types)
        {
            return new EntityReference(CreateEntity(types), this);
        }

        public EntityReference GetReference(Entity entity)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            return new EntityReference(entity, this);
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
                entityLinks.SetAt(entityId, new EntityLink());
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
            if (removeData.entityId >= 0)
            {
                entityLinks[removeData.entityId].IndexInData = removeData.newIndexInChunk;
            }
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

            MoveEntity(ref entityLink, data, entityType.Types, entityType.Types.Length);
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

        public void ChangeComponents(Entity entity,
            ComponentType[] addComponents,
            ComponentType[] removeComponents)
        {
            ref var entityLink = ref entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var entityType = entityLink.Data.EntityType;

            if (componentTypeCache.Length < entityType.Types.Length + 1)
            {
                Array.Resize(ref componentTypeCache, entityType.Types.Length + 1);
            }

            var index = 0;
            var previousTypeId = -1;
            var entityTypeChanged = false;
            var addedComponentTypes = 0;
            for (int i = 0; i < entityType.Types.Length; i++)
            {
                var entityTypeType = entityType.Types[i];
                if (addComponents != null)
                {
                    for (int j = 0; j < addComponents.Length; j++)
                    {
                        var componentTypeToAdd = addComponents[j];
                        if (componentTypeToAdd.TypeId > previousTypeId
                            && componentTypeToAdd.TypeId < entityTypeType.TypeId)
                        {
                            componentTypeCache[index++] = componentTypeToAdd;
                            ++addedComponentTypes;
                            entityTypeChanged = true;
                        }
                        else if (componentTypeToAdd.TypeId == entityTypeType.TypeId)
                        {
                            ++addedComponentTypes;
                        }
                    }
                }
                if (removeComponents != null)
                {
                    for (int j = 0; j < removeComponents.Length; j++)
                    {
                        if (removeComponents[j] == entityTypeType)
                        {
                            entityTypeChanged = true;
                            goto continueNextType;
                        }
                    }
                }
                componentTypeCache[index++] = entityTypeType;
                previousTypeId = entityTypeType.TypeId;
                continueNextType:
                ;
            }
            if (addComponents != null)
            {
                while (addedComponentTypes < addComponents.Length)
                {
                    var minimalId = int.MaxValue;
                    var minimalComponentType = new ComponentType();
                    for (int j = 0; j < addComponents.Length; j++)
                    {
                        var componentTypeToAdd = addComponents[j];
                        if (componentTypeToAdd.TypeId > previousTypeId
                            && componentTypeToAdd.TypeId < minimalId)
                        {
                            minimalId = componentTypeToAdd.TypeId;
                            minimalComponentType = componentTypeToAdd;
                        }
                    }
                    previousTypeId = minimalId;
                    componentTypeCache[index++] = minimalComponentType;
                    ++addedComponentTypes;
                    entityTypeChanged = true;
                }
            }

            // If entity type not changed => return
            if (!entityTypeChanged) return;

            var entityTypeHash = EntityType.CalculateHash(componentTypeCache, index);
            var data = GetOrCreateData(entityTypeHash, componentTypeCache, index);

            MoveEntity(ref entityLink, data, entityType.Types, entityType.Types.Length);
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
            var removed = false;
            for (var i = 0; i < entityType.Types.Length; i++)
            {
                var entityComponentType = entityType.Types[i];
                if (entityComponentType.TypeId != newComponentType.TypeId)
                {
                    componentTypeCache[index++] = entityComponentType;
                }
                else
                {
                    removed = true;
                }
            }
            if (!removed) throw new ApplicationException("Entity not contains component of type " + typeof(T));
            var entityTypeHash = EntityType.CalculateHash(componentTypeCache, index);
            var data = GetOrCreateData(entityTypeHash, componentTypeCache, index);

            var newEntityType = data.EntityType;
            MoveEntity(ref entityLink, data, newEntityType.Types, newEntityType.Types.Length);
        }

        private void MoveEntity(ref EntityLink entityLink, EntityTypeData data, ComponentType[] copyTypesBasedOn, int typeCount)
        {
            // Add new entity to new entity type data
            var newEntityIndexInData = data.AddEntity(entityLink.Entity);
            // Copy component data from old to new entity type data
            for (var i = 0; i < typeCount; i++)
            {
                var type = copyTypesBasedOn[i];
                var sourceArray = entityLink.Data.GetComponentArray(type);
                var destArray = data.GetComponentArray(type);
                if (sourceArray != null && destArray != null)
                {
                    destArray.CopyFrom(sourceArray, entityLink.IndexInData, newEntityIndexInData);
                }
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
            RegisterGroup((Group)group);
            return group;
        }

        internal void RegisterGroup(Group group)
        {
            var newSlot = new GroupSlot(group);
            groups.Add(newSlot);
            group.Register(this);
            group.Update();
            if (debug)
            {
                debugInfo = new EntityManagerDebugInfo(groups.Select(slot => slot.DebugInfo).ToArray());
            }
        }

        public void UnregisterGroup(Group group)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (Equals(groups[i].Group, group))
                {
                    groups.RemoveAt(i);
                    if (debug)
                    {
                        debugInfo = new EntityManagerDebugInfo(groups.Select(slot => slot.DebugInfo).ToArray());
                    }
                    return;
                }
            }
        }

        private void OnEntityTypeAdd(ref EntityTypeData data)
        {
            for (var index = 0; index < groups.Count; index++)
            {
                var group = groups[index];
                group.Group.OnEntityTypeAdd(data);
            }
        }

        private void OnEntityTypeRemove(ref EntityTypeData data)
        {
            for (var index = 0; index < groups.Count; index++)
            {
                var group = groups[index];
                group.Group.OnEntityTypeRemoved(data);
            }
        }

        private struct EntityLink
        {
            public Entity Entity;
            public EntityTypeData Data;
            public int IndexInData;

            public override string ToString()
            {
                return $"{Entity} => {IndexInData} in \"{Data.EntityType}\"";
            }
        }

        private class AllEntities : Group
        {
            public Collections.EntityCollection Entities;
        }

        private struct GroupSlot
        {
            public readonly Group Group;
            public readonly GroupDebugInfo DebugInfo;

            public GroupSlot(Group @group) : this()
            {
                Group = @group;
                DebugInfo = new GroupDebugInfo(group);
            }
        }
    }
}