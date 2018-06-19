namespace Automa.Entities.Tasks
{
    public interface ITaskSource
    {
         int Tasks(out ITask[] tasks);
    }
}
