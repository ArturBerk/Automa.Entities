﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Automa.Entities
{
    public class Context : IDebuggableContext
    {
        private readonly Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

        public Context() : this(false)
        {
        }

        public Context(bool debug)
        {
            this.debug = debug;
            if (debug)
            {
                stopwatch = new Stopwatch();
                debugInfo = new ContextDebugInfo();
            }
        }

        public bool HasManager<T>() where T : IManager
        {
            return managers.ContainsKey(typeof(T));
        }

        public bool HasManager(Type type)
        {
            return managers.ContainsKey(type);
        }

        public T GetManager<T>() where T : IManager
        {
            if (!managers.TryGetValue(typeof(T), out var manager))
            {
                throw new ApplicationException($"Manager of type {typeof(T)} not found");
            }
            return (T) manager;
        }

        public IManager GetManager(Type type)
        {
            if (!managers.TryGetValue(type, out var manager))
            {
                throw new ApplicationException($"Manager of type {type} not found");
            }
            return manager;
        }

        public T SetManager<T>(T manager) where T : IManager
        {
            if (managers.ContainsKey(typeof(T)))
            {
                managers[typeof(T)].OnDetachFromContext(this);
            }
            managers[typeof(T)] = manager;
            manager.OnAttachToContext(this);
            return manager;
        }

        public void RemoveManager<T>() where T : IManager
        {
            if (managers.TryGetValue(typeof(T), out var manager))
            {
                managers.Remove(typeof(T));
                manager.OnDetachFromContext(this);
            }
        }

        public void Update()
        {
            if (debug)
            {
                stopwatch.Restart();
                foreach (var manager in managers)
                {
                    manager.Value.OnUpdate();
                }
                stopwatch.Stop();
                debugInfo.UpdateTime = stopwatch.Elapsed;
            }
            else
            {
                foreach (var manager in managers)
                {
                    manager.Value.OnUpdate();
                }
            }
        }

        public void Dispose()
        {
            foreach (var manager in managers.Values)
            {
                manager.OnDetachFromContext(this);
            }
            managers.Clear();
        }

        #region Debugging

        private readonly bool debug;
        private readonly Stopwatch stopwatch;
        private readonly ContextDebugInfo debugInfo;
        public ContextDebugInfo DebugInfo => debugInfo;

        #endregion
    }

    public class ContextDebugInfo
    {
        public TimeSpan UpdateTime;
    }

    public interface IContext : IDisposable
    {
        bool HasManager<T>() where T : IManager;
        bool HasManager(Type type);
        T GetManager<T>() where T : IManager;
        IManager GetManager(Type type);
        T SetManager<T>(T manager) where T : IManager;
        void RemoveManager<T>() where T : IManager;
        void Update();
    }

    public interface IDebuggableContext : IContext
    {
        ContextDebugInfo DebugInfo { get; }

    }
}