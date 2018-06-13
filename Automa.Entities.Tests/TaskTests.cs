using System;
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
            var task1 = new Task1();
            for (int i = 0; i < 1000; i++)
            {
                task1.sum = 0;
                taskManager.Schedule(task1);
                Assert.AreEqual(1000, task1.sum);
            }
        }

        private class Task1 : ITask
        {
            public int sum;

            public void Execute()
            {
                for (int i = 0; i < 1000; i++)
                {
                    sum += 1;
                }
            }
        }

    }
}
