using System.Threading;

namespace Automa.Tasks
{
    public interface ITask
    {
        ManualResetEventSlim Completed { get; }

        void Execute();
    }

    public abstract class Task : ITask
    {
        public ManualResetEventSlim Completed { get; } = new ManualResetEventSlim(false);

        public abstract void Execute();
    }
}