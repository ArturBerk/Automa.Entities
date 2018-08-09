using System;
using System.Collections.Generic;
using Automa.Common;

namespace Automa.Behaviours
{
    public interface IBehaviourGroup
    {
        string Name { get; }
        IBehaviourLink Add<T>(IBehaviour<T> slot);
        IBehaviourGroup GetGroup(string name);
        void Remove<T>(IBehaviour<T> slot);
        void Apply();
    }

    public class BehaviourGroup : IBehaviourGroup
    {
        private readonly EntityGroup entities;
        private ArrayList<IBehaviourSlot> behaviourList = new ArrayList<IBehaviourSlot>(4);
        private readonly Dictionary<string, IBehaviourGroup> groups = new Dictionary<string, IBehaviourGroup>();

        internal BehaviourGroup(EntityGroup entities, string name)
        {
            this.entities = entities;
            Name = name;
        }

        public string Name { get; }

        public IBehaviourLink Add<T>(IBehaviour<T> behaviour)
        {
            var behaviourSlot = new BehaviourSlot<T>(this, behaviour, (EntityList<T>) entities.GetEntityList<T>());
            behaviourList.Add(behaviourSlot);
            return behaviourSlot;
        }

        public IBehaviourGroup GetGroup(string name)
        {
            if (!groups.TryGetValue(name, out var group))
            {
                group = new BehaviourGroup(entities, name);
                groups.Add(name, group);
            }
            return group;
        }

        public void Remove<T>(IBehaviour<T> slot)
        {
            for (int i = 0; i < behaviourList.Count; i++)
            {
                var behaviourSlot = behaviourList[i];
                if (!ReferenceEquals(behaviourSlot.Behaviour, slot)) continue;
                behaviourSlot.InternalDispose();
                behaviourList.UnorderedRemoveAt(i);
                return;
            }
        }

        internal void Remove(IBehaviourSlot slot)
        {
            var index = behaviourList.IndexOf(slot);
            if (index <= 0) return;
            behaviourList.UnorderedRemoveAt(index);
        }

        public void Apply()
        {
            for (var i = 0; i < behaviourList.Count; i++)
            {
                behaviourList[i].Apply();
            }
        }

        internal interface IBehaviourSlot : IBehaviourLink
        {
            object Behaviour { get; }
            void Apply();
            void InternalDispose();
        }

        internal class BehaviourSlot<T> : IBehaviourSlot
        {
            private readonly IBehaviour<T> behaviour;
            private readonly EntityList<T> entities;
            private readonly BehaviourGroup @group;

            public BehaviourSlot(BehaviourGroup @group, IBehaviour<T> behaviour, EntityList<T> entities)
            {
                this.@group = @group;
                this.behaviour = behaviour;
                this.entities = entities;

                if (behaviour is IEntityAddedHandler<T> addedHandler)
                {
                    entities.AddHandler(addedHandler);
                }
                if (behaviour is IEntityRemovedHandler<T> removedHandler)
                {
                    entities.AddHandler(removedHandler);
                }
            }

            public object Behaviour => behaviour;

            public void Apply()
            {
                behaviour.Apply(new EntityCollection<T>(entities));
            }

            public void InternalDispose()
            {
                if (behaviour is IEntityAddedHandler<T> addedHandler)
                {
                    entities.RemoveHandler(addedHandler);
                }
                if (behaviour is IEntityRemovedHandler<T> removedHandler)
                {
                    entities.RemoveHandler(removedHandler);
                }
            }

            public void Dispose()
            {
                InternalDispose();
                @group.Remove(behaviour);
            }
        }
    }
}