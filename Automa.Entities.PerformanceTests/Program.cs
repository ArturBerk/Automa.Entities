using System;
using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    internal class Program
    {
        private static void Main()
        {
            var b = new EntitiesVsBehavioursBenchmark();
            b.Execute().Print();
            //            Benchmark.ExecuteAll();
            Console.ReadKey();
        }
    }
}