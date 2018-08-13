using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Automa.Tasks.Tests
{
    [TestFixture]
    [Category("Tasks.BlockingQueue")]
    public class BlockingQueueTests
    {
        [Test]
        public void EnqueueTest()
        {
            BlockingQueue<int> queue = new BlockingQueue<int>();
            queue.Enqueue(1);
            Assert.AreEqual(1, queue.data[0]);
            Assert.AreEqual(1, queue.count);
        }

        [Test]
        public void DequeueTest()
        {
            BlockingQueue<int> queue = new BlockingQueue<int>();
            queue.Enqueue(1);
            var value = queue.WaitDequeue();
            Assert.AreEqual(1, value);
            Assert.AreEqual(0, queue.count);
        }

        [Test]
        public void GrowTest()
        {
            BlockingQueue<int> queue = new BlockingQueue<int>();
            for (int i = 0; i < 100; i++)
            {
                queue.Enqueue(i);
            }
            Assert.IsTrue(queue.data.Length > 100);
            Assert.AreEqual(100, queue.count);
        }

        [Test]
        public void ConcurrentTest()
        {
            BlockingQueue<int> queue = new BlockingQueue<int>();
            var sumTask = System.Threading.Tasks.Task.Run(() =>
            {
                var sum1 = 0;
                for (int i = 0; i < 10; i++)
                {
                    sum1 += queue.WaitDequeue();
                }
                return sum1;
            });
            Thread.Sleep(100);
            for (int i = 0; i < 10; i++)
            {
                queue.Enqueue(i);
            }
            sumTask.Wait(1000);
            Assert.AreEqual(45, sumTask.Result);
        }

        [Test]
        public void ConcurrentTest2()
        {
            BlockingQueue<int> queue = new BlockingQueue<int>();
            var sumTask = System.Threading.Tasks.Task.Run(() =>
            {
                Thread.Sleep(100);
                var sum1 = 0;
                for (int i = 0; i < 10; i++)
                {
                    sum1 += queue.WaitDequeue();
                }
                return sum1;
            });
            for (int i = 0; i < 10; i++)
            {
                queue.Enqueue(i);
            }
            sumTask.Wait(1000);
            Assert.AreEqual(45, sumTask.Result);
        }
    }
}
