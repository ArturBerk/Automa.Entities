using System.Runtime.CompilerServices;
using System.Threading;
using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    class LockBenchmark : Benchmark
    {
        private int lockCount = 100000;
        private int lockState = 0;
        private InterlockedLock interlockedLock;
        private object syncRoot = new object();

        protected override void Prepare()
        {
            IterationCount = 100;
        }


        [Case("Lock")]
        public void Lock()
        {
            for (int i = 0; i < lockCount; i++)
            {
                lock (syncRoot)
                {
                    
                }
            }
        }

        [Case("Monitor")]
        public void MonitorLock()
        {
            for (int i = 0; i < lockCount; i++)
            {
                if (Monitor.TryEnter(syncRoot))
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }

        [Case("Interlocked")]
        public void InterlockedLock1()
        {
            for (int i = 0; i < lockCount; i++)
            {
                if (Interlocked.CompareExchange(ref lockState, 1, 0) == 0)
                {
                    Interlocked.Decrement(ref lockState);
                }
            }
        }

        [Case("Interlocked (struct)")]
        public void InterlockedLock2()
        {
            for (int i = 0; i < lockCount; i++)
            {
                if (interlockedLock.TryLock())
                {
                    interlockedLock.Unlock();
                }
            }
        }

        private struct InterlockedLock
        {
            private int state;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryLock()
            {
                return Interlocked.CompareExchange(ref state, 1, 0) == 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Unlock()
            {
                Interlocked.Decrement(ref state);
            }
        }
    }
}
