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
    }
}