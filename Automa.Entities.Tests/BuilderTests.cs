using Automa.Entities.Builders;
using Automa.Entities.Tests.Model;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Builders")]
    public class BuilderTests
    {
        [Test]
        public void ComponentAction()
        {
            var entityManager = new EntityManager();

            var entityTemplate = new EntityBuilder(
                new ComponentAction<StructComponent>(e => new StructComponent(10)),
                new ComponentAction<Struct2Component>(e => new Struct2Component(20)));

            var entity = entityTemplate.Build(entityManager);

            Assert.IsTrue(entityManager.IsExists(entity.Entity));
            Assert.IsTrue(entity.HasComponent<StructComponent>());
            Assert.IsTrue(entity.HasComponent<Struct2Component>());
            Assert.AreEqual(10, entity.GetComponent<StructComponent>().Value);
            Assert.AreEqual(20, entity.GetComponent<Struct2Component>().Value);
        }

        [Test]
        public void ComponentValue()
        {
            var entityManager = new EntityManager();

            var entityTemplate = new EntityBuilder(
                new ComponentValue<StructComponent>(new StructComponent(10)),
                new ComponentValue<Struct2Component>(new Struct2Component(20)));

            var entity = entityTemplate.Build(entityManager);

            Assert.IsTrue(entityManager.IsExists(entity.Entity));
            Assert.IsTrue(entity.HasComponent<StructComponent>());
            Assert.IsTrue(entity.HasComponent<Struct2Component>());
            Assert.AreEqual(10, entity.GetComponent<StructComponent>().Value);
            Assert.AreEqual(20, entity.GetComponent<Struct2Component>().Value);
        }

        [Test]
        public void MultiEntity()
        {
            var entityManager = new EntityManager();

            var entityTemplate1 = new EntityBuilder(
                new ComponentValue<StructComponent>(new StructComponent(10)),
                new ComponentValue<Struct2Component>(new Struct2Component(20)));
            var entityTemplate2 = new EntityBuilder(
                new ComponentValue<StructComponent>(new StructComponent(100)),
                new ComponentValue<Struct2Component>(new Struct2Component(200)));

            var template = new Builder(entityTemplate1, entityTemplate2);
            template.Build(entityManager, array =>
            {
                Assert.AreEqual(2, entityManager.EntityCount);
                Assert.IsTrue(array[0].HasComponent<StructComponent>());
                Assert.IsTrue(array[0].HasComponent<Struct2Component>());
                Assert.AreEqual(10, array[0].GetComponent<StructComponent>().Value);
                Assert.AreEqual(20, array[0].GetComponent<Struct2Component>().Value);

                Assert.IsTrue(array[1].HasComponent<StructComponent>());
                Assert.IsTrue(array[1].HasComponent<Struct2Component>());
                Assert.AreEqual(100, array[1].GetComponent<StructComponent>().Value);
                Assert.AreEqual(200, array[1].GetComponent<Struct2Component>().Value);
            });
        }

        [Test]
        public void EntityLink()
        {
            var entityManager = new EntityManager();

            var entityTemplate1 = new EntityBuilder("Entity1",
                new ComponentAction<EntityLinkComponent>(e => new EntityLinkComponent(e["Entity2"].Entity)));
            var entityTemplate2 = new EntityBuilder("Entity2",
                new ComponentAction<EntityLinkComponent>(e => new EntityLinkComponent(e["Entity1"].Entity)));

            var template = new Builder(entityTemplate1, entityTemplate2);
            template.Build(entityManager, array =>
            {
                Assert.AreEqual(2, entityManager.EntityCount);
                Assert.IsTrue(array[0].HasComponent<EntityLinkComponent>());
                Assert.AreEqual(array[1].Entity, array[0].GetComponent<EntityLinkComponent>().Entity);

                Assert.IsTrue(array[1].HasComponent<EntityLinkComponent>());
                Assert.AreEqual(array[0].Entity, array[1].GetComponent<EntityLinkComponent>().Entity);
            });
        }
    }
}