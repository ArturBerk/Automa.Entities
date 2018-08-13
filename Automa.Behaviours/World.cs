using System;

namespace Automa.Behaviours
{
    public class World : IDisposable
    {
        public readonly EntityGroup Entities;
        public readonly IBehaviourGroup Behaviours;

        public World()
        {
            Entities = new EntityGroup();
            Behaviours = new BehaviourGroup(this, "Root");
        }

        public void Dispose()
        {
            Entities.Dispose();
        }
    }
}