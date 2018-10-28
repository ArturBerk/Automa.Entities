using System;
using System.Collections.Generic;
using System.Reflection;
using Automa.Common;
using Automa.Entities.Collections;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public class Group
    {
        internal ArrayList<int> componentArrayLengths = new ArrayList<int>(4);
        internal ArrayList<EntityDataLink> entityTypeDatas = new ArrayList<EntityDataLink>(4);
        internal CollectionBase[] componentCollections;
        internal ComponentType[] excludedTypes;
        internal ComponentType[] includedTypes;

        public EntityManager EntityManager { get; private set; }
        public int Count;

        internal class EntityDataLink : Internal.IEntityAddedListener, Internal.IEntityRemovingListener
        {
            private Group group;
            internal IEntityAddedListener addedListener;
            internal IEntityRemovingListener removingListener;
            public readonly int Index;
            public readonly EntityTypeData Data;
            // For delayed listener call
            public ArrayList<Entity> AddedEntities;

            public EntityDataLink(Group @group, int index, EntityTypeData data)
            {
                Index = index;
                Data = data;
                this.@group = @group;
                AddedEntities = new ArrayList<Entity>(4);
            }

            public void OnEntityAdded(Entity entity, EntityTypeData movedFrom)
            {
                if (movedFrom != null)
                {
                    for (int i = 0; i < @group.entityTypeDatas.Count; i++)
                    {
                        var entityData = @group.entityTypeDatas[i];
                        if (entityData.Data == movedFrom)
                        {
                            return;
                        }
                    }
                }
                AddedEntities.Add(entity);
            }

            public void OnEntityRemoving(Entity entity, EntityTypeData movingTo)
            {
                if (movingTo != null)
                {
                    for (int i = 0; i < @group.entityTypeDatas.Count; i++)
                    {
                        var entityData = @group.entityTypeDatas[i];
                        if (entityData.Data == movingTo)
                        {
                            return;
                        }
                    }
                }
                removingListener.OnEntityRemoving(entity);
            }
        }
        
        protected Group()
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
                        ComponentType componentType = ComponentType.Create(fieldType.GetGenericArguments()[0]);
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
                ComponentType componentType = ComponentType.Create(excludeComponentAttribute.ComponentType);
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
            componentCollections[0].GetArrayLengths(ref componentArrayLengths, out Count);
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
            foreach (var entityTypeData in entityManager.Datas)
            {
                OnEntityTypeAdd(entityTypeData);
            }
        }

        internal void OnEntityTypeAdd(EntityTypeData data)
        {
            var entityType = data.EntityType;
            if (IsEntityTypeMatching(ref entityType))
            {
                EntityDataLink link = new EntityDataLink(this, entityTypeDatas.Count, data);
                if (this is IEntityAddedListener addedListener)
                {
                    link.addedListener = addedListener;
                    data.addedListeners.Add(link);
                }
                if (this is IEntityRemovingListener removingListener)
                {
                    link.removingListener = removingListener;
                    data.removingListeners.Add(link);
                }
                // Call added listener for all existing entities
                for (int i = 0; i < data.count; i++)
                {
                    link.OnEntityAdded(data.entityArray[i], null);
                }
                entityTypeDatas.Add(link);
                foreach (var componentArray in componentCollections)
                {
                    componentArray.AddArray(data);
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
            foreach (var entityTypeData in entityTypeDatas)
            {
                if (entityTypeData.addedListener != null)
                {
                    entityTypeData.Data.addedListeners.Remove(entityTypeData);
                }
                if (entityTypeData.removingListener != null)
                {
                    entityTypeData.Data.removingListeners.Remove(entityTypeData);
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

        public void HandleModifications()
        {
            for (int i = 0; i < entityTypeDatas.Count; i++)
            {
                ref var entityDataLink = ref entityTypeDatas[i];
                if (entityDataLink.addedListener != null)
                {
                    for (int j = 0; j < entityDataLink.AddedEntities.Count; j++)
                    {
                        entityDataLink.addedListener.OnEntityAdded(entityDataLink.AddedEntities[j]);
                    }
                    entityDataLink.AddedEntities.FastClear();
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

    public interface IEntityAddedListener
    {
        void OnEntityAdded(Entity index);
    }

    public interface IEntityRemovingListener
    {
        void OnEntityRemoving(Entity index);
    }

}