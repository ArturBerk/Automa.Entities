using System;
using Automa.Entities.Internal;
using System.Reflection;
using Automa.Entities.Attributes;

namespace Automa.Entities
{
    public class Context
    {
        internal static int DefaultOrder = 0;

        private readonly ArrayList<BehaviourSlot> behaviours = new ArrayList<BehaviourSlot>();
        private readonly EntityManager entityManager = new EntityManager();

        public EntityManager EntityManager => entityManager;

        public void AddBehaviour(IBehaviour behaviour)
        {
            var orderAttribute = behaviour.GetType().GetCustomAttribute<OrderAttribute>();
            var newSlot = new BehaviourSlot(orderAttribute?.Order ?? Context.DefaultOrder, behaviour);
            var inserted = false;
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].Order < newSlot.Order)
                {
                    behaviours.Insert(i, newSlot);
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                behaviours.Add(newSlot);
            }
            newSlot.Behaviour.OnAddToContext(this);
        }

        public void RemoveBehaviour(IBehaviour behaviour)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                var slot = behaviours[i];
                if (ReferenceEquals(slot.Behaviour, behaviour))
                {
                    slot.Behaviour.OnRemoveFromContext(this);
                    behaviours.RemoveAt(i);
                    break;
                }
            }
        }

        public void Update()
        {
            //entityManager.Update();
            var rawArray = behaviours.ToArrayFast();
            for (var i = 0; i < behaviours.Count; i++)
            {
                rawArray[i].Behaviour.OnUpdate();
            }
        }
    }

}