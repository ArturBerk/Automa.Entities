using Automa.Benchmarks;
using Automa.Entities.Collections;
using Automa.Entities.Internal;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    class AddRemoveComponentBenchmark : Benchmark
    {
        private EntityManager entityManager;
        
        protected override void Prepare()
        {
            IterationCount = 100;
            entityManager = new EntityManager();
            var v1 = new ComponentType[]
            {
                typeof(StructComponent),
                typeof(Struct2Component),
                typeof(ClassComponent)
            };
            for (int i = 0; i < 12000; i++)
            {
                entityManager.CreateEntity(v1);
            }
        }

        [Case("Add/Remove separated")]
        private void TestIndex()
        {
            var entities = entityManager.Entities.ToArray();
            for (int i = 0; i < entities.Length; i++)
            {
                entityManager.RemoveComponent<Struct2Component>(entities[i]);
                entityManager.AddComponent(entities[i], new Struct3Component());
            }
            for (int i = 0; i < entities.Length; i++)
            {
                entityManager.RemoveComponent<Struct3Component>(entities[i]);
                entityManager.AddComponent(entities[i], new Struct2Component());
            }
        }

        [Case("Add/Remove united")]
        private void TestUnited()
        {
            var entities = entityManager.Entities.ToArray();
            var remove = new ComponentType[] { typeof(Struct2Component)};
            var add = new ComponentType[] { typeof(Struct3Component) };
            for (int i = 0; i < entities.Length; i++)
            {
                entityManager.ChangeComponents(entities[i], add, remove);
                entityManager.SetComponent(entities[i], new Struct3Component());
            }
            for (int i = 0; i < entities.Length; i++)
            {
                entityManager.ChangeComponents(entities[i], remove, add);
            }
        }

    }
}
