﻿using System;

namespace Automa.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExcludeComponentAttribute : Attribute
    {
        public readonly Type ComponentType;

        public ExcludeComponentAttribute(Type componentType)
        {
            ComponentType = componentType;
        }
    }
}