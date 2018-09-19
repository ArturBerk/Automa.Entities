using NUnit.Framework;

namespace Automa.Behaviours.Tests
{
    [TestFixture]
    [Category("Behaviours.Reactive")]
    public class ReactiveTests
    {

        [Test]
        public void EntityAddExistedListenerTest()
        {
            World world = new World();
            for (int i = 0; i < 10; i++)
            {
                world.Entities.Add(new Entity { Value = 1 });
            }
            var listener = new Listener();
            world.Entities.GetEntities<Entity>().AddHandler((IEntityAddedHandler<Entity>)listener);
            Assert.AreEqual(10, listener.addedSum);
        }

        [Test]
        public void EntityAddListenerTest()
        {
            World world = new World();
            var listener = new Listener();
            world.Entities.GetEntities<Entity>().AddHandler((IEntityAddedHandler<Entity>)listener);
            for (int i = 0; i < 10; i++)
            {
                world.Entities.Add(new Entity { Value = 1 });
            }
            Assert.AreEqual(10, listener.addedSum);
        }

        [Test]
        public void EntityRemovedListenerTest()
        {
            World world = new World();
            var listener = new Listener();
            world.Entities.GetEntities<Entity>().AddHandler((IEntityRemovedHandler<Entity>)listener);
            for (int i = 0; i < 10; i++)
            {
                var e = new Entity {Value = 1};
                world.Entities.Add(e);
            }
            var entities = world.Entities.GetEntities<Entity>().ToArray();
            for (int i = 0; i < 10; i++)
            {
                world.Entities.Remove(entities[i]);
            }
            Assert.AreEqual(10, listener.removedSum);
        }

        [Test]
        public void EntityRemovedExistingListenerTest()
        {
            World world = new World();
            var listener = new Listener();
            world.Entities.GetEntities<Entity>().AddHandler((IEntityRemovedHandler<Entity>)listener);
            for (int i = 0; i < 10; i++)
            {
                world.Entities.Add(new Entity { Value = 1 });
            }
            world.Entities.GetEntities<Entity>().RemoveHandler((IEntityRemovedHandler<Entity>)listener);
            Assert.AreEqual(10, listener.removedSum);
        }

        [Test]
        public void BehaviourAddListenerTest()
        {
            World world = new World();
            var behaviour = new Listener();
            world.Behaviours.Add(behaviour);
            for (int i = 0; i < 10; i++)
            {
                world.Entities.Add(new Entity { Value = 1 });
            }
            Assert.AreEqual(10, behaviour.addedSum);
        }

        [Test]
        public void BehaviourRemoveListenerTest()
        {
            World world = new World();
            var behaviour = new Listener();
            world.Behaviours.Add(behaviour);
            for (int i = 0; i < 10; i++)
            {
                var e = new Entity { Value = 1 };
                world.Entities.Add(e);
            }
            var entities = world.Entities.GetEntities<Entity>().ToArray();
            for (int i = 0; i < 10; i++)
            {
                world.Entities.Remove(entities[i]);
            }
            Assert.AreEqual(10, behaviour.removedSum);
        }

        private class Entity : IEntity
        {
            public int Value;
            public EntityReference Reference { get; set; }
        }

        private class Listener : IBehaviour, IEntityAddedHandler<Entity>, IEntityRemovedHandler<Entity>
        {
            public int addedSum = 0;
            public int removedSum = 0;

            public void OnEntityAdded(ref Entity entity)
            {
                addedSum += entity.Value;
            }

            public void OnEntityRemoved(ref Entity entity)
            {
                removedSum += entity.Value;
            }

            public void OnAttach(World world)
            {
                world.Entities.GetEntities<Entity>().AddHandler((IEntityAddedHandler<Entity>)this);
                world.Entities.GetEntities<Entity>().AddHandler((IEntityRemovedHandler<Entity>)this);
            }

            public void Apply()
            {

            }
        }
    }
}
