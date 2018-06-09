using Automa.Entities.Internal;

namespace Automa.Entities.Events
{
    internal class EventHandler<TSource, TEvent> : ArrayList<(TSource, TEvent)>, IEventHandler where TEvent : struct
    {
        private readonly ArrayList<(TSource Source, TEvent Event)> events = new ArrayList<(TSource, TEvent)>();
        private readonly ArrayList<IEventListener<TSource, TEvent>> listeners = new ArrayList<IEventListener<TSource, TEvent>>();

        public void Raise(TSource source, TEvent eventInstance)
        {
            events.Add((source, eventInstance));
        }

        public void Dispatch()
        {
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                for (int j = 0; j < listeners.Count; j++)
                {
                    listeners[j].OnEvent(e.Source, e.Event);
                }
            }
            events.FastClear();
        }

        public void RegisterListener(IEventListener<TSource, TEvent> listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(IEventListener<TSource, TEvent> listener)
        {
            listeners.Remove(listener);
        }
    }
}