namespace Automa.Behaviours
{
    public struct EntityReference
    {
        public static readonly EntityReference Null = new EntityReference(0, -1, 0);

        internal ushort TypeIndex;
        internal byte Version;
        internal int Index;

        internal EntityReference(ushort typeIndex, int index, byte version)
        {
            TypeIndex = typeIndex;
            Index = index;
            Version = version;
        }

        public bool IsNull => Index < 0;
    }
}