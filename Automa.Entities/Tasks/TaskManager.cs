using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Automa.Entities.Tasks
{
    public class TaskManager : IManager
    {
        private const int WorkersCount = 8;
        private int index;
        private Worker[] workers;
        private readonly AutoResetEvent taskCompletedEvent = new AutoResetEvent(false);
        private long activeTasks = 0;

        public void OnAttachToContext(IContext context)
        {
            workers = new Worker[WorkersCount];
            for (var i = 0; i < workers.Length; i++)
            {
                workers[i] = new Worker(this);
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
            Interlocked.Increment(ref activeTasks);
            workers[index].queue.Enqueue(task);
            workers[index].TaskEnqueuedEvent.Set();
        }

        public void ScheduleFrom(ITaskSource taskSource)
        {
            var count = taskSource.Tasks(out var tasks);
            for (var i = 0; i < count; i++)
            {
                var task = tasks[i];
                AdvanceWorkerIndex();
                Interlocked.Increment(ref activeTasks);
                workers[index].queue.Enqueue(task);
                workers[index].TaskEnqueuedEvent.Set();
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
                taskCompletedEvent.WaitOne(100);
                if (Interlocked.Read(ref activeTasks) == 0) break;
            }
        }

        private class Worker
        {
            public readonly ConcurrentQueue<ITask> queue = new ConcurrentQueue<ITask>();
            private Thread thread;
            private readonly TaskManager taskManager;
            public readonly AutoResetEvent TaskEnqueuedEvent = new AutoResetEvent(false);

            public Worker(TaskManager taskManager)
            {
                this.taskManager = taskManager;
            }

            public void Start()
            {
                thread = new Thread(Run);
                thread.Start();
            }

            public void Stop()
            {
                thread.Interrupt();
            }

            private void Run()
            {
                try
                {
                    while (true)
                    {
                        TaskEnqueuedEvent.WaitOne();
                        while (queue.TryDequeue(out var task))
                        {
                            try
                            {
                                task.Execute();
                            }
                            finally
                            {
                                Interlocked.Decrement(ref taskManager.activeTasks);
                                taskManager.taskCompletedEvent.Set();
                            }
                        }
                    }
                }
                catch (ThreadInterruptedException) { }
            }
        }
    }
}