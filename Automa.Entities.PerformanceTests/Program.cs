using System;
using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    internal class Program
    {
        private static void Main()
        {
            Benchmark.ExecuteAll();
            Console.ReadKey();
        }
    }
}