using System;
using System.Collections.Generic;
using System.Reflection;
using Automa.Entities.Attributes;
using Automa.Entities.Collections;
using Automa.Entities.Debugging;
using Automa.Entities.Internal;

namespace Automa.Entities
{

    public class Group
    {
        internal ArrayList<int> componentArrayLengths = new ArrayList<int>();
        internal CollectionBase[] componentCollections;
        internal ComponentType[] excludedTypes;
        internal ComponentType[] includedTypes;

        public EntityManager EntityManager { get; private set; }
        public int Count;
        
        public Group()
        {
            var includedTypesTmp = new List<ComponentType>();
            var excludedTypesTmp = new List<ComponentType>();
            var componentArraysTmp = new List<CollectionBase>();
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsGenericType)
                {
                    var genericType = fieldType.GetGenericTypeDefinition();
                    if (genericType == typeof(ComponentCollection<>))
                    {
                        ComponentType componentType = fieldType.GetGenericArguments()[0];
                        includedTypesTmp.Add(componentType);
                        var componentsArray = (CollectionBase)Activator.CreateInstance(fieldType);
                        componentArraysTmp.Add(componentsArray);
                        fieldInfo.SetValue(this, componentsArray);
                    }
                }
                else if (fieldType == typeof(Collections.EntityCollection))
                {
                    var entitiesArray = new Collections.EntityCollection();
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
            componentCollections = componentArraysTmp.ToArray();
        }

        public void Update()
        {
            componentArrayLengths.FastClear();
            if (componentCollections.Length == 0)
            {
                Count = 0;
                return;
            }
            Count = componentCollections[0].CalculatedCount;
            componentCollections[0].GetArrayLengths(componentArrayLengths, out Count);
        }

        public Iterator GetIterator()
        {
            return new Iterator(this);
        }

        public Iterator GetIterator(int startIndex, int endIndex)
        {
            return new Iterator(this, startIndex, endIndex);
        }

        internal void Register(EntityManager entityManager)
        {
            EntityManager = entityManager;
            foreach (var entityManagerChunk in entityManager.Datas)
            {
                var entityType = entityManagerChunk.EntityType;
                if (IsEntityTypeMatching(ref entityType))
                {
                    foreach (var componentArray in componentCollections)
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
                foreach (var componentArray in componentCollections)
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
                foreach (var componentArray in componentCollections)
                {
                    componentArray.RemoveArray(data);
                }
            }
        }

        internal void Unregister(EntityManager entityManager1)
        {
            EntityManager = null;
            componentCollections = null;
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsGenericType)
                {
                    var genericType = fieldType.GetGenericTypeDefinition();
                    if (genericType == typeof(ComponentCollection<>))
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

        public struct Iterator
        {
            public readonly Group Group;
            public EntityIndex Index;
            private int currentIndexRaw;
            public int StartIndex;
            public int EndIndex;
            public bool IsCompleted;

            public Iterator(Group @group) : this(@group, 0, group.Count)
            {
            }

            public Iterator(Group @group, int startIndex, int endIndex) : this()
            {
                Group = @group;
                Index = new EntityIndex(0, 0);
                StartIndex = startIndex;
                EndIndex = endIndex;
                IsCompleted = StartIndex >= group.Count;
                currentIndexRaw = StartIndex - 1;

                // Find start Index
                Index = new EntityIndex(0, -1);
                var currentIndex = 0;
                for (int i = 0; i < group.componentArrayLengths.Count; i++)
                {
                    var currentArrayLength = Group.componentArrayLengths.Buffer[i];
                    var indexInArray = StartIndex - currentIndex;
                    if (indexInArray < currentArrayLength)
                    {
                        Index = new EntityIndex(i, indexInArray - 1);
                        break;
                    }
                    currentIndex += currentArrayLength;
                }
            }

            public bool MoveNext()
            {
                if (IsCompleted) return false;
                ++currentIndexRaw;
                if (currentIndexRaw >= EndIndex)
                {
                    IsCompleted = true;
                    return false;
                }
                ++Index.Index;
                var currentArrayLength = Group.componentArrayLengths.Buffer[Index.ArrayIndex];
                if (Index.Index < currentArrayLength) return true;
                Index.Index = 0;
                // move to next array
                while (true)
                {
                    ++Index.ArrayIndex;
                    if (Index.ArrayIndex >= Group.componentArrayLengths.Count)
                    {
                        IsCompleted = true;
                        return false;
                    }
                    currentArrayLength = Group.componentArrayLengths.Buffer[Index.ArrayIndex];
                    if (Index.Index < currentArrayLength) return true;
                }
            }
        }

        public struct EntityIndex
        {
            internal int ArrayIndex;
            internal int Index;

            internal EntityIndex(int arrayIndex, int index)
            {
                this.ArrayIndex = arrayIndex;
                this.Index = index;
            }
        }
    }

}