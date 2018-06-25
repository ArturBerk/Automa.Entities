using System.Collections.Generic;

namespace Automa.Entities.Builders
{
    public class ComponentValue<TType> : IComponentBuilder
    {
        private static readonly ComponentType[] types = { ComponentType.Create<TType>() };

        private readonly TType value;

        public ComponentValue(TType value)
        {
            this.value = value;
        }

        public void Build(ref EntityReference entity,
            Dictionary<string, EntityReference> templateEntities)
        {
            entity.SetComponent(value);
        }

        public ComponentType[] Types => types;
    }

    public class ComponentValue<TParameter, TType> : IComponentBuilder<TParameter>
    {
        private static readonly ComponentType[] types = {ComponentType.Create<TType>()};

        private readonly TType value;

        public ComponentValue(TType value)
        {
            this.value = value;
        }

        public void Build(ref EntityReference entity, ref TParameter parameter,
            Dictionary<string, EntityReference> templateEntities)
        {
            entity.SetComponent(value);
        }

        public ComponentType[] Types => types;
    }
}