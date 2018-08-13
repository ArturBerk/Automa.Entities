using NUnit.Framework;

namespace Automa.Behaviours.Tests
{
    [TestFixture]
    [Category("Reactive")]
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
            world.Entities.GetEntityList<Entity>().AddHandler((IEntityAddedHandler<Entity>)listener);
            Assert.AreEqual(10, listener.addedSum);
        }

        [Test]
        public void EntityAddListenerTest()
        {
            World world = new World();
            var listener = new Listener();
            world.Entities.GetEntityList<Entity>().AddHandler((IEntityAddedHandler<Entity>)listener);
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
            world.Entities.GetEntityList<Entity>().AddHandler((IEntityRemovedHandler<Entity>)listener);
            for (int i = 0; i < 10; i++)
            {
                world.Entities.Add(new Entity { Value = 1 });
            }
            var entities = world.Entities.GetEntityList<Entity>().ToArray();
            for (int i = 0; i < 10; i++)
            {
                entities[i].Link.Dispose();
            }
            Assert.AreEqual(10, listener.removedSum);
        }

        [Test]
        public void EntityRemovedExistingListenerTest()
        {
            World world = new World();
            var listener = new Listener();
            world.Entities.GetEntityList<Entity>().AddHandler((IEntityRemovedHandler<Entity>)listener);
            for (int i = 0; i < 10; i++)
            {
                world.Entities.Add(new Entity { Value = 1 });
            }
            world.Entities.GetEntityList<Entity>().RemoveHandler((IEntityRemovedHandler<Entity>)listener);
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
                world.Entities.Add(new Entity { Value = 1 });
            }
            var entities = world.Entities.GetEntityList<Entity>().ToArray();
            for (int i = 0; i < 10; i++)
            {
                entities[i].Link.Dispose();
            }
            Assert.AreEqual(10, behaviour.removedSum);
        }

        private class Entity : IEntity
        {
            public int Value;
            public IEntityLink Link { get; set; }
        }

        private class Listener : IBehaviour, IEntityAddedHandler<Entity>, IEntityRemovedHandler<Entity>
        {
            public int addedSum = 0;
            public int removedSum = 0;

            public void OnEntityAdded(Entity entity)
            {
                addedSum += entity.Value;
            }

            public void OnEntityRemoved(Entity entity)
            {
                removedSum += entity.Value;
            }

            public void OnAttach(World world)
            {
                
            }

            public void Apply()
            {
                
            }
        }
    }
}
