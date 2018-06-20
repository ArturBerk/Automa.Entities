using System;
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

        internal ArrayList<SystemSlot> systems = new ArrayList<SystemSlot>(4);
        internal ArrayList<SystemSlot> updateSystems = new ArrayList<SystemSlot>(4);

        private IContext context;

        public SystemManager() : this(false)
        {
        }

        public SystemManager(bool debug)
        {
            this.debug = debug;
            if (debug)
            {
                stopwatch = new Stopwatch();
                if (debug)
                {
                    debugInfo = new SystemManagerDebugInfo();
                }
            }
        }

        public void OnUpdate()
        {
            if (debug)
            {
                for (var i = 0; i < updateSystems.Count; i++)
                {
                    var systemSlot = updateSystems[i];
                    stopwatch.Restart();
                    systemSlot.UpdateSystem.OnUpdate();
                    stopwatch.Stop();
                    systemSlot.DebugInfo.UpdateTime = stopwatch.Elapsed;
                }
            }
            else
            {
                for (var i = 0; i < updateSystems.Count; i++)
                {
                    updateSystems[i].UpdateSystem.OnUpdate();
                }
            }
        }

        public void OnAttachToContext(IContext context)
        {
            this.context = context;
            foreach (var systemSlot in systems)
            {
                systemSlot.System.OnAttachToContext(context);
            }
            if (debug)
            {
                foreach (var debugInfoSystem in DebugInfo.Systems)
                {
                    debugInfoSystem.OnAttachToContext(context);
                }
            }
        }

        public void OnDetachFromContext(IContext context)
        {
            this.context = null;
            foreach (var systemSlot in systems)
            {
                systemSlot.System.OnDetachFromContext(context);
            }
            if (debug)
            {
                foreach (var debugInfoSystem in DebugInfo.Systems)
                {
                    debugInfoSystem.OnDetachFromContext(context);
                }
            }
        }

        public void AddSystem(ISystem system)
        {
            var orderAttribute = system.GetType().GetCustomAttribute<OrderAttribute>();
            var newSlot = new SystemSlot(orderAttribute?.Order ?? DefaultOrder, system);
            AddSystemToGroup(ref systems, ref newSlot);
            if (context != null)
            {
                newSlot.System.OnAttachToContext(context);
            }

            if (newSlot.System.IsEnabled && newSlot.UpdateSystem != null)
            {
                AddSystemToGroup(ref updateSystems, ref newSlot);
            }
            system.EnabledChanged += SystemOnEnabledChanged;

            if (debug)
            {
                debugInfo = new SystemManagerDebugInfo(systems.Select(slot => slot.DebugInfo).ToArray());
                if (context != null)
                {
                    foreach (var debugInfoSystem in DebugInfo.Systems)
                    {
                        debugInfoSystem.OnAttachToContext(context);
                    }
                }
            }
        }

        private void SystemOnEnabledChanged(ISystem system, bool state)
        {
            if (state)
            {
                ref var slot = ref systems[FindSystemInGroup(ref systems, system)];
                if (slot.UpdateSystem != null)
                {
                    AddSystemToGroup(ref updateSystems, ref slot);
                }
            }
            else
            {
                ref var slot = ref systems[FindSystemInGroup(ref systems, system)];
                if (slot.DebugInfo != null) slot.DebugInfo.UpdateTime = TimeSpan.Zero;
                if (system is IUpdateSystem)
                {
                    RemoveSystemFromGroup(ref updateSystems, system);
                }
            }
        }

        private int FindSystemInGroup(ref ArrayList<SystemSlot> array, ISystem system)
        {
            for (var i = 0; i < array.Count; i++)
            {
                ref var slot = ref array.Buffer[i];
                if (ReferenceEquals(slot.System, system))
                {
                    return i;
                }
            }
            return -1;
        }

        private void AddSystemToGroup(ref ArrayList<SystemSlot> array, ref SystemSlot newSlot)
        {
            var inserted = false;
            for (var i = 0; i < array.Count; i++)
            {
                if (array[i].Order < newSlot.Order)
                {
                    array.Insert(i, newSlot);
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                array.Add(newSlot);
            }
        }

        public void RemoveSystem(ISystem system)
        {
            if (RemoveSystemFromGroup(ref systems, system))
            {
                if (context != null)
                {
                    system.OnDetachFromContext(context);
                }
                RemoveSystemFromGroup(ref updateSystems, system);
            }
            if (debug)
            {
                debugInfo = new SystemManagerDebugInfo(systems.Select(slot => slot.DebugInfo).ToArray());
                if (context != null)
                {
                    foreach (var debugInfoSystem in DebugInfo.Systems)
                    {
                        debugInfoSystem.OnDetachFromContext(context);
                    }
                }
            }
        }

        private bool RemoveSystemFromGroup(ref ArrayList<SystemSlot> array, ISystem system)
        {
            var index = FindSystemInGroup(ref array, system);
            if (index >= 0)
            {
                array.RemoveAt(index);
                return true;
            }
            return false;
        }

        #region Debugging

        private readonly bool debug;
        private readonly Stopwatch stopwatch;
        private SystemManagerDebugInfo debugInfo;
        public SystemManagerDebugInfo DebugInfo => debugInfo;

        #endregion
    }
}