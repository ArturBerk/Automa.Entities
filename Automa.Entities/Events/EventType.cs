using System;
using System.Collections.Generic;
using Automa.Entities.Internal;

namespace Automa.Entities.Events
{
    internal struct EventType
    {
        public bool Equals(EventType other)
        {
            return TypeId == other.TypeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EventType && Equals((EventType) obj);
        }

        public override int GetHashCode()
        {
            return TypeId.GetHashCode();
        }

        public ushort TypeId;

        private EventType(Type type)
        {
            TypeId = EventTypeManager.GetTypeIndex(type);
        }

        public static EventType Create<T>()
        {
            return new EventType(typeof(T));
        }

        public static implicit operator EventType(Type type)
        {
            return new EventType(type);
        }

        public static implicit operator Type(EventType type)
        {
            return EventTypeManager.GetTypeFromIndex(type.TypeId);
        }

        public static bool operator ==(EventType c1, EventType c2)
        {
            return c1.TypeId == c2.TypeId;
        }

        public static bool operator !=(EventType c1, EventType c2)
        {
            return c1.TypeId != c2.TypeId;
        }

        public override string ToString()
        {
            return $"{TypeId} ({EventTypeManager.GetTypeFromIndex(TypeId)})";
        }
    }

    internal static class EventTypeManager
    {
        private static readonly Dictionary<Type, ushort> indicesByTypes = new Dictionary<Type, ushort>();
        private static readonly ArrayList<Type> types = new ArrayList<Type>();

        static EventTypeManager()
        {
        }

        public static int TypeCount => types.Count;

        public static ushort GetTypeIndex<T>()
        {
            var result = StaticEventIndex<T>.TypeIndex;
            if (result != 0) return result;
            result = GetTypeIndex(typeof(T));
            StaticEventIndex<T>.TypeIndex = result;
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

        public static Type GetTypeFromIndex(int typeTypeId)
        {
            if (types.Count > typeTypeId)
            {
                return types[typeTypeId];
            }
            throw new ArgumentException($"Type with id={typeTypeId} not registred");
        }
    }

    internal static class StaticEventIndex<T>
    {
        public static ushort TypeIndex;
    }
}