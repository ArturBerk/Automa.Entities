using Automa.Entities.Events;

namespace Automa.Entities.Systems.Internal
{
    internal class EventListenerRegistrator<TEvent> : IEventListenerRegistrator where TEvent : struct
    {
        public void Register(EntityEventManager eventManager, object listener)
        {
            eventManager.RegisterListener((IEventListener<Entity, TEvent>)listener);
        }

        public void Unregister(EntityEventManager eventManager, object listener)
        {
            eventManager.UnregisterListener((IEventListener<Entity, TEvent>)listener);
        }
    }
}