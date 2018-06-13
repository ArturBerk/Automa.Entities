using Automa.Entities.Internal;

namespace Automa.Entities.Tasks.Special
{
    public abstract class GroupIteratorTask<TGroup> : ITaskSource where TGroup: Group
    {
        protected readonly TGroup Group;
        private readonly int batch;
        private readonly ArrayList<ITask> subTasks = new ArrayList<ITask>();

        protected GroupIteratorTask(TGroup @group, int batch)
        {
            this.Group = @group;
            this.batch = batch;
        }

        public ITask[] Tasks()
        {
            subTasks.FastClear();
            var startIndex = 0;
            while (startIndex < Group.Count)
            {
                var endIndex = startIndex + batch;
                if (endIndex >= Group.Count) endIndex = Group.Count;
                startIndex = endIndex;
                subTasks.Add(new SubTask(this, Group.GetIterator(startIndex, endIndex)));
            }
            return subTasks.Buffer;
        }

        public abstract void Execute(Group.EntityIndex index);

        private struct SubTask : ITask
        {
            private readonly GroupIteratorTask<TGroup> task;
            private Group.Iterator iterator;

            public SubTask(GroupIteratorTask<TGroup> task, Group.Iterator iterator)
            {
                this.task = task;
                this.iterator = iterator;
            }

            public void Execute()
            {
                while (iterator.MoveNext())
                {
                    task.Execute(iterator.CurrentIndex);
                }
            }
        }
    }
}
