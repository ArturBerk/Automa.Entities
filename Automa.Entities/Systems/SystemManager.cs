using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Automa.Common;
using Automa.Entities.Internal;
using Automa.Entities.Systems.Debugging;

namespace Automa.Entities.Systems
{
    public sealed class SystemManager : ManagerBase
    {
        public const int DefaultOrder = 0;

        internal ArrayList<SystemSlot> systems = new ArrayList<SystemSlot>(4);
        internal ArrayList<SystemSlot> updateSystems = new ArrayList<SystemSlot>(4);

        public SystemManager() : this(false)
        {
        }

        public SystemManager(bool debug)
        {
            this.debug = debug;
            if (debug)
            {
                if (debug)
                {
                    debugInfo = new SystemManagerDebugInfo();
                }
            }
        }

        public override void OnUpdate()
        {
            if (debug)
            {
                for (var i = 0; i < updateSystems.Count; i++)
                {
                    var systemSlot = updateSystems[i];
                    debugInfo.Stopwatch.Restart();
                    systemSlot.UpdateSystem.OnUpdate();
                    debugInfo.Stopwatch.Stop();
                    systemSlot.DebugInfo.UpdateTime = debugInfo.Stopwatch.Elapsed;
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

        public override void OnAttachToContext(IContext context)
        {
            base.OnAttachToContext(context);
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

        public override void OnDetachFromContext(IContext context)
        {
            base.OnDetachFromContext(context);
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

        public void AddSystem(ISystem system, int orderIndex = DefaultOrder)
        {
            var newSlot = new SystemSlot(orderIndex, system);
            AddSystemToGroup(ref systems, ref newSlot);
            if (Context != null)
            {
                newSlot.System.OnAttachToContext(Context);
            }

            if (newSlot.System.IsEnabled && newSlot.UpdateSystem != null)
            {
                AddSystemToGroup(ref updateSystems, ref newSlot);
            }
            system.EnabledChanged += SystemOnEnabledChanged;

            if (debug)
            {
                debugInfo = new SystemManagerDebugInfo(systems.Select(systemSlot => systemSlot.System is SystemGroup systemGroup
                    ? new SystemGroupDebugInfo(systemGroup)
                    : new SystemDebugInfo(systemSlot.System)).ToArray());
                if (Context == null) return;
                foreach (var debugInfoSystem in DebugInfo.Systems)
                {
                    debugInfoSystem.OnAttachToContext(Context);
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
                if (Context != null)
                {
                    system.OnDetachFromContext(Context);
                }
                RemoveSystemFromGroup(ref updateSystems, system);
            }
            if (debug)
            {
                debugInfo = new SystemManagerDebugInfo(systems.Select(systemSlot => systemSlot.System is SystemGroup systemGroup
                    ? new SystemGroupDebugInfo(systemGroup)
                    : new SystemDebugInfo(systemSlot.System)).ToArray());
                if (Context == null) return;
                foreach (var debugInfoSystem in DebugInfo.Systems)
                {
                    debugInfoSystem.OnDetachFromContext(Context);
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
        private SystemManagerDebugInfo debugInfo;
        public SystemManagerDebugInfo DebugInfo => debugInfo;

        #endregion
    }
}