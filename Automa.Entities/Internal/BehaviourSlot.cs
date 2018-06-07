using System;
using System.Collections.Generic;

namespace Automa.Entities.Internal
{
    internal struct BehaviourSlot : IComparable<BehaviourSlot>
    {
        public static readonly IComparer<BehaviourSlot> DefaultComparer = new Comparer();
        public readonly int Order;
        public readonly IBehaviour Behaviour;

        public BehaviourSlot(int order, IBehaviour behaviour)
        {
            Order = order;
            Behaviour = behaviour;
        }

        public int CompareTo(BehaviourSlot other)
        {
            return Order.CompareTo(other.Order);
        }

        public class Comparer : IComparer<BehaviourSlot>
        {
            public int Compare(BehaviourSlot x, BehaviourSlot y)
            {
                return x.Order.CompareTo(y.Order);
            }
        }
    }
}