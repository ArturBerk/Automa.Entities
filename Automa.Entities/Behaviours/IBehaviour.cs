namespace Automa.Entities.Behaviours
{
    public interface IBehaviour
    {
        bool IsEnabled { get; set; }

        void OnAttachToContext(IContext context);
        void OnDetachFromContext(IContext context);
        void OnUpdate();
    }
}