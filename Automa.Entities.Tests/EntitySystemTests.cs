using Automa.Entities.Collections;
using Automa.Entities.Commands;
using Automa.Entities.Events;
using Automa.Entities.Systems;
using Automa.Entities.Tests.Model;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("EntitySystem")]
    public class EntitySystemTests
    {
        [Test]
        public void InjectGroupTest()
        {
            var context = ContextFactory.CreateEntitiesContext();
            context.GetManager<EntityManager>().CreateEntity(ComponentType.Create<StructComponent>());

            var systems = context.GetManager<SystemManager>();
            var system = new TestSystem();

            Assert.IsNull(system.group);
            Assert.IsNull(system.group2);

            systems.AddSystem(system);
            
            Assert.NotNull(system.group);
            Assert.IsNull(system.group2);
        }

        [Test]
        public void PrepareGroupTest()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systems = context.GetManager<SystemManager>();
            var system = new TestSystem();
            systems.AddSystem(system);
            Assert.AreEqual(0, system.@group.Count);

            context.GetManager<EntityManager>().CreateEntity(ComponentType.Create<StructComponent>());
            context.Update();
            Assert.AreEqual(1, system.@group.Count);
        }

        [Test]
        public void InjectCommandBufferTest()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systems = context.GetManager<SystemManager>();
            var system = new TestSystem();

            Assert.IsNull(system.TestCommands);
            Assert.IsNull(system.TestCommands2);

            systems.AddSystem(system);

            Assert.NotNull(system.TestCommands);
            Assert.IsNull(system.TestCommands2);
        }

        [Test]
        public void ExecuteCommandBufferTest()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systems = context.GetManager<SystemManager>();
            var system = new TestSystem();
            Assert.IsFalse(system.CommandExecuted);
            systems.AddSystem(system);
            context.Update();
            Assert.IsTrue(system.CommandExecuted);
        }

        [Test]
        public void EventListenerTest()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var systems = context.GetManager<SystemManager>();
            var system = new TestSystem();
            Assert.IsFalse(system.EventRecieved);
            systems.AddSystem(system);
            context.Update();
            Assert.IsTrue(system.EventRecieved);
        }

        private class TestSystem : EntityUpdateSystem, IEntityEventListener<Struct2Component>
        {
            public class StructGroup : Group
            {
                public ComponentCollection<StructComponent> Structs;
            }

            public struct TestCommand : IEntityCommand
            {
                public TestSystem system;

                public TestCommand(TestSystem system)
                {
                    this.system = system;
                }

                public void Execute(EntityManager context)
                {
                    system.CommandExecuted = true;
                }
            }

            public bool CommandExecuted;
            public bool EventRecieved;

            [Inject]
            internal StructGroup group;

            internal StructGroup group2;

            [Inject]
            internal EntityCommandBuffer<TestCommand> TestCommands;

            internal EntityCommandBuffer<TestCommand> TestCommands2;

            protected override void OnSystemUpdate()
            {
                TestCommands.Add(new TestCommand(this));
                EventManager.Raise(Entity.Null, new Struct2Component());
            }

            public void OnEvent(Entity source, Struct2Component eventInstance)
            {
                EventRecieved = true;
            }
        }

    }
}
