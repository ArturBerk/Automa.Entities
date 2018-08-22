using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Automa.Common;

namespace Automa.Behaviours
{
    public struct EntityType
    {
        public bool Equals(EntityType other)
        {
            return TypeId == other.TypeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EntityType type && Equals(type);
        }

        public override int GetHashCode()
        {
            return TypeId.GetHashCode();
        }

        public readonly ushort TypeId;

        private EntityType(ushort typeId)
        {
            TypeId = typeId;
        }

        public static EntityType Create<T>()
        {
            return new EntityType(EntityTypeManager.GetTypeIndex<T>());
        }

        public static EntityType Create(Type type)
        {
            return new EntityType(EntityTypeManager.GetTypeIndex(type));
        }

        public static implicit operator Type(EntityType type)
        {
            return EntityTypeManager.GetTypeFromIndex(type.TypeId);
        }

        public static bool operator ==(EntityType c1, EntityType c2)
        {
            return c1.TypeId == c2.TypeId;
        }

        public static bool operator !=(EntityType c1, EntityType c2)
        {
            return c1.TypeId != c2.TypeId;
        }

        public override string ToString()
        {
            return $"{TypeId} ({EntityTypeManager.GetTypeFromIndex(TypeId).Name})";
        }
    }

    internal static class EntityTypeManager
    {
        private static readonly Dictionary<Type, ushort> indicesByTypes = new Dictionary<Type, ushort>();
        private static ArrayList<Type> types = new ArrayList<Type>(4);

        public static int TypeCount => types.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetTypeIndex<T>()
        {
            var result = StaticTypeIndex<T>.TypeIndex;
            if (result != 0) return result;
            result = GetTypeIndex(typeof(T));
            StaticTypeIndex<T>.TypeIndex = result;
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
            var newId = (ushort)indicesByTypes.Count;
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
            throw new ArgumentException($"Entity type with id={typeTypeId} not registred");
        }
    }

    internal static class StaticTypeIndex<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static ushort TypeIndex;
    }
}
