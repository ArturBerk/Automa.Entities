using System;
using System.Collections.Generic;
using System.Linq;
using Automa.Collections;

namespace Automa.Entities
{
    public class EntityManager
    {
        private FastList<EntityLink> entityLinks;
        private readonly Dictionary<uint, EntityChunk> archetypeChunks = new Dictionary<uint, EntityChunk>();
        private readonly Queue<int> availableIndices = new Queue<int>();

        public Entity CreateEntity(ComponentType[] types)
        {
            var archetype = new EntityArchetype(types);
            var chunk = GetOrCreateChunk(archetype.Index, types, types.Length);
            var entityId = availableIndices.Count > 0 ? availableIndices.Dequeue() : entityLinks.Count;
            var version = 0;
            if (entityLinks.Count > entityId)
            {
                version = entityLinks[entityId].Entity.Version + 1;
            }
            else
            {
                entityLinks.Insert(entityId, new EntityLink());
            }
            Entity entity = new Entity(entityId, version);
            var chunkIndex = chunk.AddEntity(entity);
            entityLinks[entityId] = new EntityLink
            {
                Chunk = chunk,
                IndexInChunk = chunkIndex,
                Entity = entity
            };
            return entity;
        }

        public void SetComponent<T>(Entity entity, T component) where T : IComponent
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            entityLink.Chunk.SetComponent(entityLink.IndexInChunk, component);
        }

        private ComponentType[] componentTypeCache = new ComponentType[10];

        public void AddComponent<T>(Entity entity, T component) where T : IComponent
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var archetype = entityLink.Chunk.Archetype;


            if (componentTypeCache.Length < archetype.Types.Length + 1)
            {
                Array.Resize(ref componentTypeCache, archetype.Types.Length + 1);
            }
            ComponentType newComponentType = typeof(T);
            var index = 0;
            for (int i = 0; i < archetype.Types.Length; i++)
            {
                var archetypeType = archetype.Types[i];
                if (archetypeType.TypeId < newComponentType.TypeId)
                {
                    componentTypeCache[index++] = archetypeType;
                }
            }
            componentTypeCache[index++] = newComponentType;
            for (int i = 0; i < archetype.Types.Length; i++)
            {
                componentTypeCache[index++] = archetype.Types[i];
            }
            var archetypeIndex = EntityArchetype.CalculateIndex(componentTypeCache, index);
            var chunk = GetOrCreateChunk(archetypeIndex, componentTypeCache, index);

            entityLink.Chunk.RemoveEntity(entityLink.IndexInChunk);
            chunk.SetComponent(entityLink.IndexInChunk, component);
            entityLinks[entity.Id] = new EntityLink
            {
                Entity = entity,
                Chunk = chunk,
                IndexInChunk = chunk.AddEntity(entity)
            };
        }

        private EntityChunk GetOrCreateChunk(uint index, ComponentType[] components, int count)
        {
            if (archetypeChunks.TryGetValue(index, out var chunk))
            {
                chunk = new EntityChunk(new EntityArchetype(index, components, count));
                archetypeChunks.Add(index, chunk);
            }
            return chunk;
        }

        public void RemoveComponent<T>(Entity entity)
        {
            var entityLink = entityLinks[entity.Id];
            if (entityLink.Entity != entity)
                throw new ArgumentException("Entity not found");
            var archetype = entityLink.Chunk.Archetype;

            if (componentTypeCache.Length < archetype.Types.Length - 1)
            {
                Array.Resize(ref componentTypeCache, archetype.Types.Length - 1);
            }
            ComponentType newComponentType = typeof(T);
            var index = 0;
            for (int i = 0; i < archetype.Types.Length; i++)
            {
                var archetypeType = archetype.Types[i];
                if (archetypeType.TypeId != newComponentType.TypeId)
                {
                    componentTypeCache[index++] = archetypeType;
                }
            }
            var archetypeIndex = EntityArchetype.CalculateIndex(componentTypeCache, index);
            var chunk = GetOrCreateChunk(archetypeIndex, componentTypeCache, index);

            entityLink.Chunk.RemoveEntity(entityLink.IndexInChunk);
            entityLinks[entity.Id] = new EntityLink
            {
                Entity = entity,
                Chunk = chunk,
                IndexInChunk = chunk.AddEntity(entity)
            };
        }
    }

    struct EntityArchetype
    {
        public readonly ComponentType[] Types;
        internal readonly uint Index;

        public EntityArchetype(ComponentType[] types)
        {
            Types = types;
            Array.Sort(Types, (c1, c2) => c2.TypeId - c1.TypeId);
            Index = CalculateIndex(types, types.Length);
        }

        public EntityArchetype(uint index, ComponentType[] types, int typeCount)
        {
            Types = new ComponentType[typeCount];
            Array.Copy(types, Types, typeCount);
            Array.Sort(Types, (c1, c2) => c2.TypeId - c1.TypeId);
            Index = index;
        }

        public static uint CalculateIndex(ComponentType[] types, int count)
        {
            return HashUtility.Fletcher32(types, count);
        }
    }

    struct EntityLink
    {
        public Entity Entity;
        public EntityChunk Chunk;
        public int IndexInChunk;
    }

    class EntityChunk
    {
        public readonly EntityArchetype Archetype;
        private int count;

        private readonly ChunkArray<Entity> entityArray;
        private IChunkArray[] componentArrays;
        private readonly Queue<int> availableIndices = new Queue<int>();

        public ChunkArray<T> GetComponentArray<T>()
        {
            var r = componentArrays[((ComponentType)typeof(T)).TypeId];
            if (r == null) throw new ArgumentException($"Chunk not contains component of type {typeof(T)}");
            return (ChunkArray<T>)r;
        }

        public EntityChunk(EntityArchetype archetype)
        {
            this.Archetype = archetype;
            entityArray = new ChunkArray<Entity>();
            InitializeArrays(archetype.Types);
        }

        private void InitializeArrays(ComponentType[] types)
        {
            componentArrays = new IChunkArray[ComponentTypeManager.TypeCount];
            Type t = typeof(ChunkArray<>);
            for (int i = 0; i < types.Length; i++)
            {
                var componentType = types[i];
                componentArrays[componentType.TypeId] = (IChunkArray)Activator.CreateInstance(
                    t.MakeGenericType(componentType));
            }
        }

        public int AddEntity(Entity entity)
        {
            var index = availableIndices.Count > 0 ? availableIndices.Dequeue() : count;
            entityArray.Insert(index, entity);
            foreach (var componentArray in componentArrays)
            {
                componentArray.InsertDefault(index);
            }
            count += 1;
            return index;
        }

        public void RemoveEntity(int index)
        {
            availableIndices.Enqueue(index);
            entityArray.InsertDefault(index);
            for (int i = 0; i < componentArrays.Length; i++)
            {
                componentArrays[i].InsertDefault(index);
            }
        }

        public void SetComponent<T>(int index, T component)
        {
            GetComponentArray<T>().Insert(index, component);
        }

        public bool HasComponent<T>()
        {
            return componentArrays[((ComponentType)typeof(T)).TypeId] != null;
        }
    }

    interface IChunkArray
    {
        void InsertDefault(int index);
    }

    class ChunkArray<T> : FastList<T>, IChunkArray
    {
        public void InsertDefault(int index)
        {
            base.Insert(index, default(T));
        }
    }

}
