using System;
using System.Threading;
using Automa.Entities.Collections;
using Automa.Entities.Tasks;
using Automa.Entities.Tasks.Special;
using Automa.Entities.Tests.Model;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Tasks")]
    public class TaskTests
    {

        [Test]
        public void ExecuteTasks()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var taskManager = context.GetManager<TaskManager>();
            var task1 = new TaskSource();
            for (int i = 0; i < 10; i++)
            {
                taskManager.ScheduleFrom(task1);
                taskManager.Wait();
//                Assert.AreEqual(1000, task1.sum);
            }
            context.Dispose();
        }

        [Test]
        public void GroupIteratorTask()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var entityManager = context.GetManager<EntityManager>();
            var taskManager = context.GetManager<TaskManager>();

            for (int i = 0; i < 10000; i++)
            {
                entityManager.CreateEntity(typeof(FloatComponent));
            }

            var group = entityManager.RegisterGroup(new FloatGroup());
            var task = new FloatTask(group, 500);
            for (int i = 0; i < 100; i++)
            {
                taskManager.ScheduleFrom(task);
                Thread.Sleep(10);
                taskManager.Wait();
                Thread.Sleep(10);
            }
            context.Dispose();
        }

        private class FloatTask : GroupIteratorTask<FloatGroup>
        {
            public FloatTask(FloatGroup @group, int batch) : base(@group, batch)
            {
            }

            public override void Execute(Group.EntityIndex index)
            {
                ref var floatComponent = ref Group.Floats[index];
                floatComponent.Value = floatComponent.Value * MathF.Cos(index.Index);
            }
        }

        private class FloatGroup : Group
        {
            public ComponentCollection<FloatComponent> Floats;
        }

        private class TaskSource : ITaskSource
        {
            private ITask[] tasks = new ITask[] { new Task1(), new Task1(), new Task1(), new Task1(), new Task1() };

            public int Tasks(out ITask[] tasks)
            {
                tasks = this.tasks;
                return this.tasks.Length;
            }
        }

        private class Task1 : ITask
        {
            public float sum;

            public void Execute()
            {
                for (int i = 0; i < 100000; i++)
                {
                    sum += (float)Math.Sin(i) * (float)Math.Cos(i);
                }
            }
        }

    }
}
