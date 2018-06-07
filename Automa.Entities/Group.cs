using System;
using System.Collections.Generic;
using System.Reflection;
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
                var archetype = entityManagerChunk.Archetype;
                if (IsArchetypeMatching(ref archetype))
                {
                    foreach (var componentArray in componentArrays)
                    {
                        componentArray.AddArray(entityManagerChunk);
                    }
                }
            }
        }

        internal void OnArchetypeAdd(ArchetypeData data)
        {
            var archetype = data.Archetype;
            if (IsArchetypeMatching(ref archetype))
            {
                foreach (var componentArray in componentArrays)
                {
                    componentArray.AddArray(data);
                }
            }
        }

        internal void OnArchetypeRemoved(ArchetypeData data)
        {
            var archetype = data.Archetype;
            if (IsArchetypeMatching(ref archetype))
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

        internal bool IsArchetypeMatching(ref Archetype archetype)
        {
            var archetypeTypes = archetype.Types;
            var all = true;
            for (var i = 0; i < includedTypes.Length; i++)
            {
                var found = false;
                for (var j = 0; j < archetypeTypes.Length; j++)
                {
                    if (archetypeTypes[j] == includedTypes[i])
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

            for (var j = 0; j < archetypeTypes.Length; j++)
            {
                if (excludedTypes == null) continue;
                for (var i = 0; i < excludedTypes.Length; i++)
                {
                    if (archetypeTypes[j] == excludedTypes[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    public class GroupWithLength : Group
    {
        public int Length;

        public void UpdateLength()
        {
            if (componentArrays.Length == 0) Length = 0;
            Length = componentArrays[0].CalculatedCount;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExcludeComponentAttribute : Attribute
    {
        public readonly Type ComponentType;

        public ExcludeComponentAttribute(Type componentType)
        {
            ComponentType = componentType;
        }
    }
}