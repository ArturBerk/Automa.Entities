using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automa.Entities.Events;
using Automa.Entities.Systems.Internal;

namespace Automa.Entities.Systems
{
    public abstract class EntitySystem : ISystem
    {
        internal Group[] groups;
        private bool isEnabled = true;
        public EntityManager EntityManager;
        public EventManager EventManager;

        public event Action<ISystem, bool> EnabledChanged;

        public virtual void OnAttachToContext(IContext context)
        {
            EntityManager = context.GetManager<EntityManager>();
            EventManager = context.GetManager<EventManager>();
            RegisterGroups();
            RegisterEvents();
            InjectManagers(context);
        }

        private void InjectManagers(IContext context)
        {
            foreach (var injectableField in GetInjectableFields()
                .Where(info => typeof(IManager).IsAssignableFrom(info.FieldType)))
            {
                if (context.HasManager(injectableField.FieldType))
                {
                    injectableField.SetValue(this, context.GetManager(injectableField.FieldType));
                }
            }
        }

        public virtual void OnDetachFromContext(IContext context)
        {
            UnregisterGroups();
            UnregisterEvents();
            EntityManager = null;
            EventManager = null;
        }

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

        private void RegisterGroups()
        {
            List<Group> groups = new List<Group>();
            foreach (var fieldInfo in GetInjectableFields()
                .Where(fi => typeof(Group).IsAssignableFrom(fi.FieldType)))
            {
                // Create group, register in entity manager
                var instance = (Group)fieldInfo.GetValue(this);
                if (instance == null)
                {
                    instance = (Group) Activator.CreateInstance(fieldInfo.FieldType);
                    fieldInfo.SetValue(this, instance);
                }
                EntityManager.RegisterGroup(instance);
                groups.Add(instance);
            }
            this.groups = groups.ToArray();
        }

        private void UnregisterGroups()
        {
            foreach (var fieldInfo in GetInjectableFields()
                .Where(fi => typeof(Group).IsAssignableFrom(fi.FieldType)))
            {
                EntityManager.UnregisterGroup((Group)fieldInfo.GetValue(this));
            }
        }

        private void RegisterEvents()
        {
            if (EventManager == null) return;
            foreach (var @interface in GetType().GetInterfaces())
            {
                if (@interface.IsGenericType && typeof(IEventListener<>) == @interface.GetGenericTypeDefinition())
                {
                    GetRegistrator(@interface.GetGenericArguments()[0]).Register(EventManager, this);
                }
            }
        }


        private void UnregisterEvents()
        {
            if (EventManager == null) return;
            foreach (var @interface in GetType().GetInterfaces())
            {
                if (@interface.IsGenericType && typeof(IEventListener<>) == @interface.GetGenericTypeDefinition())
                {
                    GetRegistrator(@interface.GetGenericArguments()[0]).Unregister(EventManager, this);
                }
            }
        }

        protected IEnumerable<FieldInfo> GetInjectableFields()
        {
            return GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(info => info.GetCustomAttribute<InjectAttribute>() != null);
        }

        private static IEventListenerRegistrator GetRegistrator(Type eventType)
        {
            return (IEventListenerRegistrator)Activator.CreateInstance(
                typeof(EventListenerRegistrator<>).MakeGenericType(eventType));
        }
    }
}