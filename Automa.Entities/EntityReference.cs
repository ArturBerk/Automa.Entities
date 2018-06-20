using System.Runtime.CompilerServices;

namespace Automa.Entities
{
    public struct EntityReference
    {
        public readonly Entity Entity;
        public readonly EntityManager EntityManager;

        public EntityReference(Entity entity, EntityManager entityManager)
        {
            Entity = entity;
            EntityManager = entityManager;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>()
        {
            return ref EntityManager.GetComponent<T>(Entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(T component)
        {
            EntityManager.SetComponent(Entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>()
        {
            return EntityManager.HasComponent<T>(Entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove()
        {
            EntityManager.RemoveEntity(Entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(T component)
        {
            EntityManager.AddComponent(Entity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeComponents(
            ComponentType[] addComponents,
            ComponentType[] removeComponents)
        {
            EntityManager.ChangeComponents(Entity, addComponents, removeComponents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>()
        {
            EntityManager.RemoveComponent<T>(Entity);
        }
    }
}