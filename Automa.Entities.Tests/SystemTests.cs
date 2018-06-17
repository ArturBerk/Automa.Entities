using Automa.Entities.Attributes;
using Automa.Entities.Collections;
using Automa.Entities.Systems;
using Automa.Entities.Tests.Model;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Systems")]
    public class SystemTests
    {

        [Test]
        public void AddReactiveSystem()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systemManager = context.GetManager<SystemManager>();

            var system = new ReactiveSystem();
            systemManager.AddSystem(system);

            Assert.AreEqual(1, systemManager.systems.Count);
            Assert.AreEqual(0, systemManager.updateSystems.Count);
        }

        [Test]
        public void AddEnabledUpdateSystem()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systemManager = context.GetManager<SystemManager>();

            var system = new UpdateSystem();
            systemManager.AddSystem(system);

            Assert.AreEqual(1, systemManager.systems.Count);
            Assert.AreEqual(1, systemManager.updateSystems.Count);
        }

        [Test]
        public void AddDisabledUpdateSystem()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systemManager = context.GetManager<SystemManager>();

            var system = new UpdateSystem();
            system.IsEnabled = false;
            systemManager.AddSystem(system);

            Assert.AreEqual(1, systemManager.systems.Count);
            Assert.AreEqual(0, systemManager.updateSystems.Count);
        }

        [Test]
        public void DisableUpdateSystem()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systemManager = context.GetManager<SystemManager>();

            var system = new UpdateSystem();
            system.IsEnabled = true;
            systemManager.AddSystem(system);
            context.Update();

            Assert.AreEqual(1, systemManager.systems.Count);
            Assert.AreEqual(1, systemManager.updateSystems.Count);
            Assert.IsTrue(system.Updated);

            system.Updated = false;
            system.IsEnabled = false;
            context.Update();

            Assert.AreEqual(1, systemManager.systems.Count);
            Assert.AreEqual(0, systemManager.updateSystems.Count);
            Assert.IsFalse(system.Updated);
        }

        [Test]
        public void GroupInjecting()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systemManager = context.GetManager<SystemManager>();

            var system = new UpdateSystem();
            systemManager.AddSystem(system);

            Assert.NotNull(system.Group);
        }

        private class ReactiveSystem : EntitySystem
        {
            
        }

        private class UpdateSystem : EntitySystem, IUpdateSystem
        {
            public bool Updated = false;

            [Inject]
            internal TestGroup Group;

            public void OnUpdate()
            {
                Updated = true;
            }

            internal class TestGroup : Group
            {
                //public ComponentCollection<StructComponent> Structs;
            }
        }

    }
}
