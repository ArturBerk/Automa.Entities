using System;
using Automa.Common;

namespace Automa.Events
{
    internal interface IEventHandler : IDisposable
    {
        void Dispatch();
    }

    internal sealed class EventHandler<TEvent> : IEventHandler where TEvent : struct
    {
        private ArrayList<TEvent> events = new ArrayList<TEvent>(4);
        private ArrayList<IEventListener<TEvent>> listeners = new ArrayList<IEventListener<TEvent>>(4);

        public void Raise(TEvent eventInstance)
        {
            events.Add(eventInstance);
        }

        public void Raise(ref TEvent eventInstance)
        {
            events.Add(ref eventInstance);
        }

        public void Dispatch()
        {
            try
            {
                for (int i = 0; i < events.Count; i++)
                {
                    var e = events[i];
                    for (int j = 0; j < listeners.Count; j++)
                    {
                        listeners[j].OnEvent(e);
                    }
                }
            }
            finally
            {
                events.FastClear();
            }
        }

        public void RegisterListener(IEventListener<TEvent> listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(IEventListener<TEvent> listener)
        {
            listeners.Remove(listener);
        }

        public void Dispose()
        {
            events.Clear();
            listeners.Clear();
        }
    }
}