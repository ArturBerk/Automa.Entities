using System;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public class Context
    {
        private readonly ArrayList<IBehaviour> behaviours = new ArrayList<IBehaviour>();
        private readonly EntityManager entityManager = new EntityManager();

        public EntityManager EntityManager => entityManager;

        public void AddBehaviour(IBehaviour behaviour)
        {
            if (behaviours.Contains(behaviour))
            {
                return;
            }
            if (behaviour.EntityManager != null)
            {
                throw new ArgumentException("Behaviour already in context");
            }
            behaviours.Add(behaviour);
            behaviour.EntityManager = entityManager;
            behaviour.OnAddToContext(this);
        }

        public void RemoveBehaviour(IBehaviour behaviour)
        {
            if (behaviours.Remove(behaviour))
            {
                behaviour.EntityManager = null;
                behaviour.OnRemoveFromContext(this);
            }
        }

        public void Update()
        {
            //entityManager.Update();
            var rawArray = behaviours.ToArrayFast();
            for (var i = 0; i < behaviours.Count; i++)
            {
                rawArray[i].OnUpdate();
            }
        }
    }
}