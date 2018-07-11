using Automa.Benchmarks;
using Automa.Entities.Collections;
using Automa.Entities.Internal;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    class ReactivePerformanceBenchmark : Benchmark
    {
        private EntityManager entityManager;

        protected override void Prepare()
        {
            IterationCount = 100;
            entityManager = new EntityManager();
            var v1 = new ComponentType[]
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<Struct2Component>()
            };
            for (int i = 0; i < 12000; i++)
            {
                 entityManager.CreateEntity(v1);
            }
        }

        private class BenchmarkGroup : Group
        {
            public ComponentCollection<StructComponent> Data1;
            public ComponentCollection<Struct2Component> Data2;
        }

        private class BenchmarkGroupWithAdd : Group, IEntityAddedListener
        {
            public ComponentCollection<StructComponent> Data1;
            public ComponentCollection<Struct2Component> Data2;

            public void OnEntityAdded(EntityIndex index)
            {
                
            }
        }

        private class BenchmarkGroupWithRemove : Group, IEntityRemovingListener
        {
            public ComponentCollection<StructComponent> Data1;
            public ComponentCollection<Struct2Component> Data2;

            public void OnEntityRemoving(EntityIndex index)
            {
                
            }
        }

        private void CommonTest()
        {
            foreach (var entityReference in entityManager.EntityReferences)
            {
                entityReference.RemoveComponent<Struct2Component>();
            }
            foreach (var entityReference in entityManager.EntityReferences)
            {
                entityReference.AddComponent(new Struct2Component(1));
            }
            entityManager.OnUpdate();
        }

        [Case("Without listeners")]
        private void TestIndex()
        {
            CommonTest();
        }

        [Case("With add listeners")]
        private void TestAdd()
        {
            var group = entityManager.RegisterGroup(new BenchmarkGroupWithAdd());
            CommonTest();
            entityManager.UnregisterGroup(group);
        }

        [Case("With remove listeners")]
        private void TestRemove()
        {
            var group = entityManager.RegisterGroup(new BenchmarkGroupWithRemove());
            CommonTest();
            entityManager.UnregisterGroup(group);
        }

        [Case("With all listeners")]
        private void TestAll()
        {
            var group1 = entityManager.RegisterGroup(new BenchmarkGroupWithAdd());
            var group2 = entityManager.RegisterGroup(new BenchmarkGroupWithRemove());
            CommonTest();
            entityManager.UnregisterGroup(group1);
            entityManager.UnregisterGroup(group2);
        }
    }
}
