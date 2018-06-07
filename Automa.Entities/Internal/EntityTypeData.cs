using System;

namespace Automa.Entities.Internal
{
    internal class EntityTypeData
    {
        public readonly EntityType EntityType;
        private IComponentArray[] componentArrays;
        private int[] componentTypeIndices;
        private int count;

        private ComponentArray<Entity> entityArray;

        public EntityTypeData(EntityType entityType)
        {
            EntityType = entityType;
            InitializeArrays(entityType.Types);
        }

        public IComponentArray GetComponentArrayUnchecked(ComponentType type)
        {
            return componentArrays[type.TypeId];
        }

        public ComponentArray<T> GetComponentArray<T>()
        {
            var r = componentArrays[((ComponentType) typeof(T)).TypeId];
            if (r == null) throw new ArgumentException($"Chunk not contains component of type {typeof(T)}");
            return (ComponentArray<T>) r;
        }

        private void InitializeArrays(ComponentType[] types)
        {
            componentArrays = new IComponentArray[ComponentTypeManager.TypeCount];
            var t = typeof(ComponentArray<>);
            componentTypeIndices = new int[types.Length];
            for (var i = 0; i < types.Length; i++)
            {
                var componentType = types[i];
                componentArrays[componentType.TypeId] = (IComponentArray) Activator.CreateInstance(
                    t.MakeGenericType(componentType));
                componentTypeIndices[i] = componentType.TypeId;
            }
            entityArray = new ComponentArray<Entity>();
            componentArrays[0] = entityArray;
        }

        public int AddEntity(Entity entity)
        {
            var index = count;
            entityArray.SetWithExpand(index, entity);
            for (var i = 0; i < componentTypeIndices.Length; i++)
            {
                componentArrays[componentTypeIndices[i]].SetDefault(index);
            }
            count += 1;
            return index;
        }

        public (int entityId, int newIndexInChunk) RemoveEntity(int index)
        {
            entityArray.UnorderedRemoveAt(index);
            for (var i = 0; i < componentTypeIndices.Length; i++)
            {
                componentArrays[componentTypeIndices[i]].UnorderedRemoveAt(index);
            }
            --count;
            return (entityArray[index].Id, index);
        }


        public void SetComponent<T>(int index, T component)
        {
            GetComponentArray<T>().SetWithExpand(index, component);
        }

        public bool HasComponent<T>()
        {
            var typeId = ((ComponentType) typeof(T)).TypeId;
            return componentArrays.Length > typeId && componentArrays[typeId] != null;
        }
    }
}