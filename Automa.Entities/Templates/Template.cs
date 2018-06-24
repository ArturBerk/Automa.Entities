using System.Collections.Generic;
using System.Linq;

namespace Automa.Entities.Templates
{
    public class Template
    {
        public readonly EntityTemplate[] Entities;

        public Template(IEnumerable<EntityTemplate> entities)
        {
            Entities = entities.ToArray();
        }

        public Template(params EntityTemplate[] entities)
        {
            Entities = entities.ToArray();
        }

        public void Create(EntityManager entityManager)
        {
            
        }
    }

    public class EntityTemplate
    {
        public readonly IComponentTemplate[] Components;
        public readonly string Name;
        private readonly ComponentType[] entityType;

        public EntityTemplate(string name, IEnumerable<IComponentTemplate> components)
        {
            Name = name;
            Components = components.ToArray();
            entityType = Components.SelectMany(template => template.Types).ToArray();
        }

        public EntityTemplate(string name, params IComponentTemplate[] components)
        {
            Name = name;
            Components = components;
            entityType = Components.SelectMany(template => template.Types).ToArray();
        }

        public EntityReference Create(EntityManager entityManager)
        {
            var entity = entityManager.CreateEntityReferenced(entityType);
            CreateComponents(entityManager, null);
            return entity;
        }

        internal EntityReference CreateEntity(EntityManager entityManager)
        {
            return entityManager.CreateEntityReferenced(entityType);
        }

        internal void CreateComponents(EntityManager entityManager, Dictionary<string, EntityReference> templateEntities)
        {

        }
    }


    public interface IComponentTemplate
    {
        ComponentType[] Types { get; }
        void Build(EntityReference entity, Dictionary<string, EntityReference> templateEntities);
    }
}
