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
            var v1 = new ComponentType[]
            {
                typeof(StructComponent),
                typeof(Struct2Component),
                typeof(ClassComponent)
            };
            for (int i = 0; i < 120000; i++)
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
            }
        }

        [Case("Referenced")]
        private void Inlining()
        {
            var entities = entityManager.Entities.ToArray();
            for (int i = 0; i < entities.Length; i++)
            {
                var reference = new EntityReference(entities[i], entityManager);
                reference.SetComponent(new Struct2Component());
                reference.SetComponent(new StructComponent());
            }
        }

    }
}
