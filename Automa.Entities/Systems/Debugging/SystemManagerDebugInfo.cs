using System.Diagnostics;

namespace Automa.Entities.Systems.Debugging
{
    public class SystemManagerDebugInfo
    {
        public SystemDebugInfo[] Systems { get; }
        internal readonly Stopwatch Stopwatch;

        public SystemManagerDebugInfo(params SystemDebugInfo[] systems)
        {
            Systems = systems;
            Stopwatch = new Stopwatch();
        }
    }
}
