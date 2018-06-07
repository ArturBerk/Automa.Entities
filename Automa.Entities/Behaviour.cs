namespace Automa.Entities
{
    public abstract class Behaviour : IBehaviour
    {
        public EntityManager EntityManager { get; set; }

        public virtual void OnAddToContext(Context context)
        {
            EntityManager = context.EntityManager;
        }

        public virtual void OnRemoveFromContext(Context context)
        {
            EntityManager = null;
        }

        public abstract void OnUpdate();
        public virtual bool IsEnabled { get; set; }
    }
}