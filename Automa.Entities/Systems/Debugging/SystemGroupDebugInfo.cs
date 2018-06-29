using System.Diagnostics;
using System.Linq;

namespace Automa.Entities.Systems.Debugging
{
    public class SystemGroupDebugInfo : SystemDebugInfo
    {
        public SystemGroupDebugInfo(SystemGroup system) : base(system)
        {
            system.debug = this;
            SystemGroup = system;
        }

        public SystemGroup SystemGroup { get; }
        public SystemDebugInfo[] Systems { get; private set; }
        internal readonly Stopwatch Stopwatch = new Stopwatch();

        internal override void OnAttachToContext(IContext context)
        {
            Systems = SystemGroup.Systems.Select(system => system is SystemGroup systemGroup
                ? new SystemGroupDebugInfo(systemGroup)
                : new SystemDebugInfo(system)).ToArray();
            foreach (var systemDebugInfo in Systems)
            {
                systemDebugInfo.OnAttachToContext(context);
            }
        }

        internal override void OnDetachFromContext(IContext context)
        {
        }
    }
}