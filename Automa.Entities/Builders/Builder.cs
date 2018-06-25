using System;
using System.Collections.Generic;
using System.Linq;
using Automa.Entities.Internal;

namespace Automa.Entities.Builders
{
    public class Builder
    {
        public readonly EntityBuilder[] Entities;

        public Builder(IEnumerable<EntityBuilder> entities)
        {
            Entities = entities.ToArray();
        }

        public Builder(params EntityBuilder[] entities)
        {
            Entities = entities;
        }

        public void Build(EntityManager entityManager, Action<ReadOnlyArray<EntityReference>> doOnEntities = null)
        {
            try
            {
                for (var index = 0; index < Entities.Length; index++)
                {
                    var entityTemplate = Entities[index];
                    var entityReference = entityTemplate.CreateEntity(entityManager);
                    BuilderHelper.CreatedEntities.Add(entityReference);
                    var name = entityTemplate.Name;
                    if (name != null)
                    {
                        BuilderHelper.CreatedEntitiesByName.Add(name, entityReference);
                    }
                }
                for (var index = 0; index < Entities.Length; index++)
                {
                    var entityTemplate = Entities[index];
                    entityTemplate.BuildComponents(ref BuilderHelper.CreatedEntities[index],
                        BuilderHelper.CreatedEntitiesByName);
                }
                doOnEntities?.Invoke(new ReadOnlyArray<EntityReference>(
                    BuilderHelper.CreatedEntities.Buffer,
                    BuilderHelper.CreatedEntities.Count));
            }
            finally
            {
                BuilderHelper.CreatedEntities.Clear();
                BuilderHelper.CreatedEntitiesByName.Clear();
            }
        }
    }

    public class Builder<TParameter>
    {
        public readonly EntityBuilder<TParameter>[] Entities;

        public Builder(IEnumerable<EntityBuilder<TParameter>> entities)
        {
            Entities = entities.ToArray();
        }

        public Builder(params EntityBuilder<TParameter>[] entities)
        {
            Entities = entities;
        }

        public void Create(EntityManager entityManager, ref TParameter parameter)
        {
            for (var index = 0; index < Entities.Length; index++)
            {
                var entityTemplate = Entities[index];
                var entityReference = entityTemplate.CreateEntity(entityManager, ref parameter);
                BuilderHelper.CreatedEntities.Add(entityReference);
                var name = entityTemplate.Name;
                if (name != null)
                {
                    BuilderHelper.CreatedEntitiesByName.Add(name, entityReference);
                }
            }
            for (var index = 0; index < Entities.Length; index++)
            {
                var entityTemplate = Entities[index];
                entityTemplate.BuildComponents(ref BuilderHelper.CreatedEntities[index],
                    ref parameter, BuilderHelper.CreatedEntitiesByName);
            }
            BuilderHelper.CreatedEntities.Clear();
            BuilderHelper.CreatedEntitiesByName.Clear();
        }
    }

    internal class BuilderHelper
    {
        public static ArrayList<EntityReference> CreatedEntities =
            new ArrayList<EntityReference>(4);

        public static readonly Dictionary<string, EntityReference> CreatedEntitiesByName =
            new Dictionary<string, EntityReference>();
    }
}