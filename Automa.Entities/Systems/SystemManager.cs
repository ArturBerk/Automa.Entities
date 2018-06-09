using System.Reflection;
using Automa.Entities.Attributes;
using Automa.Entities.Internal;

namespace Automa.Entities.Systems
{
    public class SystemManager : IManager
    {
        internal static int DefaultOrder = 0;

        private readonly ArrayList<SystemSlot> systems = new ArrayList<SystemSlot>();

        public void AddSystem(ISystem system)
        {
            var orderAttribute = system.GetType().GetCustomAttribute<OrderAttribute>();
            var newSlot = new SystemSlot(orderAttribute?.Order ?? DefaultOrder, system);
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

        public void OnUpdate()
        {
            for (var i = 0; i < systems.Count; i++)
            {
                systems[i].System.OnUpdate();
            }
        }

        private IContext context;

        public void OnAttachToContext(IContext context)
        {
            this.context = context;
            foreach (var systemSlot in systems)
            {
                systemSlot.System.OnAttachToContext(context);
            }
        }

        public void OnDetachFromContext(IContext context)
        {
            this.context = null;
            foreach (var systemSlot in systems)
            {
                systemSlot.System.OnDetachFromContext(context);
            }
        }
    }
}