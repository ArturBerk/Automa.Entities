using System;
using Automa.Benchmarks;
using Automa.Entities.Attributes;
using Automa.Entities.Collections;
using Automa.Entities.Commands;
using Automa.Entities.Events;
using Automa.Entities.Internal;
using Automa.Entities.PerformanceTests.Model;
using Automa.Entities.Systems;

namespace Automa.Entities.PerformanceTests
{
    class EventVsStateComponentBenchmark : Benchmark
    {
        protected override void Prepare()
        {
            IterationCount = 1;
        }

        [Case("StateComponent")]
        private void TestStateComponent()
        {
            var context = ContextFactory.CreateEntitiesContext();
            EntityManager entityManager = context.GetManager<EntityManager>();
            SystemManager systemManager = context.GetManager<SystemManager>();
            var componentAddTestSystem = new ComponentAddTestSystem();
            systemManager.AddSystem(componentAddTestSystem);
            for (int i = 0; i < 10000; i++)
            {
                entityManager.CreateEntity(ComponentType.Create<Progress>());
            }
            for (int i = 0; i < 100; i++)
            {
                context.Update();
            }
            //Console.WriteLine("State: " + componentAddTestSystem.updatedSystems);
        }

        [Case("Event")]
        private void TestEvents()
        {
            var context = ContextFactory.CreateEntitiesContext();
            EntityManager entityManager = context.GetManager<EntityManager>();
            SystemManager systemManager = context.GetManager<SystemManager>();
            var eventTestSystem = new EventTestSystem();
            systemManager.AddSystem(eventTestSystem);
            for (int i = 0; i < 10000; i++)
            {
                entityManager.CreateEntity(ComponentType.Create<Progress>());
            }
            for (int i = 0; i < 100; i++)
            {
                context.Update();
            }
            //Console.WriteLine("Event: " + eventTestSystem.updatedSystems);
        }

        private class ComponentAddTestSystem : EntityUpdateSystem
        {
            [Inject]
            private EntityCommandBuffer<ResetProgressCommand> resetCommands;
            [Inject]
            private EntityCommandBuffer<CompleteProgressCommand> completeCommands;
            [Inject]
            private ProgressGroup group;
            [Inject]
            private ProgressCompletedGroup completedGroup;

            public int updatedSystems = 0;

            public override void OnSystemUpdate()
            {
                for (int i = 0; i < @group.Count; i++)
                {
                    ref var progress = ref @group.Progress[i];
                    progress.Value += 0.01f * i;
                    if (progress.Value > 1.0f)
                    {
                        completeCommands.Add(new CompleteProgressCommand(@group.Entities[i]));
                    }
                }
                for (int i = 0; i < completedGroup.Count; i++)
                {
                    ++updatedSystems;
                    completedGroup.Progress[i].Value = 0.0f;
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

        private class EventTestSystem : EntityUpdateSystem, IEntityEventListener<ProgressCompleted>
        {
            [Inject]
            private ProgressGroup group;

            public int updatedSystems = 0;

            public override void OnSystemUpdate()
            {
                for (int i = 0; i < @group.Count; i++)
                {
                    ref var progress = ref @group.Progress[i];
                    progress.Value += 0.01f * i;
                    if (progress.Value > 1.0f)
                    {
                        EventManager.Raise(@group.Entities[i], new ProgressCompleted());
                    }
                }
            }

            private class ProgressGroup : Group
            {
                public EntityCollection Entities;
                public ComponentCollection<Progress> Progress;
            }

            public void OnEvent(Entity source, ProgressCompleted eventInstance)
            {
                if (EntityManager.HasComponent<Progress>(source))
                {
                    EntityManager.SetComponent(source, new Progress(0));
                    ++updatedSystems;
                }
            }
        }

        private struct ProgressCompleted
        {
            
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
