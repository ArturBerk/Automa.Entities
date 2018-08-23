using System.Threading;

namespace Automa.Tasks
{
    public class BlockingQueueSlim<T>
    {
        internal T[] data;
        private int capacity;
        private int head = -1;
        internal int count;
        private readonly SemaphoreSlim free = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim enqueuedEvent = new ManualResetEventSlim(false);

        public BlockingQueueSlim(int initialCapacity = 10)
        {
            data = new T[initialCapacity];
            capacity = data.Length;
        }

        public void Enqueue(T value)
        {
            try
            {
                free.Wait();
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
                free.Release();
                enqueuedEvent.Set();
            }
        }

        private void GrowCapacity()
        {
            var newCapacity = capacity < 100 ? (int)(capacity * 1.5) : capacity + 100;
            var newArray = new T[newCapacity];
            data.CopyTo(newArray, 0);
            data = newArray;
            capacity = newCapacity;
        }

        public T Dequeue()
        {
            try
            {
                free.Wait();
                if (count > 0)
                {
                    --count;
                    return data[(head - count + capacity) % capacity];
                }
            }
            finally
            {
                free.Release();
            }
            enqueuedEvent.Wait();
            enqueuedEvent.Reset();
            try
            {
                free.Wait();
                --count;
                return data[(head - count + capacity) % capacity];
            }
            finally
            {
                free.Release();
            }
        }

    }
}
