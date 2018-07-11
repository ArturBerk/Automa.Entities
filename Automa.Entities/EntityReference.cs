using System.Runtime.CompilerServices;

namespace Automa.Entities
{
    public struct EntityReference
    {
        public readonly Entity Entity;
        private readonly EntityManager entityManager;

        internal EntityReference(Entity entityLink, EntityManager entityManager)
        {
            Entity = entityLink;
            this.entityManager = entityManager;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>()
        {
            return ref entityManager.GetComponent<T>(Entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(T component)
        {
            entityManager.SetComponent(Entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>()
        {
            return entityManager.HasComponent<T>(Entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove()
        {
            entityManager.RemoveEntity(Entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(T component)
        {
            entityManager.AddComponent(Entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents(params ComponentType[] componentTypes)
        {
            entityManager.AddComponents(Entity, componentTypes);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents(params ComponentType[] componentTypes)
        {
            entityManager.RemoveComponents(Entity, componentTypes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeComponents(
            ComponentType[] addComponents,
            ComponentType[] removeComponents)
        {
            entityManager.ChangeComponents(Entity, addComponents, removeComponents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>()
        {
            entityManager.RemoveComponent<T>(Entity);
        }
    }
}