using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Automa.Entities.Attributes;
using Automa.Entities.Internal;
using Automa.Entities.Systems.Debugging;

namespace Automa.Entities.Systems
{
    public class SystemManager : IManager
    {
        internal static int DefaultOrder = 0;

        internal readonly ArrayList<SystemSlot> systems = new ArrayList<SystemSlot>();
        
        #region Debugging
        private readonly bool debug;
        private readonly Stopwatch stopwatch;
        private SystemManagerDebugInfo debugInfo;
        public SystemManagerDebugInfo DebugInfo => debugInfo;
        #endregion

        public SystemManager() : this(false)
        {
        }

        public SystemManager(bool debug)
        {
            this.debug = debug;
            if (debug)
            {
                stopwatch = new Stopwatch();
            }
        }

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
            if (debug)
            {
                debugInfo = new SystemManagerDebugInfo(systems.Select(slot => slot.DebugInfo).ToArray());
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
            if (debug)
            {
                debugInfo = new SystemManagerDebugInfo(systems.Select(slot => slot.DebugInfo).ToArray());
            }
        }

        public void OnUpdate()
        {
            if (debug)
            {
                for (var i = 0; i < systems.Count; i++)
                {
                    var systemSlot = systems[i];
                    stopwatch.Restart();
                    systemSlot.System.OnUpdate();
                    stopwatch.Stop();
                    systemSlot.DebugInfo.UpdateTime = stopwatch.Elapsed;
                }
            }
            else
            {
                for (var i = 0; i < systems.Count; i++)
                {
                    systems[i].System.OnUpdate();
                }
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