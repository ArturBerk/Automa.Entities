using System;
using Automa.Benchmarks;
using Automa.Entities.Collections;
using Automa.Entities.Commands;
using Automa.Entities.Events;
using Automa.Entities.Internal;
using Automa.Entities.PerformanceTests.Model;
using Automa.Entities.Systems;
using Automa.Events;

namespace Automa.Entities.PerformanceTests
{
    class EventVsStateVsPollComponentBenchmark : Benchmark
    {
        private static int entityCount = 1000;
        private static int updateCount = 500;

        public static void DoAction()
        {
//            for (int i = 0; i < 100; i++)
//            {
//            var t = Math.Sin(10) * MathF.Cos(10);
//
//            }
        }

        protected override void Prepare()
        {
            IterationCount = 1;
        }

        [Case("Polling")]
        private void TestPollComponent()
        {
            var context = ContextFactory.CreateEntitiesContext();
            EntityManager entityManager = context.GetManager<EntityManager>();
            SystemManager systemManager = context.GetManager<SystemManager>();
            var componentAddTestSystem = new PollTestSystem();
            systemManager.AddSystem(new PollProgressTestSystem());
            systemManager.AddSystem(componentAddTestSystem);
            for (int i = 0; i < entityCount; i++)
            {
                entityManager.CreateEntity(ComponentType.Create<Progress>());
            }
            for (int i = 0; i < updateCount; i++)
            {
                context.Update();
            }
//            Console.WriteLine("Poll: " + componentAddTestSystem.updatedSystems);
        }

        [Case("Polling (10 systems)")]
        private void TestPoll10Component()
        {
            var context = ContextFactory.CreateEntitiesContext();
            EntityManager entityManager = context.GetManager<EntityManager>();
            SystemManager systemManager = context.GetManager<SystemManager>();
            var componentAddTestSystem = new PollTestSystem();
            for (int i = 0; i < 10; i++)
            {
                systemManager.AddSystem(componentAddTestSystem);
            }
            systemManager.AddSystem(new PollProgressTestSystem());
            for (int i = 0; i < entityCount; i++)
            {
                entityManager.CreateEntity(ComponentType.Create<Progress>());
            }
            for (int i = 0; i < updateCount; i++)
            {
                context.Update();
            }
//            Console.WriteLine("Poll: " + componentAddTestSystem.updatedSystems);
        }

        [Case("StateComponent")]
        private void TestStateComponent()
        {
            var context = ContextFactory.CreateEntitiesContext();
            EntityManager entityManager = context.GetManager<EntityManager>();
            SystemManager systemManager = context.GetManager<SystemManager>();
            var componentAddTestSystem = new ComponentAddTestSystem();
            systemManager.AddSystem(componentAddTestSystem);
            systemManager.AddSystem(new ComponentAddProgressTestSystem());
            for (int i = 0; i < entityCount; i++)
            {
                entityManager.CreateEntity(ComponentType.Create<Progress>());
            }
            for (int i = 0; i < updateCount; i++)
            {
                context.Update();
            }
//            Console.WriteLine("State: " + componentAddTestSystem.updatedSystems);
        }

        [Case("Event")]
        private void TestEvents()
        {
            var context = ContextFactory.CreateEntitiesContext();
            EntityManager entityManager = context.GetManager<EntityManager>();
            SystemManager systemManager = context.GetManager<SystemManager>();
            systemManager.AddSystem(new EventUpdateTestSystem());
            var eventTestSystem = new EventTestSystem();
            systemManager.AddSystem(eventTestSystem);
            for (int i = 0; i < entityCount; i++)
            {
                entityManager.CreateEntity(ComponentType.Create<Progress>());
            }
            for (int i = 0; i < updateCount; i++)
            {
                context.Update();
            }
//            Console.WriteLine("Event: " + eventTestSystem.updatedSystems);
        }

        [Case("Event (10 listeners)")]
        private void Test10Events()
        {
            var context = ContextFactory.CreateEntitiesContext();
            EntityManager entityManager = context.GetManager<EntityManager>();
            SystemManager systemManager = context.GetManager<SystemManager>();
            systemManager.AddSystem(new EventUpdateTestSystem());
            EventTestSystem eventTestSystem = null;
            for (int i = 0; i < 10; i++)
            {
                eventTestSystem = new EventTestSystem();
                systemManager.AddSystem(eventTestSystem);
            }
            for (int i = 0; i < entityCount; i++)
            {
                entityManager.CreateEntity(ComponentType.Create<Progress>());
            }
            for (int i = 0; i < updateCount; i++)
            {
                context.Update();
            }
//            Console.WriteLine("Event: " + eventTestSystem.updatedSystems);
        }

        private class ComponentAddTestSystem : EntityUpdateSystem
        {
            [Inject]
            private ProgressCompletedGroup completedGroup;

            public int updatedSystems = 0;

