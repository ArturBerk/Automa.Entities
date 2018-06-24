using System;
using System.Collections.Generic;
using System.Threading;
using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    class StaticVsThreadLocalStaticBenchmark : Benchmark
    {
        private static List<int> instance = new List<int>();

        [ThreadStatic] private static List<int> instanceThreadStatic;

        private static List<int> InstanceThreadStatic
        {
            get
            {
                if (instanceThreadStatic == null)
                {
                    instanceThreadStatic = new List<int>();
                }
                return instanceThreadStatic;
            }
        }

        private static ThreadLocal<List<int>> instanceThreadLocal = new ThreadLocal<List<int>>(() => new List<int>());

        private EntityManager entityManager;
        
        protected override void Prepare()
        {
            IterationCount = 10000000;
        }

        [Case("Static")]
        private void Static()
        {
            var list = instance;
        }

        [Case("ThreadStatic")]
        private void ThreadStatic()
        {
            var list = InstanceThreadStatic;
        }

        [Case("ThreadLocal")]
        private void ThreadLocal()
        {
            var list = instanceThreadLocal.Value;
        }

    }
}
