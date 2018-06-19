using System;
using System.Threading;
using Automa.Entities.Tasks;
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
