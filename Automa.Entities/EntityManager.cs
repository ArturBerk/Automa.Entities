using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automa.Entities.Debugging;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public sealed class EntityManager : ManagerBase, IEnumerable<Entity>
    {
        internal readonly Queue<int> availableIndices = new Queue<int>();
        private readonly Dictionary<uint, EntityTypeData> entityTypeDatas = new Dictionary<uint, EntityTypeData>();
        private ComponentType[] componentTypeCache = new ComponentType[10];
        internal ArrayList<EntityLink> entityLinks = new ArrayList<EntityLink>(4);
        private ArrayList<GroupSlot> groups = new ArrayList<GroupSlot>(4);

        public EntityManager() : this(false)
        {
        }

        public EntityManager(bool debug)
        {
            this.debug = debug;
            if (debug)
            {
                DebugInfo = new EntityManagerDebugInfo(groups.Select(slot => slot.DebugInfo).ToArray());
            }
        }

        public IEnumerable<EntityReference> EntityReferences => new ReferenceEnumerable(this);

        internal IEnumerable<EntityTypeData> Datas => entityTypeDatas.Values;

        public int EntityCount => entityLinks.Count - availableIndices.Count;

        public IEnumerator<Entity> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public EntityReference CreateEntityReferenced(params ComponentType[] types)
        {
            return GetReference(CreateEntity(types));
        }

        public EntityReference GetReference(Entity entity)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            return new EntityReference(entityLink, this);
        }

        public bool IsExists(Entity entity)
        {
            var entityLink = entityLinks[entity.Id];
            return entityLink.Entity == entity;
        }

        public Entity CreateEntity(params ComponentType[] types)
        {
            var entityType = new EntityType(types);
            var data = GetOrCreateData(entityType.Hash, types, types.Length);
            var entityId = availableIndices.Count > 0 ? availableIndices.Dequeue() : entityLinks.Count;
            var version = 0;
            EntityLink link;
            if (entityLinks.Count > entityId)
            {
                link = entityLinks[entityId];
                version = link.Entity.Version + 1;
            }
            else
            {
                link = new EntityLink();
                entityLinks.SetAt(entityId, link);
            }
            var entity = new Entity(entityId, version);
            var dataIndex = data.AddEntity(entity);

            link.Data = data;
            link.IndexInData = dataIndex;
            link.Entity = entity;

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
            entityLink.Entity = Entity.Null;
            availableIndices.Enqueue(entity.Id);
        }

        internal void HandleEntityRemoving((int entityId, int newIndexInChunk) removeData)
        {
            if (removeData.entityId >= 0)
            {
                entityLinks[removeData.entityId].IndexInData = removeData.newIndexInChunk;
            }
        }

        public void AddComponent<T>(Entity entity, T component)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var entityType = entityLink.Data.EntityType;

            if (componentTypeCache.Length < entityType.Types.Length + 1)
            {
                Array.Resize(ref componentTypeCache, entityType.Types.Length + 1);
            }
            var newComponentType = ComponentType.Create<T>();
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

            MoveEntity(entityLink, data, entityType.Types, entityType.Types.Length);
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

        public void AddComponents(Entity entity, params ComponentType[] addComponents)
        {
            var entityLink = entityLinks[entity.Id];
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
            for (var i = 0; i < entityType.Types.Length; i++)
            {
                var entityTypeType = entityType.Types[i];
                for (var j = 0; j < addComponents.Length; j++)
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
                componentTypeCache[index++] = entityTypeType;
                previousTypeId = entityTypeType.TypeId;
            }
            while (addedComponentTypes < addComponents.Length)
            {
                var minimalId = int.MaxValue;
                var minimalComponentType = new ComponentType();
                for (var j = 0; j < addComponents.Length; j++)
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
            // If entity type not changed => return
            if (!entityTypeChanged) return;

            var entityTypeHash = EntityType.CalculateHash(componentTypeCache, index);
            var data = GetOrCreateData(entityTypeHash, componentTypeCache, index);

            MoveEntity(entityLink, data, entityType.Types, entityType.Types.Length);
        }

        public void RemoveComponents(Entity entity, params ComponentType[] removeComponents)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var entityType = entityLink.Data.EntityType;

            if (componentTypeCache.Length < entityType.Types.Length + 1)
            {
                Array.Resize(ref componentTypeCache, entityType.Types.Length + 1);
            }

            var index = 0;
            var entityTypeChanged = false;
            for (var i = 0; i < entityType.Types.Length; i++)
            {
                var entityTypeType = entityType.Types[i];
                for (var j = 0; j < removeComponents.Length; j++)
                {
                    if (removeComponents[j] == entityTypeType)
                    {
                        entityTypeChanged = true;
                        goto continueNextType;
                    }
                }
                componentTypeCache[index++] = entityTypeType;
                continueNextType:
                ;
            }

            // If entity type not changed => return
            if (!entityTypeChanged) return;

            var entityTypeHash = EntityType.CalculateHash(componentTypeCache, index);
            var data = GetOrCreateData(entityTypeHash, componentTypeCache, index);

            MoveEntity(entityLink, data, entityType.Types, entityType.Types.Length);
        }

        public void ChangeComponents(Entity entity,
            ComponentType[] addComponents,
            ComponentType[] removeComponents)
        {
            var entityLink = entityLinks[entity.Id];
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
            for (var i = 0; i < entityType.Types.Length; i++)
            {
                var entityTypeType = entityType.Types[i];

                for (var j = 0; j < addComponents.Length; j++)
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
                for (var j = 0; j < removeComponents.Length; j++)
                {
                    if (removeComponents[j] == entityTypeType)
                    {
                        entityTypeChanged = true;
                        goto continueNextType;
                    }
                }
                componentTypeCache[index++] = entityTypeType;
                continueNextType:
                previousTypeId = entityTypeType.TypeId;
            }
            while (addedComponentTypes < addComponents.Length)
            {
                var minimalId = int.MaxValue;
                var minimalComponentType = new ComponentType();
                for (var j = 0; j < addComponents.Length; j++)
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

            // If entity type not changed => return
            if (!entityTypeChanged) return;

            var entityTypeHash = EntityType.CalculateHash(componentTypeCache, index);
            var data = GetOrCreateData(entityTypeHash, componentTypeCache, index);

            MoveEntity(entityLink, data, entityType.Types, entityType.Types.Length);
        }

        public void RemoveComponent<T>(Entity entity)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var entityType = entityLink.Data.EntityType;

            if (componentTypeCache.Length < entityType.Types.Length - 1)
            {
                Array.Resize(ref componentTypeCache, entityType.Types.Length - 1);
            }
            var newComponentType = ComponentType.Create<T>();
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
            MoveEntity(entityLink, data, newEntityType.Types, newEntityType.Types.Length);
        }

        private void MoveEntity(EntityLink entityLink, EntityTypeData data, ComponentType[] copyTypesBasedOn,
            int typeCount)
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
            if (entityLink.Entity.Id != entity.Id && entityLink.Entity.Version != entity.Version)
                throw new ArgumentException("Entity not found");
            return ref entityLink.Data.GetComponentArray<T>()[entityLink.IndexInData];
        }

        public T RegisterGroup<T>(T group) where T : Group
        {
            RegisterGroup((Group) group);
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
                DebugInfo = new EntityManagerDebugInfo(groups.Select(slot => slot.DebugInfo).ToArray());
            }
        }

        public void UnregisterGroup(Group group)
        {
            for (var i = 0; i < groups.Count; i++)
            {
                if (Equals(groups[i].Group, group))
                {
                    groups.RemoveAt(i);
                    if (debug)
                    {
                        DebugInfo = new EntityManagerDebugInfo(groups.Select(slot => slot.DebugInfo).ToArray());
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

        public override void OnUpdate()
        {
            // Apply group additions
            for (int i = 0; i < groups.Count; ++i)
            {
                groups[i].Group.HandleModifications();
            }
        }

        internal class EntityLink
        {
            public EntityTypeData Data;
            public Entity Entity;
            public int IndexInData;

            public override string ToString()
            {
                return $"{Entity} => {IndexInData} in \"{Data.EntityType}\"";
            }
        }

        private struct GroupSlot
        {
            public readonly Group Group;
            public readonly GroupDebugInfo DebugInfo;

            public GroupSlot(Group group) : this()
            {
                Group = group;
                DebugInfo = new GroupDebugInfo(group);
            }
        }

        private struct Enumerator : IEnumerator<Entity>
        {
            private readonly EntityManager entityManager;
            private int index;
            private Entity currentEntity;

            public Enumerator(EntityManager entityManager) : this()
            {
                this.entityManager = entityManager;
                index = -1;
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (++index >= entityManager.entityLinks.Count) return false;
                    var link = entityManager.entityLinks[index];
                    currentEntity = link.Entity;
                    if (link.Entity != Entity.Null) return true;
                }
            }

            public void Reset()
            {
                index = -1;
            }

            public Entity Current => currentEntity;

            object IEnumerator.Current => currentEntity;

            public void Dispose()
            {
                Reset();
            }
        }

        private struct ReferenceEnumerable : IEnumerable<EntityReference>
        {
            private readonly EntityManager entityManager;

            public ReferenceEnumerable(EntityManager entityManager)
            {
                this.entityManager = entityManager;
            }

            public IEnumerator<EntityReference> GetEnumerator()
            {
                return new ReferenceEnumerator(entityManager);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ReferenceEnumerator(entityManager);
            }
        }

        private struct ReferenceEnumerator : IEnumerator<EntityReference>
        {
            private readonly EntityManager entityManager;
            private int index;
            private EntityLink currentEntityLink;

            public ReferenceEnumerator(EntityManager entityManager) : this()
            {
                this.entityManager = entityManager;
                index = -1;
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (++index >= entityManager.entityLinks.Count) return false;
                    var link = entityManager.entityLinks[index];
                    currentEntityLink = link;
                    if (link.Entity != Entity.Null) return true;
                }
            }

            public void Reset()
            {
                index = -1;
            }

            public EntityReference Current => new EntityReference(currentEntityLink, entityManager);

            object IEnumerator.Current => new EntityReference(currentEntityLink, entityManager);

            public void Dispose()
            {
                Reset();
            }
        }

        #region Debugging

        private readonly bool debug;
        public EntityManagerDebugInfo DebugInfo { get; private set; }

        #endregion
    }
}