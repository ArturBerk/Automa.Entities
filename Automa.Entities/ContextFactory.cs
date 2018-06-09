using Automa.Entities.Systems;

namespace Automa.Entities
{
    public static class ContextFactory
    {
        public static IContext CreateEntitiesContext()
        {
            var context = new Context();
            context.SetManager(new EntityManager());
            context.SetManager(new SystemManager());
            return context;
        }
    }
}