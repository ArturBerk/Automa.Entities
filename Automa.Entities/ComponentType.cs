using System;
using System.Collections.Generic;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    public struct ComponentType
    {
        public bool Equals(ComponentType other)
        {
            return TypeId == other.TypeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ComponentType && Equals((ComponentType) obj);
        }

        public override int GetHashCode()
        {
            return TypeId.GetHashCode();
        }

        public ushort TypeId;

        private ComponentType(Type type)
        {
            TypeId = ComponentTypeManager.GetTypeIndex(type);
        }

        public static ComponentType Create<T>()
        {
            return new ComponentType(typeof(T));
        }

        public static implicit operator ComponentType(Type type)
        {
            return new ComponentType(type);
        }

        public static implicit operator Type(ComponentType type)
        {
            return ComponentTypeManager.GetTypeFromIndex(type.TypeId);
        }

        public static bool operator ==(ComponentType c1, ComponentType c2)
        {
            return c1.TypeId == c2.TypeId;
        }

        public static bool operator !=(ComponentType c1, ComponentType c2)
        {
            return c1.TypeId != c2.TypeId;
        }

        public override string ToString()
        {
            return $"{TypeId} ({ComponentTypeManager.GetTypeFromIndex(TypeId).Name})";
        }
    }

    internal static class ComponentTypeManager
    {
        private static readonly Dictionary<Type, ushort> indicesByTypes = new Dictionary<Type, ushort>();
        private static readonly ArrayList<Type> types = new ArrayList<Type>();

        static ComponentTypeManager()
        {
            RegisterType(typeof(Entity));
        }

        public static int TypeCount => types.Count;

        public static ushort GetTypeIndex<T>()
        {
            var result = StaticTypeIndex<T>.typeIndex;
            if (result != 0) return result;
            result = GetTypeIndex(typeof(T));
            StaticTypeIndex<T>.typeIndex = result;
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
            var newId = (ushort) indicesByTypes.Count;
            indicesByTypes.Add(type, newId);
            types.SetAt(newId, type);
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

    internal static class StaticTypeIndex<T>
    {
        public static ushort typeIndex;
    }
}