using System.Reflection;
using Automa.Entities.Attributes;
using Automa.Entities.Internal;

namespace Automa.Entities.Behaviours
{
    public class BehaviourManager : IManager
    {
        internal static int DefaultOrder = 0;

        private readonly ArrayList<BehaviourSlot> behaviours = new ArrayList<BehaviourSlot>();

        public void AddBehaviour(IBehaviour behaviour)
        {
            var orderAttribute = behaviour.GetType().GetCustomAttribute<OrderAttribute>();
            var newSlot = new BehaviourSlot(orderAttribute?.Order ?? DefaultOrder, behaviour);
            var inserted = false;
            for (var i = 0; i < behaviours.Count; i++)
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
            if (context != null)
            {
                newSlot.Behaviour.OnAttachToContext(context);
            }
        }

        public void RemoveBehaviour(IBehaviour behaviour)
        {
            for (var i = 0; i < behaviours.Count; i++)
            {
                var slot = behaviours[i];
                if (ReferenceEquals(slot.Behaviour, behaviour))
                {
                    if (context != null)
                    {
                        slot.Behaviour.OnDetachFromContext(context);
                    }
                    behaviours.RemoveAt(i);
                    break;
                }
            }
        }

        public void OnUpdate()
        {
            //entityManager.Update();
            var rawArray = behaviours.ToArrayFast();
            for (var i = 0; i < behaviours.Count; i++)
            {
                rawArray[i].Behaviour.OnUpdate();
            }
        }

        private IContext context;

        public void OnAttachToContext(IContext context)
        {
            this.context = context;
            foreach (var behaviourSlot in behaviours)
            {
                behaviourSlot.Behaviour.OnAttachToContext(context);
            }
        }

        public void OnDetachFromContext(IContext context)
        {
            this.context = null;
            foreach (var behaviourSlot in behaviours)
            {
                behaviourSlot.Behaviour.OnDetachFromContext(context);
            }
        }
    }
}