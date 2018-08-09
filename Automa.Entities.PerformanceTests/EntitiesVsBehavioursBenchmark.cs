using Automa.Benchmarks;
using Automa.Behaviours;
using Automa.Entities.Collections;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    public class EntitiesVsBehavioursBenchmark : Benchmark
    {
        private static readonly int entityCount = 24000;

        private World world;

        private EntityManager entityManager;
        private StructGroup structGroup;

        protected override void Prepare()
        {
            IterationCount = 1000;
            var entityManager = new EntityManager();
            var entityType1 = new ComponentType[]
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<Struct2Component>(),
                ComponentType.Create<ClassComponent>(),
            };
            var entityType2 = new ComponentType[]
            {
                ComponentType.Create<Struct2Component>(),
                ComponentType.Create<ClassComponent>(),
            };
            var entityType3 = new ComponentType[]
            {
                ComponentType.Create<Struct2Component>(),
                ComponentType.Create<StructComponent>()
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
            structGroup = entityManager.RegisterGroup(new StructGroup());

            world = new World();
            for (int i = 0; i < entityCount; i++)
            {
                world.Entities.Add(new Entity(i));
            }
            world.Behaviours.Add(new EntityBehaviour());
        }

        [Case("Entities")]
        public void Entities()
        {
            var count = structGroup.Data.CalculatedCount;
            for (int i = 0; i < count; i++)
            {
                ref var structComponent = ref structGroup.Data2[i];
//                ref var struct2Component = ref structGroup.Data2[i];
                structComponent.Value += 10;
//                struct2Component.Value += 10;
            }
        }

        [Case("Behaviours")]
        public void Behaviours()
        {
            world.Behaviours.Apply();
        }

        private class StructGroup : Group
        {
            public ComponentCollection<StructComponent> Data;
            public ComponentCollection<ClassComponent> Data2;
        }

        private class Entity
        {
            public int Value;
            public int Value2;

            public Entity(int value)
            {
                Value = value;
            }
        }

        private class EntityBehaviour : IBehaviour<Entity>
        {
            public void Apply(Automa.Behaviours.EntityCollection<Entity> entities)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    entities[i].Value += 10;
                    //entities[i].Value2 += 10;
                }
            }
        }
    }
}