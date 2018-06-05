using System;
using System.Collections.Generic;
using Automa.Collections;

namespace Automa.Entities
{
    public struct ComponentType
    {
        public ushort TypeId;

        private ComponentType(Type type)
        {
            TypeId = ComponentTypeManager.GetTypeIndex(type);
        }

        public static implicit operator ComponentType(Type type)
        {
            return new ComponentType(type);
        }

        public static implicit operator Type(ComponentType type)
        {
            return ComponentTypeManager.GetTypeFromIndex(type.TypeId);
        }

    }

    internal static class ComponentTypeManager
    {
        private static readonly Dictionary<Type, ushort> indicesByTypes = new Dictionary<Type, ushort>();
        private static readonly FastList<Type> types = new FastList<Type>();

        public static int TypeCount => types.Count;

        public static ushort GetTypeIndex<T>() where T : IComponent
        {
            var result = StaticIndex<T>.typeIndex;
            if (result != 0) return result;
            result = GetTypeIndex(typeof(T));
            StaticIndex<T>.typeIndex = result;
            return result;
        }

        public static ushort GetTypeIndex(Type type)
        {

            if (indicesByTypes.TryGetValue(type, out var p))
            {
                return p;
            }
            return RegisterType(type);
        }

        private static ushort RegisterType(Type type)
        {
            var newId = (ushort)(indicesByTypes.Count + 1);
            indicesByTypes.Add(type, newId);
            types.Insert(newId, type);
            return newId;
        }

        public static Type GetTypeFromIndex(ushort typeTypeId)
        {
            if (types.Count > typeTypeId)
            {
                return types[typeTypeId];
            }
            throw new ArgumentException($"Type with id={typeTypeId} not registred");
        }
    }

    internal static class StaticIndex<T> where T : IComponent
    {
        public static ushort typeIndex;
    }
}
