using System.Collections.Generic;

namespace Automa.Entities.Builders
{
    public interface IComponentBuilder
    {
        ComponentType[] Types { get; }

        void Build(ref EntityReference entity,
            Dictionary<string, EntityReference> templateEntities);
    }

    public interface IComponentBuilder<TParameter>
    {
        ComponentType[] Types { get; }

        void Build(ref EntityReference entity, ref TParameter parameter,
            Dictionary<string, EntityReference> templateEntities);
    }
}