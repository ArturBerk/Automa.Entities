using System;
using System.Linq;

namespace Automa.Entities.Debugging
{
    public class GroupDebugInfo
    {
        public readonly Group Group;
        public int Count => Group.Count;
        public Type[] IncludeTypes;
        public Type[] ExcludeTypes;

        public GroupDebugInfo(Group @group)
        {
            Group = @group;
            IncludeTypes = Group.includedTypes?.Select(type => (Type)type).ToArray() ?? Array.Empty<Type>();
            ExcludeTypes = Group.excludedTypes?.Select(type => (Type)type).ToArray() ?? Array.Empty<Type>();
        }
    }
}