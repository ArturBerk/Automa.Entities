namespace Automa.Entities.Behaviours
{
    public abstract class EntityBehaviorWithGroup<T> : EntityBehaviour where T : Group, new()
    {
        protected T EntityGroup;

        public override void OnAttachToContext(IContext context)
        {
            base.OnAttachToContext(context);
            EntityGroup = EntityManager.RegisterGroup(new T());
        }

        public override void OnDetachFromContext(IContext context)
        {
            EntityManager.UnregisterGroup(EntityGroup);
            base.OnDetachFromContext(context);
        }
    }
}