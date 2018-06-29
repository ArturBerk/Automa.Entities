using System;
using System.Collections.Generic;
using System.Linq;
using Automa.Entities.Internal;
using Automa.Entities.Systems.Debugging;

namespace Automa.Entities.Systems
{
    public abstract class SystemGroup : ISystem, IUpdateSystem
    {
        private ArrayList<SystemSlot> systems = new ArrayList<SystemSlot>(4);

        public IEnumerable<ISystem> Systems => systems.Select(slot => slot.System);

        private IContext context;
        private bool isEnabled = true;

        internal SystemGroupDebugInfo debug;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;
                EnabledChanged?.Invoke(this, isEnabled);
            }
        }

        public event Action<ISystem, bool> EnabledChanged;

        public void OnAttachToContext(IContext context)
        {
            this.context = context;
            foreach (var system in systems)
            {
                system.System.OnAttachToContext(context);
            }
            debug?.OnAttachToContext(context);
        }

        public void OnDetachFromContext(IContext context)
        {
            debug?.OnDetachFromContext(context);
            foreach (var system in systems)
            {
                system.System.OnDetachFromContext(context);
            }
            this.context = null;
        }

        public void OnUpdate()
        {
            if (debug == null)
            {
                for (var i = 0; i < systems.Count; i++)
                {
                    ref var systemSlot = ref systems[i];
                    if (systemSlot.UpdateSystem != null && systemSlot.System.IsEnabled)
                    {
                        systemSlot.UpdateSystem.OnUpdate();
                    }
                }
            }
            else
            {
                var groupTime = new TimeSpan();
                for (var i = 0; i < debug.Systems.Length; i++)
                {
                    ref var systemSlot = ref debug.Systems[i];
                    if (systemSlot.System is IUpdateSystem updateSystem && systemSlot.System.IsEnabled)
                    {
                        debug.Stopwatch.Restart();
                        updateSystem.OnUpdate();
                        debug.Stopwatch.Stop();
                        systemSlot.UpdateTime = debug.Stopwatch.Elapsed;
                        groupTime = groupTime.Add(debug.Stopwatch.Elapsed);
                    }
                    debug.UpdateTime = groupTime;
                }
            }
        }

        public void AddSystem(ISystem system, int orderIndex = SystemManager.DefaultOrder)
        {
            var newSlot = new SystemSlot(orderIndex, system);
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
                debug?.OnAttachToContext(context);
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
            if (context != null)
            {
                debug?.OnDetachFromContext(context);
            }
        }
    }
}