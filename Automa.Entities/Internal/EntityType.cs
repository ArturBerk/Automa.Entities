using System;
using System.Linq;

namespace Automa.Entities.Internal
{
    internal struct EntityType
    {
        public readonly ComponentType[] Types;
        internal readonly uint Hash;

        public EntityType(params ComponentType[] types)
        {
            Types = types;
            Array.Sort(Types, (c1, c2) => c1.TypeId - c2.TypeId);
            Hash = CalculateHash(types, types.Length);
        }

        public EntityType(uint hash, ComponentType[] types, int typeCount)
        {
            Types = new ComponentType[typeCount];
            Array.Copy(types, Types, typeCount);
            Array.Sort(Types, (c1, c2) => c1.TypeId - c2.TypeId);
            Hash = hash;
        }

        public static uint CalculateHash(ComponentType[] types, int count)
        {
            return HashUtility.Fletcher32(types, count);
        }

        public override string ToString()
        {
            return string.Join(", ", Types.Select(type => type.ToString()));
        }
    }
}