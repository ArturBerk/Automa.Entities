using Automa.Entities.Events;
using Automa.Entities.Systems;
using Automa.Entities.Tasks;

namespace Automa.Entities
{
    public static class ContextFactory
    {
        public static IContext CreateEntitiesContext(bool debug = false)
        {
            var context = new Context(debug);
            context.SetManager(new EntityManager(debug));
            context.SetManager(new SystemManager(debug));
            context.SetManager(new TaskManager());
            context.SetManager(new EntityEventManager());
            return context;
        }
    }
}