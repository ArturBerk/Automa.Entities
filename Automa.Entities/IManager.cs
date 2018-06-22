namespace Automa.Entities
{
    public interface IManager
    {
        void OnAttachToContext(IContext context);
        void OnDetachFromContext(IContext context);
        void OnUpdate();
    }

    public abstract class ManagerBase : IManager
    {
        public IContext Context { get; private set; }

        public virtual void OnAttachToContext(IContext context)
        {
            Context = context;
        }

        public virtual void OnDetachFromContext(IContext context)
        {
            Context = null;
        }

        public virtual void OnUpdate() { }
    }

}