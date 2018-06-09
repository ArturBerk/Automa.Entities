using System;
using System.Collections.Generic;
using System.Reflection;
using Automa.Entities.Attributes;
using Automa.Entities.Collections;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public class Group
    {
        internal CollectionBase[] componentArrays;
        internal ComponentType[] excludedTypes;
        internal ComponentType[] includedTypes;

        public EntityManager EntityManager { get; private set; }
        public int Count;

        public void UpdateLength()
        {
            if (componentArrays.Length == 0) Count = 0;
            Count = componentArrays[0].CalculatedCount;
        }

        internal void Register(EntityManager entityManager)
        {
            EntityManager = entityManager;
            var includedTypesTmp = new List<ComponentType>();
            var excludedTypesTmp = new List<ComponentType>();
            var componentArraysTmp = new List<CollectionBase>();
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsGenericType)
                {
                    var genericType = fieldType.GetGenericTypeDefinition();
                    if (genericType == typeof(Collection<>))
                    {
                        ComponentType componentType = fieldType.GetGenericArguments()[0];
                        includedTypesTmp.Add(componentType);
                        var componentsArray = (CollectionBase) Activator.CreateInstance(fieldType);
                        componentArraysTmp.Add(componentsArray);
                        fieldInfo.SetValue(this, componentsArray);
                    }
                }
                else if (fieldType == typeof(Collections.Entities))
                {
                    var entitiesArray = new Collections.Entities();
                    componentArraysTmp.Add(entitiesArray);
                    fieldInfo.SetValue(this, entitiesArray);
                }
            }
            foreach (var excludeComponentAttribute in GetType().GetCustomAttributes<ExcludeComponentAttribute>())
            {
                ComponentType componentType = excludeComponentAttribute.ComponentType;
                excludedTypesTmp.Add(componentType);
            }

            includedTypes = includedTypesTmp.ToArray();
            excludedTypes = excludedTypesTmp.Count == 0
                ? null
                : excludedTypesTmp.ToArray();
            componentArrays = componentArraysTmp.ToArray();

            foreach (var entityManagerChunk in entityManager.Datas)
            {
                var entityType = entityManagerChunk.EntityType;
                if (IsEntityTypeMatching(ref entityType))
                {
                    foreach (var componentArray in componentArrays)
                    {
                        componentArray.AddArray(entityManagerChunk);
                    }
                }
            }
        }

        internal void OnEntityTypeAdd(EntityTypeData data)
        {
            var entityType = data.EntityType;
            if (IsEntityTypeMatching(ref entityType))
            {
                foreach (var componentArray in componentArrays)
                {
                    componentArray.AddArray(data);
                }
            }
        }

        internal void OnEntityTypeRemoved(EntityTypeData data)
        {
            var entityType = data.EntityType;
            if (IsEntityTypeMatching(ref entityType))
            {
                foreach (var componentArray in componentArrays)
                {
                    componentArray.RemoveArray(data);
                }
            }
        }

        internal void Unregister(EntityManager entityManager1)
        {
            EntityManager = null;
            componentArrays = null;
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsGenericType)
                {
                    var genericType = fieldType.GetGenericTypeDefinition();
                    if (genericType == typeof(Collection<>))
                    {
                        fieldInfo.SetValue(this, null);
                    }
                }
            }
        }

        internal bool IsEntityTypeMatching(ref EntityType entityType)
        {
            var entityComponentTypes = entityType.Types;
            var all = true;
            for (var i = 0; i < includedTypes.Length; i++)
            {
                var found = false;
                for (var j = 0; j < entityComponentTypes.Length; j++)
                {
                    if (entityComponentTypes[j] == includedTypes[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    all = false;
                    break;
                }
            }
            if (!all) return false;

            for (var j = 0; j < entityComponentTypes.Length; j++)
            {
                if (excludedTypes == null) continue;
                for (var i = 0; i < excludedTypes.Length; i++)
                {
                    if (entityComponentTypes[j] == excludedTypes[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

}