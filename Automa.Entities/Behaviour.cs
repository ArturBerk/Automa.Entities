namespace Automa.Entities
{
    public abstract class Behaviour : IBehaviour
    {
        public virtual void OnAddToContext(Context context)
        {
        }

        public virtual void OnRemoveFromContext(Context context)
        {
        }

        public abstract void OnUpdate();

        public EntityManager EntityManager { get; set; }
        public virtual bool IsEnabled { get; set; }
    }
}