using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Automa.Entities.Systems
{
    public sealed class SystemTreeBuilder
    {
        private readonly List<ISystem> rawSystemList = new List<ISystem>();

        public void AddSystem(ISystem system)
        {
            rawSystemList.Add(system);
        }

        public void RemoveSystem(ISystem system)
        {
            rawSystemList.Remove(system);
        }

        public void AddSystems(IEnumerable<ISystem> systems)
        {
            rawSystemList.AddRange(systems);
        }

        public void Build(SystemManager systemManager)
        {
            var result = new List<SystemOrder>();
            var groups = new Dictionary<Type, SystemGroup>();
            foreach (var system in rawSystemList)
            {
                var systemType = system.GetType();
                var groupType = systemType.GetCustomAttribute<GroupAttribute>()?.Type;
                var order = systemType.GetCustomAttribute<OrderAttribute>()?.Order ?? SystemManager.DefaultOrder;
                if (groupType != null)
                {
                    // This is not root, add to group
                    if (!groups.TryGetValue(groupType, out var systemGroup))
                    {
                        if (!typeof(SystemGroup).IsAssignableFrom(groupType))
                            throw new ApplicationException(
                                "Group must be instance of type, inherited from SystemGroup");
                        systemGroup = (SystemGroup) (systemManager.systems
                                                         .Select(slot => slot.System)
                                                         .FirstOrDefault(s =>
                                                             groupType.IsInstanceOfType(s))
                                                     ?? Activator.CreateInstance(groupType));
                        groups.Add(groupType, systemGroup);
                        var groupOrder = groupType.GetCustomAttribute<OrderAttribute>()?.Order ??
                                         SystemManager.DefaultOrder;
                        result.Add(new SystemOrder(systemGroup, groupOrder));
                    }
                    systemGroup.AddSystem(system, order);
                }
                else
                {
                    result.Add(new SystemOrder(system, order));
                }
            }
            foreach (var systemOrder in result)
            {
                systemManager.AddSystem(systemOrder.System, systemOrder.Order);
            }
        }

        private struct SystemOrder
        {
            public readonly ISystem System;
            public readonly int Order;

            public SystemOrder(ISystem system, int order)
            {
                System = system;
                Order = order;
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class OrderAttribute : Attribute
        {
            public readonly int Order;

            public OrderAttribute(int order)
            {
                Order = order;
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class GroupAttribute : Attribute
        {
            public readonly Type Type;

            public GroupAttribute(Type type)
            {
                Type = type;
            }
        }
    }
}