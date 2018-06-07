using System;
using System.Collections.Generic;
using System.Text;
using Automa.Entities.Attributes;
using Automa.Entities.Collections;
using Automa.Entities.Tests.Model;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Groups")]
    public class GroupsTests
    {
        [Test]
        public void IsArchetypeMatching()
        {
            ComponentType[] types1 = new []
            {
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<StructComponent>(),
            };

            var archetype = new Archetype(
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<ClassComponent>());

            var testGroup = new EntityGroup();
            testGroup.includedTypes = types1;
            Assert.IsTrue(testGroup.IsArchetypeMatching(ref archetype));
        }

        [Test]
        public void IsArchetypeMatchingExclude()
        {
            ComponentType[] types1 = new[]
            {
                ComponentType.Create<ClassComponent>(),
            };
            ComponentType[] types2 = new[]
            {
                ComponentType.Create<StructComponent>(),
            };

            var archetype = new Archetype(
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<ClassComponent>());

            var testGroup = new EntityGroup();
            testGroup.includedTypes = types1;
            testGroup.excludedTypes = types2;
            Assert.IsFalse(testGroup.IsArchetypeMatching(ref archetype));
        }

        [Test]
        public void IsArchetypeMatchingNotMatching()
        {
            ComponentType[] types1 = new[]
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<ClassComponent>()
            };

            var archetype = new Archetype(
                ComponentType.Create<StructComponent>());

            var testGroup = new EntityGroup();
            testGroup.includedTypes = types1;
            Assert.IsFalse(testGroup.IsArchetypeMatching(ref archetype));
        }

        [Test]
        public void IsArchetypeMatchingPartlyMatching()
        {
            ComponentType[] types1 = new[]
            {
                ComponentType.Create<StructComponent>()
            };

            var archetype = new Archetype(
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<ClassComponent>());

            var testGroup = new EntityGroup();
            testGroup.includedTypes = types1;
            Assert.IsTrue(testGroup.IsArchetypeMatching(ref archetype));
        }

        [Test]
        public void GroupSelect()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(typeof(StructComponent));
            contextEntityManager.AddComponent(entity, new ClassComponent(1));
            contextEntityManager.SetComponent(entity, new StructComponent(2));

            entity = contextEntityManager.CreateEntity(typeof(ClassComponent));
            contextEntityManager.SetComponent(entity, new ClassComponent(3));
            contextEntityManager.AddComponent(entity, new StructComponent(4));

            var group = contextEntityManager.RegisterGroup(new EntityGroup());

            Assert.NotNull(group.Entities);
            Assert.NotNull(group.Classes);
            Assert.NotNull(group.Structures);

            Assert.AreEqual(2, group.Entities.CalculatedCount);
            Assert.AreEqual(0, group.Entities[0].Id);
            Assert.AreEqual(1, group.Entities[1].Id);

            Assert.AreEqual(1, group.Classes[0].Value);
            Assert.AreEqual(2, group.Structures[0].Value);

            Assert.AreEqual(3, group.Classes[1].Value);
            Assert.AreEqual(4, group.Structures[1].Value);
        }

        [Test]
        public void ExcludeComponent()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(typeof(StructComponent));
            contextEntityManager.AddComponent(entity, new ClassComponent(1));
            
            var group = contextEntityManager.RegisterGroup(new ExcludeGroup());
            Assert.AreEqual(0, group.Entities.CalculatedCount);
        }

        [Test]
        public void GroupWithLength()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(typeof(StructComponent));
            contextEntityManager.AddComponent(entity, new ClassComponent(1));

            var group = contextEntityManager.RegisterGroup(new EntityGroup());
            group.UpdateLength();
            Assert.AreEqual(1, group.Length);
        }

        [ExcludeComponent(typeof(ClassComponent))]
        private class ExcludeGroup : Group
        {
            public Collections.Entities Entities;
            public Collection<StructComponent> Structures;
        }

        private class EntityGroup : GroupWithLength
        {
            public Collections.Entities Entities;
            public Collection<ClassComponent> Classes;
            public Collection<StructComponent> Structures;
        }
    }
}
