using System;
using System.Collections.Generic;
using System.Text;
using Automa.Entities.Collections;
using Automa.Entities.Tests.Model;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Collections")]
    public class CollectionTests
    {

        [Test]
        public void IterationTest()
        {
            var contextEntityManager = new EntityManager();

            for (int i = 0; i < 2; i++)
            {
                var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>(), ComponentType.Create<ClassComponent>());
                contextEntityManager.SetComponent(entity, new ClassComponent(i));
                contextEntityManager.SetComponent(entity, new StructComponent(i));
            }
            for (int i = 0; i < 2; i++)
            {
                var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>());
                contextEntityManager.SetComponent(entity, new StructComponent(i));
            }
            for (int i = 0; i < 2; i++)
            {
                var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>(), ComponentType.Create<Struct2Component>());
                contextEntityManager.SetComponent(entity, new StructComponent(i));
            }

            var group = contextEntityManager.RegisterGroup(new EntityGroup());
            group.Update();

            for (int i = 0; i < group.Count; i++)
            {
                var t = group.Structures[i];
            }
        }

        private class EntityGroup : Group
        {
            public Collections.EntityCollection Entities;
            public ComponentCollection<StructComponent> Structures;
        }

    }
}
