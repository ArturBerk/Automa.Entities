using System;
using System.Runtime.CompilerServices;
using Automa.Common;

namespace Automa.Entities.Internal
{
    internal sealed class EntityTypeData
    {
        public readonly EntityType EntityType;
        private IComponentArray[] componentArrays;
        private int[] componentTypeIndices;
        internal int count;

        internal ArrayList<IEntityAddedListener> addedListeners = new ArrayList<IEntityAddedListener>(1);
        internal ArrayList<IEntityRemovingListener> removingListeners = new ArrayList<IEntityRemovingListener>(1);

        internal ComponentArray<Entity> entityArray;

        public EntityTypeData(EntityType entityType)
        {
            EntityType = entityType;
            InitializeArrays(entityType.Types);
        }

        public IComponentArray GetComponentArray(ComponentType type)
        {
            return type.TypeId < componentArrays.Length ? componentArrays[type.TypeId] : null;
        }

        public ComponentArray<T> GetComponentArray<T>()
        {
            var r = componentArrays[ComponentTypeManager.GetTypeIndex<T>()];
            if (r == null) throw new ArgumentException($"Entity not contains component of type {typeof(T)}");
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

        public int AddEntity(Entity entity, EntityTypeData movedFrom)
        {
            var index = count;
            entityArray.SetAt(index, entity);
            for (var i = 0; i < componentTypeIndices.Length; i++)
            {
                componentArrays[componentTypeIndices[i]].SetDefault(index);
            }
            count += 1;

            if (addedListeners.Count > 0)
            {
                try
                {
                    for (int i = 0; i < addedListeners.Count; i++)
                    {
                        addedListeners[i].OnEntityAdded(entityArray[index], movedFrom);
                    }
                }
                catch
                {
                    //
                }
            }
            return index;
        }

        public (int entityId, int newIndexInChunk) RemoveEntity(int index, EntityTypeData movingTo)
        {
            --count;
            if (removingListeners.Count > 0)
            {
                try
                {
                    for (int i = 0; i < removingListeners.Count; i++)
                    {
                        removingListeners[i].OnEntityRemoving(entityArray[index], movingTo);
                    }
                }
                catch
                {
                    //
                }
            }
            entityArray.UnorderedRemoveAt(index);
            for (var i = 0; i < componentTypeIndices.Length; i++)
            {
                componentArrays[componentTypeIndices[i]].UnorderedRemoveAt(index);
            }
            return (index != count ? entityArray[index].Id : -1, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(int index, T component)
        {
            GetComponentArray<T>().SetAt(index, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>()
        {
            var typeId = ComponentTypeManager.GetTypeIndex<T>();
            return componentArrays.Length > typeId && componentArrays[typeId] != null;
        }

        public override string ToString()
        {
            return $"{nameof(EntityType)}: {EntityType}";
        }
    }

    internal interface IEntityAddedListener
    {
        void OnEntityAdded(Entity entity, EntityTypeData movedFrom);
    }

    internal interface IEntityRemovingListener
    {
        void OnEntityRemoving(Entity entity, EntityTypeData movingTo);
    }

}