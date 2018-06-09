namespace Automa.Entities.Systems.Debugging
{
    public class SystemManagerDebugInfo
    {
        public SystemDebugInfo[] Systems { get; }

        public SystemManagerDebugInfo(params SystemDebugInfo[] systems)
        {
            Systems = systems;
        }
    }
}
