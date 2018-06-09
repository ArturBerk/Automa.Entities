namespace Automa.Entities.Debugging
{
    public class EntityManagerDebugInfo
    {
        public EntityManagerDebugInfo(params GroupDebugInfo[] groups)
        {
            Groups = groups;
        }

        public GroupDebugInfo[] Groups { get; }
    }
}