            protected override void OnSystemUpdate()
            {
                // Reset Progress
                for (int i = 0; i < completedGroup.Count; i++)
                {
                    ++updatedSystems;
                    completedGroup.Progress[i].Value = 1.0f;
                    DoAction();
                }
            }

            private class ProgressCompletedGroup : Group
            {
                public EntityCollection Entities;
                public ComponentCollection<Progress> Progress;
                public ComponentCollection<ProgressCompleted> ProgressCompleted;
            }
        }

        private class ComponentAddProgressTestSystem : EntityUpdateSystem
        {
            [Inject]
            private EntityCommandBuffer<ResetProgressCommand> resetCommands;
            [Inject]
            private EntityCommandBuffer<CompleteProgressCommand> completeCommands;
            [Inject]
            private ProgressGroup group;
            [Inject]
            private ProgressCompletedGroup completedGroup;

            protected override void OnSystemUpdate()
            {
                for (int i = 0; i < @group.Count; i++)
                {
                    ref var progress = ref @group.Progress[i];
                    progress.Value += 0.1f;
                    if (progress.Value > 1.0f)
                    {
                        progress.Value = 0.0f;
                        completeCommands.Add(new CompleteProgressCommand(@group.Entities[i]));
                    }
                }
                for (int i = 0; i < completedGroup.Count; i++)
                {
                    resetCommands.Add(new ResetProgressCommand(completedGroup.Entities[i]));
                }
            }

            [ExcludeComponent(typeof(ProgressCompleted))]
            private class ProgressGroup : Group
            {
                public EntityCollection Entities;
                public ComponentCollection<Progress> Progress;
            }

            private class ProgressCompletedGroup : Group
            {
                public EntityCollection Entities;
                public ComponentCollection<Progress> Progress;
                public ComponentCollection<ProgressCompleted> ProgressCompleted;
            }

            private struct CompleteProgressCommand : IEntityCommand
            {
                private readonly Entity entity;

                public CompleteProgressCommand(Entity entity)
                {
                    this.entity = entity;
                }

                public void Execute(EntityManager context)
                {
                    context.AddComponent(entity, new ProgressCompleted());
                }
            }

            private struct ResetProgressCommand : IEntityCommand
            {
                private readonly Entity entity;

                public ResetProgressCommand(Entity entity)
                {
                    this.entity = entity;
                }

                public void Execute(EntityManager context)
                {
                    context.RemoveComponent<ProgressCompleted>(entity);
                }
            }
        }

        private class EventUpdateTestSystem : EntityUpdateSystem
        {
            [Inject]
            private ProgressGroup group;

            protected override void OnSystemUpdate()
            {
                for (int i = 0; i < @group.Count; i++)
                {
                    ref var progress = ref @group.Progress[i];
                    progress.Value += 0.1f;
                    if (progress.Value >= 1.0f)
                    {
                        progress.Value = 0.0f;
                        EventManager.Raise(new ProgressCompleted(@group.Entities[i]));
                    }
                }
            }

            private class ProgressGroup : Group
            {
                public EntityCollection Entities;
                public ComponentCollection<Progress> Progress;
            }
        }

        private class EventTestSystem : EntitySystem, IEventListener<ProgressCompleted>
        {
            public int updatedSystems = 0;

            public void OnEvent(ProgressCompleted eventInstance)
            {
                if (EntityManager.HasComponent<Progress>(eventInstance.Source))
                {
                    DoAction();
                    EntityManager.SetComponent(eventInstance.Source, new Progress(1.0f));
                    ++updatedSystems;
                }
            }
        }

        private class PollProgressTestSystem : EntityUpdateSystem
        {
            [Inject]
            private ProgressGroup group;

            protected override void OnSystemUpdate()
            {
                // Separate progress system emulation
                for (int i = 0; i < @group.Count; i++)
                {
                    ref var progress = ref @group.Progress[i];
                    if (progress.Value >= 1.0f) progress.Value = 0.1f;
                    progress.Value += 0.1f;
                }
            }

            private class ProgressGroup : Group
            {
                public ComponentCollection<Progress> Progress;
            }
        }

        private class PollTestSystem : EntityUpdateSystem
        {
            [Inject]
            private ProgressGroup group;

            public int updatedSystems = 0;

            protected override void OnSystemUpdate()
            {
                for (int i = 0; i < @group.Count; i++)
                {
                    ref var progress = ref @group.Progress[i];
                    if (progress.Value >= 1.0f)
                    {
                        ++updatedSystems;
                        group.Progress[i].Value = progress.Value;
                        DoAction();
                    }
                }
            }

            private class ProgressGroup : Group
            {
                public ComponentCollection<Progress> Progress;
            }
        }

        private struct ProgressCompleted
        {
            public Entity Source;

            public ProgressCompleted(Entity source)
            {
                Source = source;
            }
        }

        private struct Progress
        {
            public float Value;

            public Progress(float value)
            {
                Value = value;
            }
        }
    }
}
