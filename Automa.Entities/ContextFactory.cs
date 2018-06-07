using Automa.Entities.Behaviours;

namespace Automa.Entities
{
    public static class ContextFactory
    {
        public static IContext CreateEntitiesContext()
        {
            var context = new Context();
            context.SetManager(new EntityManager());
            context.SetManager(new BehaviourManager());
            return context;
        }
    }
}