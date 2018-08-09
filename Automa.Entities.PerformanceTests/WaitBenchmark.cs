using System.Threading;
using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    class WaitBenchmark : Benchmark
    {
        private int waitCount = 100;
        private AutoResetEvent autoReset = new AutoResetEvent(false);
        private ManualResetEventSlim manualReset = new ManualResetEventSlim(false);
        private readonly object syncRoot = new object();

        private Thread thread1;
        private Thread thread2;
        private Thread thread3;

        protected override void Prepare()
        {
            IterationCount = 10;
            thread1 = new Thread(RunMonitor);
            thread1.Start();
            thread2 = new Thread(RunEvent);
            thread2.Start();
            thread3 = new Thread(RunManualEvent);
            thread3.Start();
        }

        protected override void Free()
        {
            thread1.Interrupt();
            thread2.Interrupt();
            thread3.Interrupt();
        }

        private void RunMonitor()
        {
            try
            {
                while (true)
                {
                    autoReset.WaitOne();
                    autoReset.Set();
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }

        private void RunEvent()
        {
            try
            {
                while (true)
                {
                    lock (syncRoot)
                    {
                        Monitor.Pulse(syncRoot);
                        Monitor.Wait(syncRoot);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                
            }
        }

        private void RunManualEvent()
        {
            try
            {
                while (true)
                {
                    lock (syncRoot)
                    {
                        manualReset.Set();
                        manualReset.Wait();
                        manualReset.Reset();
                    }
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }

        [Case("AutoResetEvent")]
        public void AutoResetEvent()
        {
            for (int i = 0; i < waitCount; i++)
            {
                autoReset.Set();
                autoReset.WaitOne();
            }
        }

        [Case("ManualResetEventSlim")]
        public void ManualResetEventSlim()
        {
            for (int i = 0; i < waitCount; i++)
            {
                manualReset.Set();
                manualReset.Wait();
                manualReset.Reset();
            }
        }

        [Case("Monitor")]
        public void MonitorWait()
        {
            for (int i = 0; i < waitCount; i++)
            {
                lock (syncRoot)
                {
                    Monitor.Pulse(syncRoot);
                    Monitor.Wait(syncRoot);
                }
            }
        }
    }
}
