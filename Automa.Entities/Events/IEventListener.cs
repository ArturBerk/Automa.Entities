namespace Automa.Entities.Events
{
    public interface IEventListener<in TSource, in TEvent> where TEvent : struct
    {
        void OnEvent(TSource source, TEvent eventInstance);
    }
}