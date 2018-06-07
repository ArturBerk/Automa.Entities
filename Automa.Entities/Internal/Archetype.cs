using System;
using Automa.Entities.Internal;

namespace Automa.Entities
{
    internal struct Archetype
    {
        public readonly ComponentType[] Types;
        internal readonly uint Index;

        public Archetype(params ComponentType[] types)
        {
            Types = types;
            Array.Sort(Types, (c1, c2) => c1.TypeId - c2.TypeId);
            Index = CalculateIndex(types, types.Length);
        }

        public Archetype(uint index, ComponentType[] types, int typeCount)
        {
            Types = new ComponentType[typeCount];
            Array.Copy(types, Types, typeCount);
            Array.Sort(Types, (c1, c2) => c1.TypeId - c2.TypeId);
            Index = index;
        }

        public static uint CalculateIndex(ComponentType[] types, int count)
        {
            return HashUtility.Fletcher32(types, count);
        }
    }
}