using System;
using System.Linq;
using Automa.Entities.Debugging;

namespace Automa.Entities.Systems.Debugging
{
    public class SystemDebugInfo
    {
        public ISystem System { get; }
        public TimeSpan UpdateTime { get; set; }
        public GroupDebugInfo[] Groups { get; private set; }

        public SystemDebugInfo(ISystem system)
        {
            this.System = system;
        }

        internal virtual void OnAttachToContext(IContext context)
        {
            if (System is EntitySystem entitySystem)
            {
                Groups = entitySystem.groups.Select(group => new GroupDebugInfo(group)).ToArray();
            }
        }

        internal virtual void OnDetachFromContext(IContext context)
        {
        }
    }
}