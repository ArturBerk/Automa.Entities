using Automa.Benchmarks;
using Automa.Entities.Collections;
using Automa.Entities.Internal;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    class IteratorBenchmark : Benchmark
    {
        private BenchmarkGroup group;

        protected override void Prepare()
        {
            IterationCount = 1000;
            EntityManager entityManager = new EntityManager();
            var v1 = new ComponentType[]
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<Struct2Component>(),
                ComponentType.Create<Struct3Component>(),
                ComponentType.Create<ClassComponent>()
            };
            var v2 = new ComponentType[]
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<Struct2Component>(),
                ComponentType.Create<Struct3Component>()
            };
            var v3 = new ComponentType[]
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<Struct2Component>()
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
            public ComponentCollection<StructComponent> Data1;
            public ComponentCollection<Struct2Component> Data2;
        }

        [Case("Index")]
        private void TestIndex()
        {
            @group.Update();
            for (int i = 0; i < @group.Count; i++)
            {
                @group.Data1[i].Value += @group.Data1[i].Value;
                @group.Data2[i].Value += @group.Data2[i].Value;
            }
        }

        [Case("Iterator")]
        private void TestIterator()
        {
            @group.Update();
            for (var iterator = @group.GetIterator(); iterator.MoveNext();)
            {
                @group.Data1[iterator.Index].Value += @group.Data1[iterator.Index].Value;
                @group.Data2[iterator.Index].Value += @group.Data2[iterator.Index].Value;
            }
        }
    }
}
