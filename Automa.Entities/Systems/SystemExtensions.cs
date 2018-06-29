using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Automa.Entities.Systems
{
    public static class SystemExtensions
    {
        public static void AddSystems(this SystemManager systemManager, IEnumerable<ISystem> systems)
        {
            SystemTreeBuilder treeBuilder = new SystemTreeBuilder();
            treeBuilder.AddSystems(systems);
            treeBuilder.Build(systemManager);
        }

        public static IEnumerable<ISystem> GetAllSystems(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type => typeof(ISystem).IsAssignableFrom(type) && !typeof(SystemGroup).IsAssignableFrom(type))
                .Select(type => (ISystem)Activator.CreateInstance(type));
        }

        public static IEnumerable<ISystem> GetAllSystems(this Assembly assembly, Predicate<Type> filter)
        {
            return assembly.GetTypes()
                .Where(type => typeof(ISystem).IsAssignableFrom(type) && !typeof(SystemGroup).IsAssignableFrom(type) && filter(type))
                .Select(type => (ISystem)Activator.CreateInstance(type)); 
        }
    }
}
