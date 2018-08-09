using NUnit.Framework;

namespace Automa.Behaviours.Tests
{
    [TestFixture]
    [Category("Entities")]
    public class EntitiesTests
    {
        private class Entity1 : IEntity
        {
            public int Value1;
            public IEntityLink Link { get; set; }
        }

        private class Entity2 : IEntity
        {
            public int Value2;
            public IEntityLink Link { get; set; }
        }

        [Test]
        public void AddEntitiesTest()
        {
            var group = new EntityGroup();
            for (var i = 0; i < 5; i++)
            {
                var desc = (EntityLink<Entity1>) group.Add(new Entity1());
                Assert.AreEqual(i, desc.Index);
            }
            Assert.AreEqual(5, group.GetEntityList<Entity1>().Count);
            for (var i = 0; i < 5; i++)
            {
                var desc = (EntityLink<Entity2>) group.Add(new Entity2());
                Assert.AreEqual(i, desc.Index);
            }
            Assert.AreEqual(5, group.GetEntityList<Entity2>().Count);
        }

        [Test]
        public void RemoveEntitiesTest()
        {
            var group = new EntityGroup();
            var descs = new IEntityLink[5];
            for (var i = 0; i < 5; i++)
            {
                descs[i] = (EntityLink<Entity1>) group.Add(new Entity1());
            }
            for (var i = 0; i < 5; i++)
            {
                descs[i].Dispose();
            }
            Assert.AreEqual(0, group.GetEntityList<Entity1>().Count);
        }
    }
}