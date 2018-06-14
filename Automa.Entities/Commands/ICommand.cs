namespace Automa.Entities.Commands
{
    public interface ICommand<in TContext>
    {
        void Execute(TContext context);
    }
}
