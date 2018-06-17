using Automa.Entities.Events;

namespace Automa.Entities.Systems.Internal
{
    internal interface IEventListenerRegistrator
    {
        void Register(EntityEventManager eventManager, object listener);
        void Unregister(EntityEventManager eventManager, object listener);
    }
}