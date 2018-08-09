namespace Automa.Behaviours
{
    public interface IBehaviour<T>
    {
        void Apply(EntityCollection<T> entities);
    }
}