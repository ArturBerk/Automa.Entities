using System;
using System.Diagnostics;
using Automa.Entities.Tests.Model;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Entities")]
    public class EntityTests
    {

        [Test]
        public void CreateEntityTest()
        {
            EntityManager entityManager = new EntityManager();
            var entity = entityManager.CreateEntity(ComponentType.Create<ClassComponent>(), ComponentType.Create<StructComponent>());
            Assert.AreEqual(0, entity.Id);
            Assert.AreEqual(0, entity.Version);
            entity = entityManager.CreateEntity(ComponentType.Create<ClassComponent>());
            Assert.AreEqual(1, entity.Id);
            Assert.AreEqual(0, entity.Version);
            entity = entityManager.CreateEntity(ComponentType.Create<StructComponent>());
            Assert.AreEqual(2, entity.Id);
            Assert.AreEqual(0, entity.Version);

            Assert.AreEqual(3, entityManager.EntityCount);
        }

        [Test]
        public void RemoveEntityTest()
        {
            EntityManager entityManager = new EntityManager();
            var entity = entityManager.CreateEntity(ComponentType.Create<ClassComponent>(), ComponentType.Create<StructComponent>());
            Assert.AreEqual(1, entityManager.EntityCount);
            entityManager.RemoveEntity(entity);
            Assert.AreEqual(0, entityManager.EntityCount);
        }

        [Test]
        public void SetComponentTest()
        {
            EntityManager entityManager = new EntityManager();
            var entity1 = entityManager.CreateEntity(ComponentType.Create<ClassComponent>(), ComponentType.Create<StructComponent>());
            var entity2 = entityManager.CreateEntity(ComponentType.Create<ClassComponent>(), ComponentType.Create<StructComponent>());
            var entity3 = entityManager.CreateEntity(ComponentType.Create<ClassComponent>());

            var c1 = entityManager.GetComponent<ClassComponent>(entity1);
            var c2 = entityManager.GetComponent<ClassComponent>(entity2);
            var c3 = entityManager.GetComponent<ClassComponent>(entity3);
            Assert.IsNull(c1);
            Assert.IsNull(c2);
            Assert.IsNull(c3);

            Assert.Throws<ArgumentException>(() => entityManager.GetComponent<StructComponent>(entity3));

            entityManager.SetComponent(entity1, new ClassComponent(1));
            entityManager.SetComponent(entity2, new ClassComponent(2));
            entityManager.SetComponent(entity3, new ClassComponent(3));

            entityManager.SetComponent(entity1, new StructComponent(1));
            entityManager.SetComponent(entity2, new StructComponent(2));

            c1 = entityManager.GetComponent<ClassComponent>(entity1);
            c2 = entityManager.GetComponent<ClassComponent>(entity2);
            c3 = entityManager.GetComponent<ClassComponent>(entity3);

            Assert.AreEqual(1, c1.Value);
            Assert.AreEqual(2, c2.Value);
            Assert.AreEqual(3, c3.Value);

            ref var s1 = ref entityManager.GetComponent<StructComponent>(entity1);
            ref var s2 = ref entityManager.GetComponent<StructComponent>(entity2);
            Assert.AreEqual(1, s1.Value);
            Assert.AreEqual(2, s2.Value);

            s1.Value = 10;
            s2.Value = 20;
            //entityManager.SetComponent(entity1, new StructComponent(10));
            //entityManager.SetComponent(entity2, new StructComponent(20));
            s1 = entityManager.GetComponent<StructComponent>(entity1);
            s2 = entityManager.GetComponent<StructComponent>(entity2);
            Assert.AreEqual(10, s1.Value);
            Assert.AreEqual(20, s2.Value);
        }

        [Test]
        public void CollectionToArrayTest()
        {
            EntityManager entityManager = new EntityManager();
            var entities = new Entity[10];
            for (int i = 0; i < 10; i++)
            {
                entities[i] = entityManager.CreateEntity(ComponentType.Create<StructComponent>());
            }
            var entitiesArray = entityManager.Entities.ToArray();
            Assert.AreEqual(10, entitiesArray.Length);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(entities[i], entitiesArray[i]);
            }
        }

        [Test]
        public void HasComponentTest()
        {
            EntityManager entityManager = new EntityManager();
            var e = entityManager.CreateEntity(ComponentType.Create<StructComponent>(), ComponentType.Create<ClassComponent>(), ComponentType.Create<Struct2Component>());
            entityManager.SetComponent(e, new StructComponent(10 + e.Id));
            entityManager.SetComponent(e, new Struct2Component(20 + e.Id));
            entityManager.SetComponent(e, new ClassComponent(40 + e.Id));

            Assert.IsTrue(entityManager.HasComponent<StructComponent>(e));
            Assert.IsTrue(entityManager.HasComponent<ClassComponent>(e));
            Assert.IsTrue(entityManager.HasComponent<Struct2Component>(e));
            Assert.IsFalse(entityManager.HasComponent<Struct3Component>(e));
        }

        [Test]
        public void AddComponentTest()
        {
            EntityManager entityManager = new EntityManager();
            for (int i = 0; i < 10; i++)
            {
                var e = entityManager.CreateEntity(ComponentType.Create<StructComponent>(), ComponentType.Create<ClassComponent>(), ComponentType.Create<Struct2Component>());
                entityManager.SetComponent(e, new StructComponent(10 + e.Id));
                entityManager.SetComponent(e, new Struct2Component(20 + e.Id));
                entityManager.SetComponent(e, new ClassComponent(40 + e.Id));
            }
            for (int i = 0; i < 10; i++)
            {
                var e = entityManager.CreateEntity(ComponentType.Create<StructComponent>(), ComponentType.Create<ClassComponent>(), 
                    ComponentType.Create<Struct2Component>(), ComponentType.Create<Struct3Component>());
                entityManager.SetComponent(e, new StructComponent(10 + e.Id));
                entityManager.SetComponent(e, new Struct2Component(20 + e.Id));
                entityManager.SetComponent(e, new ClassComponent(40 + e.Id));
                entityManager.SetComponent(e, new Struct3Component(30 + e.Id));
            }
            var entities = entityManager.Entities.ToArray();
            foreach (var entity in entities)
            {
                if (!entityManager.HasComponent<Struct3Component>(entity))
                {
                    entityManager.AddComponent(entity, new Struct3Component(30 + entity.Id));
                }
            }

            foreach (var entity in entities)
            {
                Assert.AreEqual(10 + entity.Id, entityManager.GetComponent<StructComponent>(entity).Value);
                Assert.AreEqual(20 + entity.Id, entityManager.GetComponent<Struct2Component>(entity).Value);
                Assert.AreEqual(30 + entity.Id, entityManager.GetComponent<Struct3Component>(entity).Value);
                Assert.AreEqual(40 + entity.Id, entityManager.GetComponent<ClassComponent>(entity).Value);
            }
        }

        [Test]
        public void RemoveComponentTest()
        {
            EntityManager entityManager = new EntityManager();
            for (int i = 0; i < 10; i++)
            {
                var e = entityManager.CreateEntity(ComponentType.Create<StructComponent>(), ComponentType.Create<ClassComponent>(), ComponentType.Create<Struct2Component>());
                entityManager.SetComponent(e, new StructComponent(10 + e.Id));
                entityManager.SetComponent(e, new Struct2Component(20 + e.Id));
                entityManager.SetComponent(e, new ClassComponent(40 + e.Id));
            }
            var entities = entityManager.Entities.ToArray();
            foreach (var entity in entities)
            {
                entityManager.RemoveComponent<Struct2Component>(entity);
            }

            foreach (var entity in entities)
            {
                Assert.AreEqual(10 + entity.Id, entityManager.GetComponent<StructComponent>(entity).Value);
                Assert.AreEqual(40 + entity.Id, entityManager.GetComponent<ClassComponent>(entity).Value);
            }
        }


        [Test]
        public void EmptyEntityTest()
        {
            EntityManager entityManager = new EntityManager();
            var entity = entityManager.CreateEntity();
            Assert.AreEqual(1, entityManager.EntityCount);
        }

        [Test]
        public void AddComponentsTest()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>(), ComponentType.Create<Struct2Component>());
            contextEntityManager.SetComponent(entity, new StructComponent(1));
            contextEntityManager.SetComponent(entity, new Struct2Component(2));

            Assert.IsTrue(contextEntityManager.HasComponent<StructComponent>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct2Component>(entity));
            Assert.IsFalse(contextEntityManager.HasComponent<Struct3Component>(entity));
            Assert.IsFalse(contextEntityManager.HasComponent<ClassComponent>(entity));

            contextEntityManager.AddComponents(entity, new []
            {
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<Struct3Component>()
            });


            Assert.IsTrue(contextEntityManager.HasComponent<StructComponent>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct2Component>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct3Component>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<ClassComponent>(entity));
            Assert.AreEqual(2, contextEntityManager.GetComponent<Struct2Component>(entity).Value);
        }

        [Test]
        public void RemoveComponentsTest()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(
                ComponentType.Create<StructComponent>(), 
                ComponentType.Create<Struct2Component>(),
                ComponentType.Create<Struct3Component>());

            contextEntityManager.SetComponent(entity, new StructComponent(1));
            contextEntityManager.SetComponent(entity, new Struct2Component(2));

            Assert.IsTrue(contextEntityManager.HasComponent<StructComponent>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct2Component>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct3Component>(entity));

            contextEntityManager.RemoveComponents(entity, new []
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<Struct3Component>()
            });


            Assert.IsFalse(contextEntityManager.HasComponent<StructComponent>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct2Component>(entity));
            Assert.IsFalse(contextEntityManager.HasComponent<Struct3Component>(entity));
            Assert.AreEqual(2, contextEntityManager.GetComponent<Struct2Component>(entity).Value);
        }

        [Test]
        public void ChangeComponentsTest()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>(), ComponentType.Create<Struct2Component>());
            contextEntityManager.SetComponent(entity, new StructComponent(1));
            contextEntityManager.SetComponent(entity, new Struct2Component(2));

            Assert.IsTrue(contextEntityManager.HasComponent<StructComponent>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct2Component>(entity));

            contextEntityManager.ChangeComponents(entity, new ComponentType[]
            {
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<Struct3Component>()
            }, new ComponentType[]
            {
                ComponentType.Create<StructComponent>()
            });


            Assert.IsFalse(contextEntityManager.HasComponent<StructComponent>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct2Component>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<Struct3Component>(entity));
            Assert.IsTrue(contextEntityManager.HasComponent<ClassComponent>(entity));
            Assert.AreEqual(2, contextEntityManager.GetComponent<Struct2Component>(entity).Value);
        }

    }
}
