using System;
using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    internal class Program
    {
        private static void Main()
        {
//            EntityComponentAccessBenchmark b = new EntityComponentAccessBenchmark();
            //Console.ReadKey();
//            b.Execute().Print();
            Benchmark.ExecuteAll();
            Console.ReadKey();
        }
    }
}