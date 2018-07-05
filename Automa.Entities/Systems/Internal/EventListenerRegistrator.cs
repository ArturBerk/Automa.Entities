using Automa.Entities.Events;

namespace Automa.Entities.Systems.Internal
{
    internal class EventListenerRegistrator<TEvent> : IEventListenerRegistrator where TEvent : struct
    {
        public void Register(EventManager eventManager, object listener)
        {
            eventManager.RegisterListener((IEventListener<TEvent>)listener);
        }

        public void Unregister(EventManager eventManager, object listener)
        {
            eventManager.UnregisterListener((IEventListener<TEvent>)listener);
        }
    }
}