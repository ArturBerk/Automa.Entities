using System;

namespace Automa.Events
{
    public class EventDispatcher : IDisposable
    {
        private IEventHandler[] eventHandlers = new IEventHandler[0];

        public void Raise<TEvent>(TEvent eventInstance) where TEvent : struct
        {
            GetEventHandler<TEvent>().Raise(eventInstance);
        }

        public void Raise<TEvent>(ref TEvent eventInstance) where TEvent : struct
        {
            GetEventHandler<TEvent>().Raise(ref eventInstance);
        }

        public void RegisterListener<TEvent>(IEventListener<TEvent> listener) where TEvent : struct
        {
            GetEventHandler<TEvent>().RegisterListener(listener);
        }

        public void UnregisterListener<TEvent>(IEventListener<TEvent> listener) where TEvent : struct
        {
            GetEventHandler<TEvent>().UnregisterListener(listener);
        }

        public void Dispatch()
        {
            for (int i = 0; i < eventHandlers.Length; i++)
            {
                eventHandlers[i].Dispatch();
            }
        }

        private EventHandler<TEvent> GetEventHandler<TEvent>() where TEvent : struct
        {
            var eventType = EventType.Create<TEvent>();
            if (eventType.TypeId >= eventHandlers.Length)
            {
                Expand(eventType.TypeId + 1);
            }
            return (EventHandler<TEvent>)eventHandlers[eventType.TypeId];
        }

        private void Expand(int newSize)
        {
            var wasLength = eventHandlers.Length;
            Array.Resize(ref eventHandlers, newSize);
            for (int i = wasLength; i < eventHandlers.Length; i++)
            {
                var genericType = typeof(EventHandler<>);
                var arrayType = genericType.MakeGenericType(EventTypeManager.GetTypeFromIndex((ushort)i));
                eventHandlers[i] = (IEventHandler)Activator.CreateInstance(arrayType);
            }
        }

        public virtual void Dispose()
        {
            foreach (var eventHandler in eventHandlers)
            {
                eventHandler.Dispose();
            }
        }
    }
    
}
