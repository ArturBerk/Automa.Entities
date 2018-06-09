using System;

namespace Automa.Entities.Systems.Debugging
{
    public class SystemDebugInfo
    {
        public ISystem System { get; }
        public TimeSpan UpdateTime { get; set; }

        public SystemDebugInfo(ISystem system)
        {
            this.System = system;
        }
    }
}