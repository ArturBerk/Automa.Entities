namespace Automa.Entities.Behaviours
{
    public abstract class EntityBehaviour : IBehaviour
    {
        public EntityManager EntityManager { get; set; }

        public virtual void OnAttachToContext(IContext context)
        {
            EntityManager = context.GetManager<EntityManager>();
        }

        public virtual void OnDetachFromContext(IContext context)
        {
            EntityManager = null;
        }

        public abstract void OnUpdate();
        public virtual bool IsEnabled { get; set; }
    }
}