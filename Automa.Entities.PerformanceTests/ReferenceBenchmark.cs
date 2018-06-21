using Automa.Benchmarks;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    class ReferenceBenchmark : Benchmark
    {
        private EntityManager entityManager;
        
        protected override void Prepare()
        {
            IterationCount = 100;
            entityManager = new EntityManager();
            var v1 = new []
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

        [Case("Direct")]
        private void Direct()
        {
            var entities = entityManager.Entities.ToArray();
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                entityManager.SetComponent(entity, new Struct2Component());
                entityManager.SetComponent(entity, new StructComponent());
                entityManager.SetComponent(entity, new Struct2Component());
                entityManager.SetComponent(entity, new StructComponent());
                entityManager.SetComponent(entity, new Struct2Component());
                entityManager.SetComponent(entity, new StructComponent());
            }
        }

        [Case("Referenced")]
        private void Inlining()
        {
            var entities = entityManager.Entities.ToArray();
            for (int i = 0; i < entities.Length; i++)
            {
                var reference = entityManager.GetReference(entities[i]);
                reference.SetComponent(new Struct2Component());
                reference.SetComponent(new StructComponent());
                reference.SetComponent(new Struct2Component());
                reference.SetComponent(new StructComponent());
                reference.SetComponent(new Struct2Component());
                reference.SetComponent(new StructComponent());
            }
        }

    }
}
