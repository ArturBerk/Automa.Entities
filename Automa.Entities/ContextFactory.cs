using Automa.Entities.Events;
using Automa.Entities.Systems;

namespace Automa.Entities
{
    public static class ContextFactory
    {
        public static IContext CreateEntitiesContext(bool debug = false)
        {
            var context = new Context();
            context.SetManager(new EntityManager(debug));
            context.SetManager(new SystemManager(debug));
            context.SetManager(new EntityEventManager());
            return context;
        }
    }
}