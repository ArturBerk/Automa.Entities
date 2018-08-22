namespace Automa.Behaviours
{
    public struct EntityReference
    {
        internal uint TypeIndex;
        internal int Index;
        internal byte Version;

        internal EntityReference(uint typeIndex, int index, byte version)
        {
            TypeIndex = typeIndex;
            Index = index;
            Version = version;
        }
    }
}