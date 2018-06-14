namespace Automa.Entities.Commands
{
    public class EntityCommandBuffer<TCommand> : CommandBuffer<TCommand, EntityManager> 
        where TCommand : ICommand<EntityManager>
    {
        public EntityCommandBuffer(EntityManager context) : base(context)
        {
        }
    }
}