using System;
using Automa.Entities.Internal;

namespace Automa.Entities.Tasks.Special
{
    public abstract class GroupIteratorTask<TGroup> : ITaskSource where TGroup: Group
    {
        protected readonly TGroup Group;
        private readonly int batchGroupCount;
        private readonly ArrayList<ITask> subTasks = new ArrayList<ITask>();

        protected GroupIteratorTask(TGroup @group, int batchGroupCount)
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
                task.Iterator = Group.GetIterator(startIndex, endIndex);
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

        public abstract void Execute(Group.EntityIndex index);

        private class SubTask : ITask
        {
            private readonly GroupIteratorTask<TGroup> task;
            public Group.Iterator Iterator;

            public SubTask(GroupIteratorTask<TGroup> task)
            {
                this.task = task;
            }

            public void Execute()
            {
                while (Iterator.MoveNext())
                {
                    task.Execute(Iterator.Index);
                }
            }
        }
    }
}
