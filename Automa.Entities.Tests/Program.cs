using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Automa.Entities.Tasks;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    class Program
    {
        public static void Main()
        {
            EntityTests e = new EntityTests();
            e.AddComponentTest();


//            var context = ContextFactory.CreateEntitiesContext();
//            var taskManager = context.GetManager<TaskManager>();
////            var tasks = new ITask[] { new Task1(), new Task1(), new Task1(), new Task1() };
////            for (int i = 0; i < 1000; i++)
////            {
////                for (int j = 0; j < tasks.Length; j++)
////                {
////                    taskManager.Schedule(tasks[j]);
////                }
////                taskManager.Wait();
////            }
////            taskManager.Schedule(new TaskFor(), 10000, 64);
////            taskManager.Wait();
//            Console.WriteLine("Completed");
//            Console.ReadKey();
//            context.Dispose();
        }

        private class Task1 : ITask
        {
            public float sum;

            public void Execute()
            {
                sum = 0;
                for (int i = 0; i < 100000; i++)
                {
                    sum += (float)Math.Sin(i);
                }
            }
        }
    }
}
