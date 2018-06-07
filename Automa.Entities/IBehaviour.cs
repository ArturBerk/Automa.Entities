namespace Automa.Entities
{
    public interface IBehaviour
    {
        bool IsEnabled { get; set; }

        void OnAddToContext(Context context);
        void OnRemoveFromContext(Context context);
        void OnUpdate();
    }
}