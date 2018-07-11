using System.Runtime.CompilerServices;

namespace Automa.Entities
{
    public struct EntityReference
    {
        private readonly EntityManager.EntityLink entityLink;
        private readonly EntityManager entityManager;

        public Entity Entity => entityLink.Entity;

        internal EntityReference(EntityManager.EntityLink entityLink, EntityManager entityManager)
        {
            this.entityLink = entityLink;
            this.entityManager = entityManager;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>()
        {
            return ref entityLink.Data.GetComponentArray<T>()[entityLink.IndexInData];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(T component)
        {
            entityLink.Data.SetComponent(entityLink.IndexInData, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>()
        {
            return entityLink.Data.HasComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove()
        {
            entityManager.HandleEntityRemoving(entityLink.Data.RemoveEntity(entityLink.IndexInData, null));
            entityLink.Entity = Entity.Null;
            entityManager.availableIndices.Enqueue(entityLink.Entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(T component)
        {
            entityManager.AddComponent(entityLink.Entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents(params ComponentType[] componentTypes)
        {
            entityManager.AddComponents(entityLink.Entity, componentTypes);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents(params ComponentType[] componentTypes)
        {
            entityManager.RemoveComponents(entityLink.Entity, componentTypes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeComponents(
            ComponentType[] addComponents,
            ComponentType[] removeComponents)
        {
            entityManager.ChangeComponents(entityLink.Entity, addComponents, removeComponents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>()
        {
            entityManager.RemoveComponent<T>(entityLink.Entity);
        }
    }
}