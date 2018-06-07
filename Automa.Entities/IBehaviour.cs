namespace Automa.Entities
{
    public interface IBehaviour
    {
        EntityManager EntityManager { get; set; }
        bool IsEnabled { get; set; }

        void OnAddToContext(Context context);
        void OnRemoveFromContext(Context context);
        void OnUpdate();
    }
}