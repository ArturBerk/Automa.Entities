using Automa.Benchmarks;
using Automa.Entities.Collections;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    class PerformanceBenchmark : Benchmark
    {
        private BenchmarkGroup group;

        protected override void Prepare()
        {
            IterationCount = 1000;
            EntityManager entityManager = new EntityManager();
            var v1 = new ComponentType[]
            {
                typeof(StructComponent),
                typeof(Struct2Component),
                typeof(Struct3Component),
                typeof(ClassComponent)
            };
            var v2 = new ComponentType[]
            {
                typeof(StructComponent),
                typeof(Struct2Component),
                typeof(Struct3Component)
            };
            var v3 = new ComponentType[]
            {
                typeof(StructComponent),
                typeof(Struct2Component)
            };
            for (int i = 0; i < 12000; i++)
            {
                if (i % 3 == 0)
                {
                    entityManager.CreateEntity(v1);
                }
                else if (i % 3 == 1)
                {
                    entityManager.CreateEntity(v2);
                }
                else
                {
                    entityManager.CreateEntity(v3);
                }
            }
            @group = entityManager.RegisterGroup(new BenchmarkGroup());
        }

        private class BenchmarkGroup : Group
        {
            public Collection<StructComponent> Data1;
            public Collection<Struct2Component> Data2;
        }

        [Case("Performance")]
        private void Test()
        {
            @group.UpdateLength();
            for (int i = 0; i < @group.Length; i++)
            {
                @group.Data1[i].Value += @group.Data1[i].Value;
                @group.Data2[i].Value += @group.Data2[i].Value;
            }
        }
    }
}
