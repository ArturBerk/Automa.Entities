using System;

namespace Automa.Entities.Events
{
    public class EventManager<TSource> : IManager
    {
        private IEventHandler[] eventHandlers = new IEventHandler[0];

        public void Raise<TEvent>(TSource source, TEvent eventInstance) where TEvent : struct
        {
            GetEventHandler<TEvent>().Raise(source, eventInstance);
        }

        public void RegisterListener<TEvent>(IEventListener<TSource, TEvent> listener) where TEvent : struct
        {
            GetEventHandler<TEvent>().RegisterListener(listener);
        }

        public void UnregisterListener<TEvent>(IEventListener<TSource, TEvent> listener) where TEvent : struct
        {
            GetEventHandler<TEvent>().UnregisterListener(listener);
        }

        public void OnAttachToContext(IContext context)
        {
        }

        public void OnDetachFromContext(IContext context)
        {
        }

        public void OnUpdate()
        {
            for (int i = 0; i < eventHandlers.Length; i++)
            {
                eventHandlers[i].Dispatch();
            }
        }

        private EventHandler<TSource, TEvent> GetEventHandler<TEvent>() where TEvent : struct
        {
            var eventType = EventType.Create<TEvent>();
            if (eventType.TypeId >= eventHandlers.Length)
            {
                Expand(eventType.TypeId + 1);
            }
            return (EventHandler<TSource, TEvent>)eventHandlers[eventType.TypeId];
        }

        private void Expand(int newSize)
        {
            var wasLength = eventHandlers.Length;
            Array.Resize(ref eventHandlers, newSize);
            for (int i = wasLength; i < eventHandlers.Length; i++)
            {
                var genericType = typeof(EventHandler<,>);
                var arrayType = genericType.MakeGenericType(typeof(TSource), EventTypeManager.GetTypeFromIndex((ushort)i));
                eventHandlers[i] = (IEventHandler)Activator.CreateInstance(arrayType);
            }
        }
    }

    public class EntityEventManager : EventManager<Entity> { }
}
