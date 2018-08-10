using System.Threading;

namespace Automa.Tasks
{
    public class BlockingQueue<T>
    {
        private readonly object sync = new object();
        internal T[] data;
        private int capacity;
        private int head = -1;
        internal int count;
        private readonly ManualResetEventSlim enqueuedEvent = new ManualResetEventSlim(false);

        public BlockingQueue(int initialCapacity = 10)
        {
            data = new T[initialCapacity];
            capacity = data.Length;
        }

        public void Enqueue(T value)
        {
            try
            {
                Monitor.Enter(sync);
                count = count + 1;
                if (count > capacity)
                {
                    GrowCapacity();
                }
                head = (head + 1) % capacity;
                data[head] = value;
            }
            finally
            {
                Monitor.Exit(sync);
            }
            enqueuedEvent.Set();
        }

        private void GrowCapacity()
        {
            var newCapacity = capacity < 100 ? (int)(capacity * 1.5) : capacity + 100;
            var newArray = new T[newCapacity];
            data.CopyTo(newArray, 0);
            data = newArray;
            capacity = newCapacity;
        }

        public T WaitDequeue()
        {
            try
            {
                Monitor.Enter(sync);
                if (count > 0)
                {
                    --count;
                    return data[(head - count) % capacity];
                }
            }
            finally
            {
                Monitor.Exit(sync);
            }
            enqueuedEvent.Wait();
            try
            {
                Monitor.Enter(sync);
                --count;
                return data[(head - count) % capacity];
            }
            finally
            {
                Monitor.Exit(sync);
            }
        }

    }
}
