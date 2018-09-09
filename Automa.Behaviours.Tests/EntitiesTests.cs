using NUnit.Framework;

namespace Automa.Behaviours.Tests
{
    [TestFixture]
    [Category("Behaviours.Entities")]
    public class EntitiesTests
    {
        private class Entity1
        {
            public int Value1;
        }

        private class Entity2
        {
            public int Value2;
        }

        [Test]
        public void AddEntitiesTest()
        {
            var group = new EntityGroup();
            for (var i = 0; i < 5; i++)
            {
                var desc = group.Add(new Entity1());
                Assert.AreEqual(i, desc.Index);
            }
            Assert.AreEqual(5, group.GetEntities<Entity1>().Count);
            for (var i = 0; i < 5; i++)
            {
                var desc = group.Add(new Entity2());
                Assert.AreEqual(i, desc.Index);
            }
            Assert.AreEqual(5, group.GetEntities<Entity2>().Count);
        }

        [Test]
        public void RemoveEntitiesTest()
        {
            var group = new EntityGroup();
            var descs = new EntityReference[5];
            for (var i = 0; i < 5; i++)
            {
                descs[i] = (EntityReference) group.Add(new Entity1());
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
            var reference = group.Add(e);
            var reference2 = group.Add(e2);
            var type = e.GetType().BaseType;
            var r = reference;
            while (type != null && !type.IsAbstract && type != typeof(object))
            {
                r = group.AddConnected(type, e, r);
                type = type.BaseType;
            }
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
            var reference = group.Add(e);
            var reference2 = group.Add(e2);
            var type = e.GetType().BaseType;
            var r = reference2;
            while (type != null && !type.IsAbstract && type != typeof(object))
            {
                r = group.AddConnected(type, e, r);
                type = type.BaseType;
            }
            group.Remove(reference);
            Assert.AreEqual(1, group.GetEntities<Entity4>().Count);
            Assert.AreEqual(1, group.GetEntities<Entity3>().Count);
            Assert.AreEqual(1, group.GetEntities<Entity1>().Count);
        }

        private class Entity3 : Entity1
        {

        }

        private class Entity4 : Entity3
        {

        }
    }
}