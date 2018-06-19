using Automa.Entities.Internal;

namespace Automa.Entities.Tasks.Special
{
    public abstract class GroupForTask<TGroup> : ITaskSource where TGroup: Group
    {
        protected readonly TGroup Group;
        private readonly int batch;
        private readonly ArrayList<ITask> subTasks = new ArrayList<ITask>();

        protected GroupForTask(TGroup @group, int batch)
        {
            this.Group = @group;
            this.batch = batch;
        }

        public int Tasks(out ITask[] tasks)
        {
            var startIndex = 0;
            Expand(Group.Count / batch + 1);
            var batchIndex = 0;
            while (startIndex < Group.Count)
            {
                var endIndex = startIndex + batch;
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
