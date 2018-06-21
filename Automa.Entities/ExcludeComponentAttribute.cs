using System;

namespace Automa.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ExcludeComponentAttribute : Attribute
    {
        public readonly Type ComponentType;

        public ExcludeComponentAttribute(Type componentType)
        {
            ComponentType = componentType;
        }
    }
}