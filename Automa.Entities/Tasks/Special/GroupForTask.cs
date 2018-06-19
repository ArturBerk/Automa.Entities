using System;
using Automa.Entities.Internal;

namespace Automa.Entities.Tasks.Special
{
    public abstract class GroupForTask<TGroup> : ITaskSource where TGroup: Group
    {
        protected readonly TGroup Group;
        private readonly int batchGroupCount;
        private readonly ArrayList<ITask> subTasks = new ArrayList<ITask>();

        protected GroupForTask(TGroup @group, int batchGroupCount)
        {
            this.Group = @group;
            this.batchGroupCount = Math.Max(Math.Min(batchGroupCount, 16), 1);
        }

        public int Tasks(out ITask[] tasks)
        {
            var startIndex = 0;
            Expand(batchGroupCount);
            var batchIndex = 0;
            var batchSize = (int)Math.Ceiling((float)Group.Count / batchGroupCount);
            while (startIndex < Group.Count)
            {
                var endIndex = startIndex + batchSize;
                if (endIndex >= Group.Count) endIndex = Group.Count;
                var task = (SubTask)subTasks[batchIndex++];
                task.startIndex = startIndex;
                task.endIndex = endIndex;
                startIndex = endIndex;
            }
            tasks = subTasks.Buffer;
            return batchIndex;
        }

        public void Expand(int size)
        {
            if (size > subTasks.Count)
            {
                var was = subTasks.Count;
                for (int i = was; i < size; i++)
                {
                    subTasks.Add(new SubTask(this));
                }
            }
        }

        public abstract void Execute(int index);

        private class SubTask : ITask
        {
            private readonly GroupForTask<TGroup> task;
            public int startIndex;
            public int endIndex;

            public SubTask(GroupForTask<TGroup> task)
            {
                this.task = task;
            }

            public void Execute()
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    task.Execute(i);
                }
            }
        }
    }
}
