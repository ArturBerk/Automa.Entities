using System;
using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    internal class Program
    {
        private static void Main()
        {
            var b = new WaitBenchmark();
            b.Execute().Print();
            //            Benchmark.ExecuteAll();
            Console.ReadKey();
        }
    }
}