using BenchmarkIt;

namespace Automa.Entities.PerformanceTests
{
    public interface IBenchmark
    {
        Result[] Execute();
    }
}