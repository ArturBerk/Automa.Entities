using Automa.Benchmarks;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    class AddRemoveComponentBenchmark : Benchmark
    {
        private EntityManager entityManager;

        protected override void Prepare()
        {
            IterationCount = 20;
            entityManager = new EntityManager();
            var v1 = new[]
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<Struct2Component>(),
                ComponentType.Create<ClassComponent>()
            };
            for (int i = 0; i < 12000; i++)
            {
                entityManager.CreateEntity(v1);
            }
        }

        [Case("Add/Remove separated")]
        private void TestIndex()
        {
            foreach (var entity in entityManager)
            {
                entityManager.RemoveComponent<Struct2Component>(entity);
                entityManager.AddComponent(entity, new Struct3Component());
            }
            foreach (var entity in entityManager)
            {
                entityManager.RemoveComponent<Struct3Component>(entity);
                entityManager.AddComponent(entity, new Struct2Component());
            }
        }

        [Case("Add/Remove united")]
        private void TestUnited()
        {
            var remove = new [] { ComponentType.Create<Struct2Component>() };
            var add = new [] { ComponentType.Create<Struct3Component>() };
            foreach (var entity in entityManager)
            {
                entityManager.ChangeComponents(entity, add, remove);
                entityManager.SetComponent(entity, new Struct3Component());
            }
            foreach (var entity in entityManager)
            {
                entityManager.ChangeComponents(entity, remove, add);
            }
        }

    }
}
