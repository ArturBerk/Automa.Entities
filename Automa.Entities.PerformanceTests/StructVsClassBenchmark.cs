using Automa.Entities.Collections;
using Automa.Entities.PerformanceTests.Model;
using BenchmarkIt;

namespace Automa.Entities.PerformanceTests
{
    internal class StructVsClassBenchmark : IBenchmark
    {
        private static readonly int entityCount = 24000;

        private EntityManager entityManager;
        private StructGroup structGroup;
        private ClassGroup classGroup;

        public Result[] Execute()
        {
            Prepare();

            return Benchmark.This("Classes", TestClass)
                .Against.This("Structs", TestStructs)
                .WithWarmup(1000)
                .For(1000).Iterations();
        }

        private void Prepare()
        {
            var entityManager = new EntityManager();
            var archetype1 = new ComponentType[]
            {
                typeof(StructComponent),
                typeof(ClassComponent),
            };
            var archetype2 = new ComponentType[]
            {
                typeof(ClassComponent),
            };
            var archetype3 = new ComponentType[]
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
                    var e = entityManager.CreateEntity(archetype1);
                    entityManager.SetComponent(e, classObjects[i]);
                }
                else if (i % 3 == 1)
                {
                    var e = entityManager.CreateEntity(archetype2);
                    entityManager.SetComponent(e, classObjects[i]);
                }
                else
                {
                    entityManager.CreateEntity(archetype3);
                }
            }
            classGroup = entityManager.RegisterGroup(new ClassGroup());
            structGroup = entityManager.RegisterGroup(new StructGroup());
        }

        private void TestStructs()
        {
            var count = structGroup.Data.CalculatedCount;
            for (int i = 0; i < count; i++)
            {
                ref var structComponent = ref structGroup.Data[i];
                structComponent.Value += 10;
            }
        }

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
            public Collection<StructComponent> Data;
        }

        private class ClassGroup : Group
        {
            public Collection<ClassComponent> Data;
        }
    }
}