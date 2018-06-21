using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automa.Entities.Internal;

namespace Automa.Entities.Systems
{
    public static class SystemExtensions
    {
        public static void AddSystems(this SystemManager systemManager, IEnumerable<ISystem> systems)
        {
            foreach (var system in systems)
            {
                systemManager.AddSystem(system);
            }
        }

        public static IEnumerable<ISystem> GetAllSystems(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type => typeof(ISystem).IsAssignableFrom(type))
                .Select(type => (ISystem)Activator.CreateInstance(type));
        }

        public static IEnumerable<ISystem> GetAllSystems(this Assembly assembly, Predicate<Type> filter)
        {
            return assembly.GetTypes()
                .Where(type => typeof(ISystem).IsAssignableFrom(type) && filter(type))
                .Select(type => (ISystem)Activator.CreateInstance(type)); 
        }
    }
}
