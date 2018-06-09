using System;
using System.Collections.Generic;
using Automa.Entities.Systems;
using Automa.Entities.Systems.Debugging;

namespace Automa.Entities.Internal
{
    internal struct SystemSlot : IComparable<SystemSlot>
    {
        public static readonly IComparer<SystemSlot> DefaultComparer = new Comparer();
        public readonly int Order;
        public readonly ISystem System;
        public readonly SystemDebugInfo DebugInfo;

        public SystemSlot(int order, ISystem system)
        {
            Order = order;
            System = system;
            DebugInfo = new SystemDebugInfo(system);
        }

        public int CompareTo(SystemSlot other)
        {
            return Order.CompareTo(other.Order);
        }

        public class Comparer : IComparer<SystemSlot>
        {
            public int Compare(SystemSlot x, SystemSlot y)
            {
                return x.Order.CompareTo(y.Order);
            }
        }
    }
}