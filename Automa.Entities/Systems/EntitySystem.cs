using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Automa.Entities.Systems
{
    public abstract class EntitySystem : ISystem
    {
        private bool isEnabled = true;
        public EntityManager EntityManager { get; set; }

        public event Action<ISystem, bool> EnabledChanged;

        public virtual void OnAttachToContext(IContext context)
        {
            EntityManager = context.GetManager<EntityManager>();
            RegisterGroups();
        }

        public virtual void OnDetachFromContext(IContext context)
        {
            UnregisterGroups();
            EntityManager = null;
        }

        public virtual bool IsEnabled
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
            foreach (var fieldInfo in GetInjectableFields()
                .Where(fi => typeof(Group).IsAssignableFrom(fi.FieldType)))
            {
                // Create group, register in entity manager
                var instance = (Group) Activator.CreateInstance(fieldInfo.FieldType);
                fieldInfo.SetValue(this, instance);
                EntityManager.RegisterGroup(instance);
            }
        }

        private void UnregisterGroups()
        {
            foreach (var fieldInfo in GetInjectableFields()
                .Where(fi => typeof(Group).IsAssignableFrom(fi.FieldType)))
            {
                EntityManager.UnregisterGroup((Group) fieldInfo.GetValue(this));
            }
        }

        private IEnumerable<FieldInfo> GetInjectableFields()
        {
            return GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}