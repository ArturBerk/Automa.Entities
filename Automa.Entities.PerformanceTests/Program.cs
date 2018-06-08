using System;
using BenchmarkIt;

namespace Automa.Entities.PerformanceTests
{
    internal class Program
    {
        private static readonly IBenchmark[] benchmarks =
        {
            //new StructVsClassBenchmark(),
            new PerformanceBenchmark(), 
        };

        private static void Main()
        {
            foreach (var benchmark in benchmarks)
            {
                benchmark.Execute().PrintComparison();
            }
            Console.ReadKey();
        }
    }
}