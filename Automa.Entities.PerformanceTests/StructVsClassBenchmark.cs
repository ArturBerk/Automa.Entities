using Automa.Benchmarks;
using Automa.Entities.Collections;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    internal class StructVsClassBenchmark : Benchmark
    {
        private static readonly int entityCount = 24000;

        private EntityManager entityManager;
        private StructGroup structGroup;
        private ClassGroup classGroup;

        protected override void Prepare()
        {
            IterationCount = 1000;
            var entityManager = new EntityManager();
            var entityType1 = new ComponentType[]
            {
                typeof(StructComponent),
                typeof(ClassComponent),
            };
            var entityType2 = new ComponentType[]
            {
                typeof(ClassComponent),
            };
            var entityType3 = new ComponentType[]
            {
                typeof(StructComponent)
            };
            var classObjects = new ClassComponent[entityCount];
            for (int i = classObjects.Length - 1; i >= 0; i--)
            {
                classObjects[i] = new ClassComponent(i);
            }
            for (var i = 0; i < entityCount; i++)
            {
                if (i % 3 == 0)
                {
                    var e = entityManager.CreateEntity(entityType1);
                    entityManager.SetComponent(e, classObjects[i]);
                }
                else if (i % 3 == 1)
                {
                    var e = entityManager.CreateEntity(entityType2);
                    entityManager.SetComponent(e, classObjects[i]);
                }
                else
                {
                    entityManager.CreateEntity(entityType3);
                }
            }
            classGroup = entityManager.RegisterGroup(new ClassGroup());
            structGroup = entityManager.RegisterGroup(new StructGroup());
        }

        [Case("Structs")]
        private void TestStructs()
        {
            var count = structGroup.Data.CalculatedCount;
            for (int i = 0; i < count; i++)
            {
                ref var structComponent = ref structGroup.Data[i];
                structComponent.Value += 10;
            }
        }

        [Case("Classes")]
        private void TestClass()
        {
            var count = classGroup.Data.CalculatedCount;
            for (int i = 0; i < count; i++)
            {
                classGroup.Data[i].Value += 10;
            }
        }

        private class StructGroup : Group
        {
            public ComponentCollection<StructComponent> Data;
        }

        private class ClassGroup : Group
        {
            public ComponentCollection<ClassComponent> Data;
        }
    }
}