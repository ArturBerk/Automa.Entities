using System;
using System.Collections.Concurrent;
using System.Threading;
using Automa.Benchmarks;
using Automa.Tasks;

namespace Automa.Entities.PerformanceTests
{
    class ConcurrentQueueBenchmark : Benchmark
    {
        private readonly BlockingCollection<int> blockingCollection = new BlockingCollection<int>();
        private readonly BlockingQueue<int> blockingQueue = new BlockingQueue<int>();
        private readonly BlockingQueueSlim<int> blockingQueueSlim = new BlockingQueueSlim<int>();

        private const int itemCount = 100;

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

        [Case("BlockingCollection")]
        public void BlockingCollection()
        {
            for (int i = 0; i < itemCount; i++)
            {
                blockingCollection.Add(i);
            }
            for (int i = 0; i < itemCount; i++)
            {
                blockingCollection.Take();
            }
        }

        [Case("BlockingQueue")]
        public void BlockingQueue()
        {
            for (int i = 0; i < itemCount; i++)
            {
                blockingQueue.Enqueue(i);
            }
            for (int i = 0; i < itemCount; i++)
            {
                blockingQueue.Dequeue();
            }
        }

        [Case("BlockingQueueSlim")]
        public void BlockingQueueSlim()
        {
            for (int i = 0; i < itemCount; i++)
            {
                blockingQueueSlim.Enqueue(i);
            }
            for (int i = 0; i < itemCount; i++)
            {
                blockingQueueSlim.Dequeue();
            }
        }

        protected override void Free()
        {
//            thread1.Interrupt();
//            thread2.Interrupt();
            //            thread3.Interrupt();
        }

        private void RunBlockingQueue()
        {
            try
            {
                while (true)
                {
                    var i = blockingQueue.Dequeue();
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }

        private void RunBlockingCollection()
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

        private void RunBlockingQueueSlim()
        {
            try
            {
                while (true)
                {
                    var i = blockingQueueSlim.Dequeue();
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }
    }
}
