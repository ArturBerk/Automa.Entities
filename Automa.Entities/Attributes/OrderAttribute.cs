﻿using System;

namespace Automa.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OrderAttribute : Attribute
    {
        public readonly int Order;

        public OrderAttribute(int order)
        {
            Order = order;
        }
    }
}