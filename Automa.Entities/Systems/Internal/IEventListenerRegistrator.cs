using Automa.Entities.Events;

namespace Automa.Entities.Systems.Internal
{
    internal interface IEventListenerRegistrator
    {
        void Register(EventManager eventManager, object listener);
        void Unregister(EventManager eventManager, object listener);
    }
}