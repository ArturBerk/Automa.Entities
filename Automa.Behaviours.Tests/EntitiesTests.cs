using NUnit.Framework;

namespace Automa.Behaviours.Tests
{
    [TestFixture]
    [Category("Behaviours.Entities")]
    public class EntitiesTests
    {
        private class Entity1 : IEntity
        {
            public int Value1;
            public EntityReference Reference { get; set; }
        }

        private class Entity2 : IEntity
        {
            public int Value2;
            public EntityReference Reference { get; set; }
        }

        [Test]
        public void AddEntitiesTest()
        {
            var group = new EntityGroup();
            for (var i = 0; i < 5; i++)
            {
                var entity = group.Add(new Entity1());
                Assert.AreEqual(i, entity.Reference.Index);
            }
            Assert.AreEqual(5, group.GetEntities<Entity1>().Count);
            for (var i = 0; i < 5; i++)
            {
                var entity = group.Add(new Entity2());
                Assert.AreEqual(i, entity.Reference.Index);
            }
            Assert.AreEqual(5, group.GetEntities<Entity2>().Count);
        }

        [Test]
        public void RemoveEntitiesTest()
        {
            var group = new EntityGroup();
            var descs = new Entity1[5];
            for (var i = 0; i < 5; i++)
            {
                descs[i] = group.Add(new Entity1());
            }
            for (var i = 0; i < 5; i++)
            {
                group.Remove(descs[i]);
            }
            Assert.AreEqual(0, group.GetEntities<Entity1>().Count);
        }

        [Test]
        public void TestAddConnected()
        {
            var group = new EntityGroup();
            var e = new Entity4();
            var e2 = new Entity4();
            group.Add(e, true);
            group.Add(e2);
            Assert.AreEqual(2, group.GetEntities<Entity4>().Count);
            Assert.AreEqual(1, group.GetEntities<Entity3>().Count);
            Assert.AreEqual(1, group.GetEntities<Entity1>().Count);
        }

        [Test]
        public void TestRemoveConnected()
        {
            var group = new EntityGroup();
            var e = new Entity4();
            var e2 = new Entity4();
            group.Add(e, true);
            group.Add(e2);

            group.Remove(e);
            Assert.AreEqual(1, group.GetEntities<Entity4>().Count);
            Assert.AreEqual(0, group.GetEntities<Entity3>().Count);
            Assert.AreEqual(0, group.GetEntities<Entity1>().Count);

            group.Remove(e2);
            Assert.AreEqual(0, group.GetEntities<Entity4>().Count);
            Assert.AreEqual(0, group.GetEntities<Entity3>().Count);
            Assert.AreEqual(0, group.GetEntities<Entity1>().Count);

        }

        private class Entity3 : Entity1
        {

        }

        private class Entity4 : Entity3
        {

        }
    }
}