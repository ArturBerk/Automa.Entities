namespace Automa.Entities
{
    public struct Entity
    {
        public bool Equals(Entity other)
        {
            return Id == other.Id && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Entity && Equals((Entity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ Version;
            }
        }

        public static readonly Entity Null = new Entity(0,0);

        public readonly int Id;
        public readonly int Version;

        public Entity(int id, int version)
        {
            Id = id;
            Version = version;
        }

        public static bool operator ==(Entity e1, Entity e2)
        {
            return e1.Version == e2.Version && e1.Id == e2.Id;
        }

        public static bool operator !=(Entity e1, Entity e2)
        {
            return e1.Version != e2.Version || e1.Id != e2.Id;
        }
    }
}