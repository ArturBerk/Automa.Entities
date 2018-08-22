using System.Collections.Concurrent;
using System.Threading;
using Automa.Benchmarks;
using Automa.Common;

namespace Automa.Entities.PerformanceTests
{
    class ConcurrentQueueBenchmark : Benchmark
    {
        private readonly BlockingCollection<int> blockingCollection = new BlockingCollection<int>();
        private readonly WorkerQueue<int> workerQueue = new WorkerQueue<int>();

//        private Thread thread1;
//        private Thread thread2;
//        private Thread thread3;

        private class WorkerQueue<T>
        {
            private readonly SemaphoreSlim free = new SemaphoreSlim(1);
            private readonly ManualResetEventSlim enqueued = new ManualResetEventSlim(false);
            private ArrayList<T> queue = new ArrayList<T>(4);

            public void Enqueue(T value)
            {
                free.Wait();
                queue.Add(value);
                free.Release();
                enqueued.Set();
            }

            public ref T Dequeue()
            {
                free.Wait();
                if (queue.Count > 0)
                {
                    var index = queue.Count - 1;
                    ref var result = ref queue[index];
                    queue.RemoveAt(index);
                    free.Release();
                    return ref result;
                }
                free.Release();
                enqueued.Wait();
                enqueued.Reset();
                return ref Dequeue();
            }
        }

        protected override void Prepare()
        {
            IterationCount = 1000;
//            thread1 = new Thread(RunWorkerQueue);
//            thread1.Start();
//            thread2 = new Thread(RunBlockingQueue);
//            thread2.Start();
            //            thread3 = new Thread(RunManualEvent);
            //            thread3.Start();
        }

        [Case("RunBlockingQueue")]
        public void BlockingQueue()
        {
            for (int i = 0; i < 1000; i++)
            {
                blockingCollection.Add(i);
            }
            for (int i = 0; i < 1000; i++)
            {
                blockingCollection.Take();
            }
        }

        [Case("RunQueue")]
        public void Queue()
        {
            for (int i = 0; i < 1000; i++)
            {
                workerQueue.Enqueue(i);
            }
            for (int i = 0; i < 1000; i++)
            {
                workerQueue.Dequeue();
            }
        }

        protected override void Free()
        {
//            thread1.Interrupt();
//            thread2.Interrupt();
            //            thread3.Interrupt();
        }

        private void RunWorkerQueue()
        {
            try
            {
                while (true)
                {
                    var i = workerQueue.Dequeue();
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }

        private void RunBlockingQueue()
        {
            try
            {
                while (true)
                {
                    var i = blockingCollection.Take();
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }
    }
}
