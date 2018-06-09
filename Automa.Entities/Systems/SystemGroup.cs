using System.Reflection;
using Automa.Entities.Attributes;
using Automa.Entities.Systems;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public sealed class SystemGroup : ISystem
    {
        private readonly ArrayList<SystemSlot> systems = new ArrayList<SystemSlot>();

        private IContext context;

        public bool IsEnabled { get; set; }

        public void OnAttachToContext(IContext context)
        {
            this.context = context;
            foreach (var system in systems)
            {
                system.System.OnAttachToContext(context);
            }
        }

        public void OnDetachFromContext(IContext context)
        {
            foreach (var system in systems)
            {
                system.System.OnDetachFromContext(context);
            }
            this.context = null;
        }

        public void OnUpdate()
        {
            for (var i = 0; i < systems.Count; i++)
            {
                systems[i].System.OnUpdate();
            }
        }

        public void AddSystem(ISystem system)
        {
            var orderAttribute = system.GetType().GetCustomAttribute<OrderAttribute>();
            var newSlot = new SystemSlot(orderAttribute?.Order ?? SystemManager.DefaultOrder, system);
            var inserted = false;
            for (var i = 0; i < systems.Count; i++)
            {
                if (systems[i].Order < newSlot.Order)
                {
                    systems.Insert(i, newSlot);
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                systems.Add(newSlot);
            }
            if (context != null)
            {
                newSlot.System.OnAttachToContext(context);
            }
        }

        public void RemoveSystem(ISystem system)
        {
            for (var i = 0; i < systems.Count; i++)
            {
                var slot = systems[i];
                if (ReferenceEquals(slot.System, system))
                {
                    if (context != null)
                    {
                        slot.System.OnDetachFromContext(context);
                    }
                    systems.RemoveAt(i);
                    break;
                }
            }
        }
    }
}