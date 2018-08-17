using Automa.Events;

namespace Automa.Entities.Events
{
    public class EventManager : EventDispatcher, IManager
    {
        public void OnAttachToContext(IContext context)
        {
        }

        public void OnDetachFromContext(IContext context)
        {
        }

        public void OnUpdate()
        {
            Dispatch();
        }
    }
}