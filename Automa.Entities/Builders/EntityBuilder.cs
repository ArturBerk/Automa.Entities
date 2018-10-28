using System.Collections.Generic;
using System.Linq;

namespace Automa.Entities.Builders
{
    public class EntityBuilder
    {
        public readonly IComponentBuilder[] Components;
        private readonly ComponentType[] entityType;
        public readonly string Name;

        public EntityBuilder(IEnumerable<IComponentBuilder> components): this(null, components)
        {
            
        }

        public EntityBuilder(params IComponentBuilder[] components) : this(null, components)
        {

        }

        public EntityBuilder(string name, IEnumerable<IComponentBuilder> components)
        {
            Name = name;
            Components = components.ToArray();
            entityType = this.Components.SelectMany(template => template.Types).ToArray();
        }

        public EntityBuilder(string name, params IComponentBuilder[] components)
        {
            Name = name;
            Components = components;
            entityType = components.SelectMany(template => template.Types).ToArray();
        }

        public EntityReference Build(EntityManager entityManager)
        {
            var entity = entityManager.CreateEntityReferenced(entityType);
            BuildComponents(ref entity, null);
            return entity;
        }

        internal EntityReference CreateEntity(EntityManager entityManager)
        {
            return entityManager.CreateEntityReferenced(entityType);
        }

        internal void BuildComponents(ref EntityReference entity,
            Dictionary<string, EntityReference> templateEntities)
        {
            for (var index = 0; index < Components.Length; index++)
            {
                Components[index].Build(ref entity, templateEntities);
            }
        }
    }

    public class EntityBuilder<TParameter>
    {
        public readonly IComponentBuilder<TParameter>[] Components;
        private readonly ComponentType[] entityType;
        public readonly string Name;

        public EntityBuilder(IEnumerable<IComponentBuilder<TParameter>> components) : this(null, components)
        {

        }

        public EntityBuilder(params IComponentBuilder<TParameter>[] components) : this(null, components)
        {

        }

        public EntityBuilder(string name, IEnumerable<IComponentBuilder<TParameter>> components)
        {
            Name = name;
            Components = components.ToArray();
            entityType = this.Components.SelectMany(template => template.Types).ToArray();
        }

        public EntityBuilder(string name, params IComponentBuilder<TParameter>[] components)
        {
            Name = name;
            Components = components;
            entityType = components.SelectMany(template => template.Types).ToArray();
        }

        public EntityReference Build(EntityManager entityManager, ref TParameter parameter)
        {
            var entity = entityManager.CreateEntityReferenced(entityType);
            BuildComponents(ref entity, ref parameter, null);
            return entity;
        }

        internal EntityReference CreateEntity(EntityManager entityManager, ref TParameter parameter)
        {
            return entityManager.CreateEntityReferenced(entityType);
        }

        internal void BuildComponents(ref EntityReference entity, ref TParameter parameter,
            Dictionary<string, EntityReference> templateEntities)
        {
            for (var index = 0; index < Components.Length; index++)
            {
                Components[index].Build(ref entity, ref parameter, templateEntities);
            }
        }
    }
}