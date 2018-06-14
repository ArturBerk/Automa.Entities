using Automa.Entities.Internal;

namespace Automa.Entities.Commands
{
    public class CommandBuffer<TCommand, TContext> where TCommand : ICommand<TContext>
    {
        public readonly TContext Context;

        public CommandBuffer(TContext context)
        {
            Context = context;
        }

        private readonly ArrayList<TCommand> buffer = new ArrayList<TCommand>();

        public void Clear()
        {
            buffer.Clear();
        }

        public void Add(TCommand command)
        {
            buffer.Add(command);
        }

        public void Execute()
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                buffer[i].Execute(Context);
            }
        }

        public interface ICommandList
        {
            int Count { get; }
            ref TCommand this[int index] { get; }
        }
    }
}
