namespace Automa.Entities.Systems
{
    public abstract class EntitySystemWithGroup<T> : EntitySystem where T : Group, new()
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