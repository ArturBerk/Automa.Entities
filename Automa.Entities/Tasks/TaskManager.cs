using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Automa.Entities.Tasks
{
    public class TaskManager : IManager
    {
        private int workersCount = 2;
        private int index;
        private Worker[] workers;
        private readonly object sync = new object();

        public void OnAttachToContext(IContext context)
        {
            workers = new Worker[workersCount];
            for (var i = 0; i < workers.Length; i++)
            {
                workers[i] = new Worker(sync);
                workers[i].Start();
            }
        }

        public void OnDetachFromContext(IContext context)
        {
            for (var i = 0; i < workers.Length; i++)
            {
                workers[i].Stop();
            }
        }

        public void OnUpdate()
        {
        }

        public void Schedule(ITask task)
        {
            AdvanceWorkerIndex();
            Interlocked.Increment(ref workers[index].activeTasks);
            workers[index].queue.Add(task);
        }

        public void ScheduleFrom(ITaskSource taskSource)
        {
            var tasks = taskSource.Tasks();
            for (var i = 0; i < tasks.Length; i++)
            {
                var task = tasks[i];
                AdvanceWorkerIndex();
                Interlocked.Increment(ref workers[this.index].activeTasks);
                workers[this.index].queue.Add(task);
            }
        }

        public void Schedule(ITaskFor taskFor, int length, int batch)
        {
            var startIndex = 0;
            while (startIndex < length)
            {
                var endIndex = startIndex + batch;
                if (endIndex >= length) endIndex = length;
                startIndex = endIndex;
                Schedule(new TaskForItem(taskFor, startIndex, endIndex));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AdvanceWorkerIndex()
        {
            ++index;
            if (++index >= workers.Length)
            {
                index = 0;
            }
        }

        public void Wait()
        {
            while (true)
            {
                lock (sync)
                {
                    if (IsCompleted()) break;
                    Monitor.Wait(sync);
                }
            }
        }

        internal struct TaskForItem : ITask
        {
            private readonly ITaskFor taskFor;
            private readonly int StartIndex;
            private readonly int EndIndex;

            public TaskForItem(ITaskFor taskFor, int startIndex, int endIndex)
            {
                this.taskFor = taskFor;
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public void Execute()
            {
                for (int i = StartIndex; i < EndIndex; i++)
                {
                    taskFor.Execute(i);
                }
            }
        }

        private bool IsCompleted()
        {
            for (var i = 0; i < workers.Length; i++)
            {
                var activeTasks = Interlocked.Read(ref workers[i].activeTasks);
                if (activeTasks > 0) return false;
            }
            return true;
        }

        private class Worker
        {
            public readonly BlockingCollection<ITask> queue = new BlockingCollection<ITask>();
            private Thread thread;
            private readonly object sync;
            public long activeTasks = 0;

            public Worker(object sync)
            {
                this.sync = sync;
            }

            public void Start()
            {
                thread = new Thread(Run);
                thread.Start();
            }

            public void Stop()
            {
                queue.CompleteAdding();
            }

            private void Run()
            {
                foreach (var task in queue.GetConsumingEnumerable())
                {
                    task.Execute();
                    Interlocked.Decrement(ref activeTasks);
                    lock (sync)
                    {
                        Monitor.Pulse(sync);
                    }
                }
            }
        }
    }
}