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
        private object sync = new object();

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
            Interlocked.Increment(ref workers[index].ActiveTasks);
            workers[index].queue.Add(task);
        }

        public void ScheduleFrom(ITaskSource taskSource)
        {
            var count = taskSource.Tasks(out var tasks);
            for (var i = 0; i < count; i++)
            {
                var task = tasks[i];
                AdvanceWorkerIndex();
                Interlocked.Increment(ref workers[this.index].ActiveTasks);
                workers[this.index].queue.Add(task);
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
                try
                {
                    Monitor.Enter(sync);
                    if (IsCompleted()) break;
                    Monitor.Wait(sync, 100);
                }
                finally
                {
                    Monitor.Exit(sync);
                }
//                syncEvent.WaitOne(100);
            }
        }
        
        private bool IsCompleted()
        {
            for (var i = 0; i < workers.Length; i++)
            {
                var activeTasks = Interlocked.Read(ref workers[i].ActiveTasks);
                if (activeTasks > 0) return false;
            }
            return true;
        }

        private class Worker
        {
            public readonly BlockingCollection<ITask> queue = new BlockingCollection<ITask>();
            private Thread thread;
            private readonly object sync;
            public long ActiveTasks = 0;

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
                    try
                    {
                        task.Execute();
                    }
                    finally
                    {
                        Interlocked.Decrement(ref ActiveTasks);
                        try
                        {
                            Monitor.Enter(sync);
                            Monitor.Pulse(sync);
                        }
                        finally
                        {
                            Monitor.Exit(sync);
                        }
                    }
                }
            }
        }
    }
}