using System;
using System.Collections.Generic;

namespace Automa.Entities.Builders
{
    public class ComponentAction<TType> : IComponentBuilder
    {
        private static readonly ComponentType[] types = { ComponentType.Create<TType>() };

        private readonly Func<Dictionary<string, EntityReference>, TType> value;

        public ComponentAction(Func<Dictionary<string, EntityReference>, TType> value)
        {
            this.value = value;
        }

        public void Build(ref EntityReference entity,
            Dictionary<string, EntityReference> templateEntities)
        {
            entity.SetComponent(value(templateEntities));
        }

        public ComponentType[] Types => types;
    }

    public class ComponentAction<TParameter, TType> : IComponentBuilder<TParameter>
    {
        private static readonly ComponentType[] types = {ComponentType.Create<TType>()};

        private readonly Func<TParameter, Dictionary<string, EntityReference>, TType> value;

        public ComponentAction(Func<TParameter, Dictionary<string, EntityReference>, TType> value)
        {
            this.value = value;
        }

        public void Build(ref EntityReference entity, ref TParameter parameter,
            Dictionary<string, EntityReference> templateEntities)
        {
            entity.SetComponent(value(parameter, templateEntities));
        }

        public ComponentType[] Types => types;
    }
}