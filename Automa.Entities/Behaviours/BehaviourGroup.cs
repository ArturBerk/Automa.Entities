﻿using System.Reflection;
using Automa.Entities.Attributes;
using Automa.Entities.Behaviours;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public sealed class BehaviourGroup : IBehaviour
    {
        private readonly ArrayList<BehaviourSlot> behaviours = new ArrayList<BehaviourSlot>();

        private IContext context;

        public bool IsEnabled { get; set; }

        public void OnAttachToContext(IContext context)
        {
            this.context = context;
            foreach (var behaviour in behaviours)
            {
                behaviour.Behaviour.OnAttachToContext(context);
            }
        }

        public void OnDetachFromContext(IContext context)
        {
            foreach (var behaviour in behaviours)
            {
                behaviour.Behaviour.OnDetachFromContext(context);
            }
            this.context = null;
        }

        public void OnUpdate()
        {
            for (var i = 0; i < behaviours.Count; i++)
            {
                behaviours[i].Behaviour.OnUpdate();
            }
        }

        public void AddBehaviour(IBehaviour behaviour)
        {
            var orderAttribute = behaviour.GetType().GetCustomAttribute<OrderAttribute>();
            var newSlot = new BehaviourSlot(orderAttribute?.Order ?? BehaviourManager.DefaultOrder, behaviour);
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
    }
}