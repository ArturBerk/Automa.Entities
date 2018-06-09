namespace Automa.Entities.Systems
{
    public interface ISystem
    {
        bool IsEnabled { get; set; }

        void OnAttachToContext(IContext context);
        void OnDetachFromContext(IContext context);
        void OnUpdate();
    }
}