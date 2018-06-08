using System;
using System.Collections.Generic;

namespace Automa.Entities
{
    public class Context : IContext
    {
        private readonly Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

        public T GetManager<T>() where T : IManager
        {
            if (!managers.TryGetValue(typeof(T), out var manager))
            {
                throw new ApplicationException($"Manager of type {typeof(T)} not found");
            }
            return (T) manager;
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
            foreach (var manager in managers)
            {
                manager.Value.OnUpdate();
            }
        }
    }

    public interface IContext
    {
        T GetManager<T>() where T : IManager;
        T SetManager<T>(T manager) where T : IManager;
        void RemoveManager<T>() where T : IManager;
        void Update();
    }

    public interface IManager
    {
        void OnAttachToContext(IContext context);
        void OnDetachFromContext(IContext context);
        void OnUpdate();
    }

}