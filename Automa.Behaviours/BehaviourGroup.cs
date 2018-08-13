using System.Collections.Generic;
using Automa.Common;

namespace Automa.Behaviours
{
    public interface IBehaviourGroup
    {
        string Name { get; }
        IBehaviourLink Add(IBehaviour slot);
        IBehaviourGroup GetGroup(string name);
        void Remove(IBehaviour slot);
        void Apply();
    }

    public class BehaviourGroup : IBehaviourGroup
    {
        private readonly Dictionary<string, IBehaviourGroup> groups = new Dictionary<string, IBehaviourGroup>();
        private readonly World world;
        private ArrayList<IBehaviourSlot> behaviourList = new ArrayList<IBehaviourSlot>(4);

        internal BehaviourGroup(World world, string name)
        {
            this.world = world;
            Name = name;
        }

        public string Name { get; }

        public IBehaviourLink Add(IBehaviour behaviour)
        {
            var behaviourSlot =
                new BehaviourSlot(this, behaviour);
            behaviourList.Add(behaviourSlot);
            behaviour.OnAttach(world);
            return behaviourSlot;
        }

        public IBehaviourGroup GetGroup(string name)
        {
            if (!groups.TryGetValue(name, out var group))
            {
                group = new BehaviourGroup(world, name);
                groups.Add(name, group);
            }
            return group;
        }

        public void Remove(IBehaviour slot)
        {
            for (var i = 0; i < behaviourList.Count; i++)
            {
                var behaviourSlot = behaviourList[i];
                if (!ReferenceEquals(behaviourSlot.Behaviour, slot)) continue;
                behaviourList.UnorderedRemoveAt(i);
                return;
            }
        }

        public void Apply()
        {
            for (var i = 0; i < behaviourList.Count; i++)
            {
                behaviourList[i].Apply();
            }
        }

        internal void Remove(IBehaviourSlot slot)
        {
            var index = behaviourList.IndexOf(slot);
            if (index <= 0) return;
            behaviourList.UnorderedRemoveAt(index);
        }

        internal interface IBehaviourSlot : IBehaviourLink
        {
            object Behaviour { get; }
            void Apply();
        }

        internal class BehaviourSlot : IBehaviourSlot
        {
            private readonly IBehaviour behaviour;
            private readonly BehaviourGroup group;

            public BehaviourSlot(BehaviourGroup group, IBehaviour behaviour)
            {
                this.group = group;
                this.behaviour = behaviour;
            }

            public object Behaviour => behaviour;

            public void Apply()
            {
                behaviour.Apply();
            }

            public void Dispose()
            {
                group.Remove(behaviour);
            }
        }
    }
}