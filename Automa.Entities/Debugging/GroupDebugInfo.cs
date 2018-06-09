using System;
using System.Collections.Generic;
using System.Linq;

namespace Automa.Entities.Debugging
{
    public class GroupDebugInfo
    {
        public readonly Group Group;
        public int Count => Group.Count;
        public IEnumerable<Type> IncludeTypes => Group.includedTypes.Select(type => (Type) type);
        public IEnumerable<Type> ExcludeTypes => Group.excludedTypes?.Select(type => (Type) type) ?? Enumerable.Empty<Type>();

        public GroupDebugInfo(Group @group)
        {
            Group = @group;
        }
    }
}