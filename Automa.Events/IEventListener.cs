namespace Automa.Events
{
    public interface IEventListener<in TEvent> where TEvent : struct
    {
        void OnEvent(TEvent eventInstance);
    }

}