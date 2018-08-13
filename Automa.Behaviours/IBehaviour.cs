namespace Automa.Behaviours
{
    public interface IBehaviour
    {
        void OnAttach(World world);
        void Apply();
    }
}