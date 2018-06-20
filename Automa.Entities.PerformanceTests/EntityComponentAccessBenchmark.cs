using Automa.Benchmarks;
using Automa.Entities.Collections;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    class EntityComponentAccessBenchmark : Benchmark
    {
        private EntityManager entityManager;
        private BenchmarkGroup group;

        protected override void Prepare()
        {
            IterationCount = 1000;
            entityManager = new EntityManager();
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
            public EntityCollection Entities;
            public ComponentCollection<StructComponent> Data1;
            public ComponentCollection<Struct2Component> Data2;
        }

        [Case("Group access")]
        private void TestIndex()
        {
            @group.Update();
            for (int i = 0; i < @group.Count; i++)
            {
                ref var data1 = ref @group.Data1[i];
                //ref var data2 = ref @group.Data2[i];
                data1.Value += data1.Value;
                //data2.Value += data2.Value;
            }
        }

        [Case("Direct access")]
        private void TestIterator()
        {
            @group.Update();
            for (int i = 0; i < @group.Count; ++i)
            {
                var entity = @group.Entities[i];
                ref var data1 = ref entityManager.GetComponent<StructComponent>(entity);
                //ref var data2 = ref entityManager.GetComponent<Struct2Component>(entity);
                data1.Value += data1.Value;
                //data2.Value += data2.Value;
            }
        }
    }
}
